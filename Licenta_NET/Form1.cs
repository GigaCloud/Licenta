using MCP2221;
using PICkitS;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.XPath;

namespace Licenta {
    enum Converter {
        MCP = 1,
        PIC,
    }

    public partial class Form1 :Form {
        bool absoluteMode = true;
        bool touchpadMode = false;

        [DllImport("User32.Dll")]
        static extern long SetCursorPos(int x, int y);

        byte[] i2cPosData = new byte[5];
        byte[] i2cPressData = new byte[2];

        const int port = 44441;
        Socket client;
        IPEndPoint localEndPoint;

        const int VID = 0x4D8;
        const int PID = 0xDD;

        const uint I2CSpeed = 100_000; //seems stable at this speed
                                       // const byte AR1000Address = 0x4D; //7 bit address

        const byte AR1000Address = 0x4D;

        const byte AR1000ReadId = (AR1000Address << 1) | 1; //read ID address p15 from AR1000 doc
        
        const uint NO_POS_BYTES = 5; //AR1000 datasheet, 5 bytes for pos

        const uint NO_PRESS_BYTES = 2;

        const byte FMAAddress = 0x28; //7 bit, Honeywell pressure sensor FMAMSDXX005WC2C3
        const byte FMAReadId = (FMAAddress << 1) | 1;   

        const uint xMax = 1 << 12;
        const uint yMax = 1 << 12;
        const float edgePercent = 12.5f;

        Converter lastConverter;

        MCP2221.MchpUsbI2c MCP2221;

        Rectangle screenRes = Screen.PrimaryScreen.Bounds;

        bool initMCP() {
            MCP2221 = new MCP2221.MchpUsbI2c(VID, PID);
            if (!MCP2221.Settings.GetConnectionStatus()) {
                richTextData.AppendText("Could not connect to MCP, resetting and retying...! \n");
                MCP2221.Functions.ResetDevice();
                Thread.Sleep(100);
                if (MCP2221.Settings.GetConnectionStatus()) {
                    MCP2221.Management.SelectDev(0);
                    richTextData.AppendText("MCP2221 Connected! \n");
                    return true;
                } else {
                    richTextData.AppendText("Critical failure - could not conect to MCP! \n");
                    return false;
                }
            } else {
                MCP2221.Management.SelectDev(0);
                richTextData.AppendText("MCP2221 Connected! \n");
                return true;
            }
        }

        void initPIC() {
            bool PICout = true;

            PICout &= PICkitS.Device.Initialize_PICkitSerial();
            if (PICout == false) {
                PICkitS.Device.Cleanup();
                PICout = true;
                PICout &= PICkitS.Device.Initialize_PICkitSerial(); //try again...
            }

            PICkitS.Device.Set_Script_Timeout(50); //ms

            PICout &= PICkitS.I2CM.Set_I2C_Bit_Rate(I2CSpeed / 1000); //argument is in kHz
            PICout &= PICkitS.I2CM.Configure_PICkitSerial_For_I2CMaster(false, false, true, true, true, (double)3.3f);

            PICkitS.I2CM.Set_Receive_Wait_Time(50); //ms

            if (PICout)
                richTextData.AppendText("PICkit Serial Analyzer connected! \n");
        }

        void sendDataSocket(UInt16 xCursor, UInt16 yCursor, UInt16 forceData, UInt16 flags) {
            byte[] bytesX = BitConverter.GetBytes(xCursor);
            byte[] bytesY = BitConverter.GetBytes(yCursor);
            byte[] bytesForce = BitConverter.GetBytes(forceData);
            byte[] bytesFlags = BitConverter.GetBytes(flags);

            byte[] packet = new byte[bytesX.Length + bytesY.Length + bytesForce.Length + bytesFlags.Length];

            System.Buffer.BlockCopy(bytesX, 0, packet, 0, bytesX.Length);
            System.Buffer.BlockCopy(bytesY, 0, packet, bytesX.Length, bytesY.Length);
            System.Buffer.BlockCopy(bytesForce, 0, packet, (bytesX.Length + bytesY.Length), bytesForce.Length);
            System.Buffer.BlockCopy(bytesFlags, 0, packet, (bytesX.Length + bytesY.Length + bytesForce.Length), bytesFlags.Length);
            
            if (client.Connected) {
                client.Send(packet);
            }
        }

