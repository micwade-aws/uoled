/*******************************************************************************
 * File: uOLED.cs
 * Poet: Michael J. Wade
 * Purpose: Class library used for serial communication with the uOLED-128-G1.  
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace uOLED_Interface {
    public class uOLED : SerialPort {
        
        #region vars                
        public enum FONTSIZE { SMALL = 0x0, MEDIUM = 0x01, LARGE = 0x02 };
        public enum COLORMODE { BIT8= 0x08, BIT16 = 0x10 };
        public enum TEXTTRANSPARENCY { TRANSPARENT = 0x0, OPAQUE = 0x01 };
        public enum PENSIZE { SOLID = 0x0, WIRE = 0x01 };
        public enum MODE { DISPLAY = 0x01, CONTRAST = 0x02, POWER = 0x03 };
        const byte OFF = 0x00;
        const byte ON  = 0x01;
        #endregion

        #region ctors        
        public uOLED(string in_portName, int in_baudRate) {
            try {
                // Connection parameters
                PortName = in_portName;
                BaudRate = in_baudRate;
                Parity = Parity.None;
                DataBits = 8;
                StopBits = StopBits.One;                
                Handshake = Handshake.None;

                // Method for receiving the data
                DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);

                // Timeout the writes
                WriteTimeout = 500; // 5 seconds

                // Open the port
                Open();                
                                
                // Signal to auto-detect the baud rate
                Write("U");
                ReadSync();
                
                // Turn on the power and up the contrast
                DisplayControl(MODE.POWER, ON);
                DisplayControl(MODE.DISPLAY, ON);
                DisplayControl(MODE.CONTRAST, 15); //max

                // Erase the screen
                EraseScreen();                

            } catch (Exception ex) {
                Console.Write(ex.ToString());
            }
        }
        #endregion

        #region commands
                        
        public void AddUserBitmappedCharacter(byte in_charNum, byte[] in_data) {
            // Prepare the command
            byte[] command = new byte[10];
            command[0] = 0x41; //A
            command[1] = in_charNum;

            // Add the data
            in_data.CopyTo(command,2);

            // Send it to the card
            Write(command, 0, 10);
            ReadSync();
        }
        
        public void SetBackgroundColor(ushort in_color) {
            // Get the converted color values
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x42,       // cmd = B
                                color[1],   // msb
                                color[0]    // lsb
                                }, 0, 3);
            ReadSync();
        }

        // TODO: No yet implemented
        public void PlaceTextButton() { }
        

        public void DrawCircle(byte in_x, byte in_y, 
                               byte in_rad, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x43,       // cmd = C
                                in_x,       // x
                                in_y,       // y
                                in_rad,     // radius
                                color[1],   // msb
                                color[0]    // lsb
                             }, 0, 6);
            ReadSync();
        }

        // TODO: No yet implemented
        public void BitmapCopy() { }

        public void DisplayUserBitmappedCharacter(byte in_charNum, byte in_x, 
                                                  byte in_y, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x44,       // cmd = D
                                in_charNum, // char#
                                in_x,       // x
                                in_y,       // y
                                color[1],   // msb    
                                color[0]    // lsb
                             }, 0, 6);
            ReadSync();
        }

        public void EraseScreen() {
            Write("E");
            ReadSync();
        }

        public void SetFontSize(byte in_size) {
            // Check if the font size is too large.
            if (in_size > 0x02) {
                throw new Exception("Error: Font size is too large, must be under 0x02");
            }

            Write(new byte[] {  0x46,       // cmd = F
                                in_size},   // size
                                0, 2);
            ReadSync();
        }

        public void DrawTriangle(byte in_x1, byte in_y1,
                                    byte in_x2, byte in_y2,
                                    byte in_x3, byte in_y3, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x47,       // cmd = G
                                in_x1,      // x1
                                in_y1,      // y1 
                                in_x2,      // x2 
                                in_y2,      // y2
                                in_x3,      // x3
                                in_y3,      // y3
                                color[1],   // msb    
                                color[0]    // lsb
                             }, 0, 9);
            ReadSync();
        }

        public void DrawPolygon(byte[] in_vertices, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            int cmdLength = 1 + 1 + in_vertices.Length + 2;
            byte[] cmd = new byte[cmdLength];

            // Load the command byte buffer
            cmd[0] = 0x67;                                  // cmd = g
            cmd[1] = Convert.ToByte(in_vertices.Length);    // numVertices
            for (int i = 2; i < in_vertices.Length+2; ++i) {// vertices
                cmd[i] = in_vertices[i - 2];                
            }
            cmd[cmd.Length - 2] = color[1];                 // msb
            cmd[cmd.Length - 1] = color[0];                 // msb

            Write(cmd, 0, cmdLength);
            ReadSync();
        }

        public void DrawImage(byte in_x, byte in_y,
                              byte in_width, byte in_height, 
                              COLORMODE in_colorMode, byte[] in_pixels) {
            // This needs to be REALLY fast, so I'll break it into two writes
            // That way I can skip the usual copy step of in_pixels into the 
            // command buffer before sending it out.
            Write(new byte[] {  0x49,       // cmd = I
                                in_x,       // Image horizontal start position
                                in_y,       // Image vertical start position
                                in_width,   // width
                                in_height,  // height
                                Convert.ToByte(in_colorMode) // 8bit or 16bit
                             }, 0, 6);
            Write(in_pixels, 0, in_pixels.Length); // pixel data
            
            ReadSync();
        }

        public void DrawLine(byte in_x1, byte in_y1, 
                             byte in_x2, byte in_y2, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x4C,       // cmd = L
                                in_x1,      // x1
                                in_y1,      // y1 
                                in_x2,      // x2 
                                in_y2,      // y2
                                color[1],   // msb    
                                color[0]    // lsb
                             }, 0, 7);
            ReadSync();
        }

        public void SetTextTransparency(TEXTTRANSPARENCY in_mode) {
            Write(new byte[] { 0x4F,                    // cmd = O
                                Convert.ToByte(in_mode) // transparent = 0, opaque = 1
                              }, 0, 2);
            ReadSync();
        }

        public void PutPixel(byte in_x, byte in_y, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);           
            Write(new byte[] {  0x50,       // cmd = P
                                in_x,       // x
                                in_y,       // y
                                color[1],   // msb    
                                color[0]    // lsb
                             }, 0, 5);
            ReadSync();
        }

        public void SetPenSize(PENSIZE in_size) {
            Write(new byte[] { 0x70,                    // cmd = p
                               Convert.ToByte(in_size)  // solid | wire frame
                              }, 0, 2);
            ReadSync();
        }

        public ushort ReadPixel(byte in_x, byte in_y) {
            Write(new byte[] { 0x52, // cmd = R
                               in_x, // x
                               in_y, // y
                              }, 0, 3);
            return Convert.ToUInt16(ReadExisting());
        }

        public void DrawRectangle(byte in_x1, byte in_y1,
                                  byte in_x2, byte in_y2, ushort in_color) {
            byte[] color = ConvertIntColorTo2Byte(in_color);
            Write(new byte[] {  0x72,       // cmd = r
                                in_x1,      // x1
                                in_y1,      // y1 
                                in_x2,      // x2 
                                in_y2,      // y2
                                color[1],   // msb    
                                color[0]    // lsb
                             }, 0, 7);
            ReadSync();
        }

        public void PlaceUnformattedASCII(byte in_x, byte in_y, FONTSIZE in_font, 
                                          ushort in_color, byte in_width, 
                                          byte in_height, string in_string) {
            // Run a quick check.
            if (in_string.Length > 256) {
                throw new Exception("Input string is too long.");
            }
            byte[] color = ConvertIntColorTo2Byte(in_color);
            // Break the command into three parts...
            Write(new byte[] {  0x53,                       // cmd = S
                                in_x,                       // x
                                in_y,                       // y 
                                Convert.ToByte(in_font),    // font                                 
                                color[1],                   // msb    
                                color[0],                   // lsb
                                in_width,                   // width
                                in_height                   // height
                             }, 0, 8);
            Write(in_string);                               // Unformatted string
            Write(new byte[] { 0x00 }, 0, 1);               // Terminator
            ReadSync();
        }

        public void PlaceFormattedASCII(byte in_column, byte in_row, FONTSIZE in_font,
                                          ushort in_color, byte in_width,
                                          byte in_height, string in_string) {
            // Run a few checks
            if (in_string.Length > 256) {
                throw new Exception("Input string is too long.");
            }
            if ( (in_font == FONTSIZE.SMALL && in_column > 20) ||
                 (in_font > FONTSIZE.SMALL && in_column > 15)){
                throw new Exception("Invalid column specification.");
            }
            if ((in_font == FONTSIZE.LARGE && in_row > 9) ||
                (in_font < FONTSIZE.LARGE && in_row > 15)) {
                throw new Exception("Invalid row specification.");
            }

            byte[] color = ConvertIntColorTo2Byte(in_color);
            // Break the command into three parts...
            Write(new byte[] {  0x73,                       // cmd = s
                                in_column,                  // column
                                in_row,                     // row
                                Convert.ToByte(in_font),    // font                                 
                                color[1],                   // msb    
                                color[0],                   // lsb
                                in_width,                   // width
                                in_height                   // height
                             }, 0, 8);
            Write(in_string);                               // Unformatted string
            Write(new byte[] { 0x00 }, 0, 1);               // Terminator
            ReadSync();
        }

        public void PlaceFormattedTextCharacter(char in_char, byte in_column, 
                                                byte in_row, ushort in_color) {
            // Run a few checks            
            //if ((in_font == FONTSIZE.SMALL && in_column > 20) ||
            //     (in_font > FONTSIZE.SMALL && in_column > 15)) {
            //    throw new Exception("Invalid column specification.");
            //}
            //if ((in_font == FONTSIZE.LARGE && in_row > 9) ||
            //    (in_font < FONTSIZE.LARGE && in_row > 15)) {
            //    throw new Exception("Invalid row specification.");
            //}

            byte[] color = ConvertIntColorTo2Byte(in_color);
            // Break the command into three parts...
            Write(new byte[] {  0x54,                       // cmd = T
                                Convert.ToByte(in_char),    // character
                                in_column,                  // column
                                in_row,                     // row                                                            
                                color[1],                   // msb    
                                color[0]                   // lsb
                             }, 0, 6);            
            ReadSync();
        }

        public void PlaceUnformattedTextCharacter(char in_char, byte in_x, byte in_y,
                                                  ushort in_color, byte in_width, byte in_height) {
            // Run a few checks            
            //if ((in_font == FONTSIZE.SMALL && in_column > 20) ||
            //     (in_font > FONTSIZE.SMALL && in_column > 15)) {
            //    throw new Exception("Invalid column specification.");
            //}
            //if ((in_font == FONTSIZE.LARGE && in_row > 9) ||
            //    (in_font < FONTSIZE.LARGE && in_row > 15)) {
            //    throw new Exception("Invalid row specification.");
            //}

            byte[] color = ConvertIntColorTo2Byte(in_color);
            // Break the command into three parts...
            Write(new byte[] {  0x74,                       // cmd = t
                                Convert.ToByte(in_char),    // character
                                in_x,                       // column
                                in_y,                       // row                                                            
                                color[1],                   // msb    
                                color[0],                   // lsb
                                in_width,                   // width
                                in_height                   // height
                             }, 0, 8);
            ReadSync();
        }

        public void Shutdown() {
            EraseScreen();
            DisplayControl(MODE.CONTRAST, 0x00); // Contrast off
            DisplayControl(MODE.DISPLAY, OFF); // Display off
            DisplayControl(MODE.POWER, OFF); // Power-Down
            Close();
        }

        public void DisplayControl(MODE in_mode, byte in_value) {
            Write(new byte[] {  0x59,                       // cmd = Y                                
                                Convert.ToByte(in_mode),    // width
                                in_value                    // height
                             }, 0, 3);
            ReadSync();
        }

        #endregion

        #region static_datahandling
        //private delegate void SetTextDeleg(string text);
        //private void si_DataReceived(string data) { textBlock1.Text = data.Trim(); }

        void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
            //Thread.Sleep(500);
            //string data = this.ReadLine();
            
            // Invokes the delegate on the UI thread, and sends the data that was received to the invoked method.
            // ---- The "si_DataReceived" method will be executed on the UI thread which allows populating of the textbox.
            //this.Dispatcher.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }
        #endregion

        #region methods
        public byte[] ConvertIntColorTo2Byte(ushort in_color) {
            return BitConverter.GetBytes(in_color);             
        }

        private void ReadSync() {
            while (ReadExisting() == "") { }
        }
        #endregion
    }
}
