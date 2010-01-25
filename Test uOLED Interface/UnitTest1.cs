using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uOLED_Interface;

namespace Test_uOLED_Interface {
    /// <summary>
    /// Each of these tests will check the functionality of the uOLED Interface
    /// </summary>
    [TestClass]
    public class UnitTest1 {
        public UnitTest1() {
            // Setup the port and baud rate
            m_portName = "COM3";
            m_baudRate = 115200;            
            m_Display = new uOLED(m_portName, m_baudRate);
        }

        private TestContext testContextInstance;
        private string m_portName;
        private int    m_baudRate;
        private uOLED  m_Display;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public void Init() {
            if (!m_Display.IsOpen) {
                m_Display.Open();
            }
        }

        public void Cleanup() {
            if (m_Display.IsOpen) {
                m_Display.EraseScreen();
                m_Display.Close();
            }
        }
        
        [TestMethod]
        public void Test_OpenClose() {
            if (m_Display.IsOpen) {
                m_Display.Close();
            }
            m_Display.Open();
            m_Display.Close();            
        }

        [TestMethod]
        public void Test_Shutdown() {
            Init();
            m_Display.Shutdown();
            Cleanup();           
        }

        [TestMethod]
        public void Test_AddUserBitmappedCharacter() {
            Init();
            m_Display.AddUserBitmappedCharacter(0x01, new byte[] { 0x18, 0x24, 0x42, 0x81, 0x81, 0x42, 0x24, 0x18 });
            Cleanup();
        }

        [TestMethod]
        public void Test_SetBackgroundColor() {
            Init();            
            m_Display.SetBackgroundColor(0xFFFF); //white
            m_Display.SetBackgroundColor(0x0); //black
            Cleanup();
        }

        [TestMethod]
        public void Test_PlaceTextButton() {
            throw new Exception("Not Implemented");            
        }

        [TestMethod]
        public void Test_DrawCircle() {
            Init();
            m_Display.DrawCircle(63, 63, 34, 0x001F);
            Cleanup();
        }

        [TestMethod]
        public void Test_BitmapCopy() {
            throw new Exception("Not Implemented");
        }

        [TestMethod]
        public void Test_DisplayUserBitmappedCharacter() {
            Init();
            // Load a Bitmapped Character into char# 0x01
            m_Display.AddUserBitmappedCharacter(0x01, new byte[] { 0x18, 0x24, 0x42, 0x81, 0x81, 0x42, 0x24, 0x18 });
            // Display 8x8 bitmap character number 1 at x=0, y=0, colour=red
            m_Display.DisplayUserBitmappedCharacter(0x01, 0, 0, 0xF800);
            Cleanup();
        }

        [TestMethod]
        public void Test_EraseScreen() {
            Init();
            m_Display.EraseScreen();
            Cleanup();
        }

        [TestMethod]
        public void Test_SetFontSize() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DrawTriangle() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DrawPolygon() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DisplayImage() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DrawLine() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_SetTextTransparency() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_PutPixel() {
            Init();
            m_Display.PutPixel(10, 10, 10);
            Cleanup();
        }

        [TestMethod]
        public void Test_SetPenSize() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_ReadPixel() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DrawRectangle() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_PlaceUnformattedASCII() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_PlaceFormattedASCII() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_PlaceUnformattedTextCharacter() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_PlaceFormattedTextCharacter() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_VersionInfoRequest() {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void Test_DisplayControl() {
            Init();
            Cleanup();
        }
    }
}