        async void getDataLoop() {
            Thread.Sleep(50);

            while (true) {
                await Task.Delay(1);

                string dummyString = "";

                switch (lastConverter) {
                    case Converter.PIC:
                        PICkitS.I2CM.Receive(AR1000ReadId, (byte)NO_POS_BYTES, ref i2cPosData, ref dummyString);
                        break;
                    case Converter.MCP:
                        MCP2221.Functions.ReadI2cData(AR1000ReadId, i2cPosData, NO_POS_BYTES, I2CSpeed);
                        MCP2221.Functions.ReadI2cData(FMAReadId, i2cPressData, NO_PRESS_BYTES, I2CSpeed);
                        //Debug.Print("Trying to read: " + res);
                        break;
                }

                Int16 y = (Int16)(i2cPosData[1] | (i2cPosData[2] << 7)); //refer to the AR1000 doc, table 7.1
                Int16 x = (Int16)(i2cPosData[3] | (i2cPosData[4] << 7)); //warning, x/y are swapped here!

                Int16 force = (Int16)((i2cPressData[0] << 7) | i2cPressData[1]);

                double xr = 1.0f - (x / (double)xMax); //because I want it mirrored
                double yr = y / (double)yMax;



                UInt16 xCursor = (UInt16)Math.Round((xr * screenRes.Width * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Width / 100.0f));
                UInt16 yCursor = (UInt16)Math.Round((yr * screenRes.Height * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Height / 100.0f));
                UInt16 forceData = (UInt16)force;

               
                String data = (xr * 100).ToString(".0") + "%\t" + (yr * 100).ToString(".0") + "%\t" + force.ToString() + "\n";
                UInt16 flags;

                if (x != 0 && y != 0) {  //new coordinates reported
                    richTextData.AppendText(data);
                    richTextData.ScrollToCaret();
                    flags = 1;
                } else {
                    flags = 0;
                }

                sendDataSocket(xCursor, yCursor, forceData, flags);


            }

        }

        public Form1() {
            InitializeComponent();
            //getDataLoopPIC();

            var ipAddress = Array.Find(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            localEndPoint = new IPEndPoint(ipAddress, port);
            client = new(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void absoluteButton_Click(object sender, EventArgs e) {
            absoluteMode = true;
            touchpadMode = false;
        }

        private void touchpadButton_Click(object sender, EventArgs e) {
            absoluteMode = false;
            touchpadMode = true;
        }

        private void connectDriver_Click(object sender, EventArgs e) {
            if (!client.Connected) {
                try {
                    client.Connect(localEndPoint);
                } catch (SocketException err) {
                    Debug.Print("Could not connect socket! Err code: ");
                    Debug.Print(err.ErrorCode.ToString());
                }
                if (client.Connected) {
                    connectDriver.Text = "Disconnect driver";
                }
            } else {
                try {
                    client.Disconnect(true);
                } catch (SocketException err) {
                    Debug.Print("Could not disconnect socket! Err code: ");
                    Debug.Print(err.ErrorCode.ToString());
                }
                if (!client.Connected) {
                    connectDriver.Text = "Connect driver";
                }

            }
        }

        private void buttonPIC_Click(object sender, EventArgs e) {
            initPIC();
            lastConverter = Converter.PIC;
            getDataLoop();

        }

        private void buttonMCP_Click(object sender, EventArgs e) {
            if (initMCP()) {
                lastConverter = Converter.MCP;
                getDataLoop();
            }
        }
    }
}