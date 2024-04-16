//using mcp2221_dll_m;
using PICkitS;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.XPath;

namespace Licenta
{
    public partial class Form1 : Form
    {
        bool absoluteMode = true;
        bool touchpadMode = false;

        [DllImport("User32.Dll")]
        static extern long SetCursorPos(int x, int y);

        byte[] i2cPosData = new byte[5];
        byte[] i2cPressData = new byte[2];

        async void getDataLoop() { 
        

            const uint I2CSpeed = 100_000; //seems stable at this speed
            // const byte AR1000Address = 0x4D; //7 bit address
            const byte AR1000Address = 0x9B; //read ID address p15 from AR1000 doc
            const uint NO_POS_BYTES = 5; //AR1000 datasheet, 5 bytes for pos

            const uint NO_PRESS_BYTES = 2;
            const byte FMAAddress = 0x28; //7 bit, Honeywell pressure sensor FMAMSDXX005WC2C3


            const uint xMax = 1 << 12;
            const uint yMax = 1 << 12;
            const float edgePercent = 12.5f;


          
            Rectangle screenRes = Screen.PrimaryScreen.Bounds;

            bool PICout = true;

            PICout &= PICkitS.Device.Initialize_PICkitSerial();
            if (PICout == false)
            {
                PICkitS.Device.Cleanup();
                PICout = true;
                PICout &= PICkitS.Device.Initialize_PICkitSerial();
            }


            PICkitS.Device.Set_Script_Timeout(100);

            PICout &= PICkitS.I2CM.Set_I2C_Bit_Rate(I2CSpeed / 1000); //argument is in kHz
            PICout &= PICkitS.I2CM.Configure_PICkitSerial_For_I2CMaster(false, false, true, true, true, (double)3.3f);

            PICkitS.I2CM.Set_Receive_Wait_Time(100);



            Thread.Sleep(50);

            while (true && PICout)
            {
                await Task.Delay(1);

                string dummyString = "";

                bool i2cRec = PICkitS.I2CM.Receive(AR1000Address, (byte)NO_POS_BYTES, ref i2cPosData, ref dummyString);

                Debug.WriteLine(dummyString);

                //if (i2cRec == false) break;
                uint perr = 0;
                PICkitS.Device.There_Is_A_Status_Error(ref perr);

                //PICkitS.I2CM.

                Int16 y = (Int16)(i2cPosData[1] | (i2cPosData[2] << 7)); //refer to the AR1000 doc, table 7.1
                Int16 x = (Int16)(i2cPosData[3] | (i2cPosData[4] << 7)); //warning, x/y are swapped here!

                Int16 force = (Int16)((i2cPressData[0] << 7) | i2cPressData[1]);

                double xr = 1.0f - (x / (double)xMax); //because I want it mirrored
                double yr = y / (double)yMax;

                int lastX = 0;
                int lastY = 0;

                if (x != 0 && y != 0)
                {  //new coordinates reported

                    if (absoluteMode)
                    {
                        int xCursor = (int)Math.Round((xr * screenRes.Width * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Width / 100.0f));
                        int yCursor = (int)Math.Round((yr * screenRes.Height * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Height / 100.0f));

                        SetCursorPos(xCursor, yCursor);
                    }

                    if (touchpadMode)
                    {
                        if (lastX == 0 && lastY == 0)
                        {
                            lastX = x;
                            lastY = y;
                        }
                    }
                }
                else
                {
                    lastX = 0;
                    lastY = 0;
                }

                String data = (xr * 100).ToString(".0") + "%\t" + (yr * 100).ToString(".0") + "%\t" + force.ToString() + "\n";
                Debug.Write(data);
                richTextData.AppendText(data);
                richTextData.ScrollToCaret();
            }

        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /* if (MessageBox.Show("Are you sure you want to quit?", "My Application", MessageBoxButtons.YesNo) == DialogResult.No)
             {
                 e.Cancel = true;
             } */

            Debug.WriteLine("Exiting and closing all MCP2221 sessions...");
            //MCP2221.M_Mcp2221_CloseAll();
        }

        public Form1()
        {
            InitializeComponent();
            FormClosing += new FormClosingEventHandler(Form1_Closing);
            getDataLoop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void absoluteButton_Click(object sender, EventArgs e)
        {
            absoluteMode = true;
            touchpadMode = false;
        }

        private void touchpadButton_Click(object sender, EventArgs e)
        {
            absoluteMode = false;
            touchpadMode = true;
        }
    }
}