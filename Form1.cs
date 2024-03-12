using mcp2221_dll_m;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.XPath;



namespace Licenta
{
    public partial class Form1 : Form
    {
        bool absoluteMode = true;
        bool touchpadMode = false;

        async void getDataLoop()
        {
            byte[] i2cPosData = new byte[5];

            const uint VID = 0x4D8;
            const uint PID = 0xDD; //default MCP2221 vals
            const uint I2CSpeed = 100000; //seems stable at this speed
            const byte AR1000Address = 0x4D; //7 bit address
            const uint NO_POS_BYTES = 5; //AR1000 datasheet, 5 bytes for pos

            const uint xMax = 1 << 12;
            const uint yMax = 1 << 12;
            const float edgePercent = 12.5f;

            [DllImport("User32.Dll")]
            static extern long SetCursorPos(int x, int y);


            Rectangle screenRes = Screen.PrimaryScreen.Bounds;


            IntPtr MCPHandle = MCP2221.M_Mcp2221_OpenByIndex(VID, PID, 0); //use the first connected MCP
            //TODO: handle invalid pointer value here

            if (MCPHandle == IntPtr.Zero) //DOESN'T WORK
                MessageBox.Show("Failed to connect to MCP!");



            Debug.WriteLine(MCPHandle.ToString());

            int e = MCP2221.M_Mcp2221_SetSpeed(MCPHandle, I2CSpeed);

            Debug.WriteLine(e);
            Thread.Sleep(50);

            while (true)
            {
                await Task.Delay(1);
                int err = MCP2221.M_Mcp2221_I2cRead(MCPHandle, NO_POS_BYTES, AR1000Address, 1, i2cPosData);

                Int16 y = (Int16)(i2cPosData[1] | (i2cPosData[2] << 7)); //refer to the AR1000 doc, table 7.1
                Int16 x = (Int16)(i2cPosData[3] | (i2cPosData[4] << 7)); //warning, x/y are swapped here!

                float xr = 1.0f - (x / (float)xMax); //because I want it mirrored
                float yr = y / (float)yMax;

                int lastX = 0;
                int lastY = 0;

                if (x != 0 && y != 0)
                {  //new coordinates reported

                    if (absoluteMode)
                    {
                        int xCursor = (int)(xr * screenRes.Width * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Width / 100.0f);
                        int yCursor = (int)(yr * screenRes.Height * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Height / 100.0f);

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

                String data = (xr * 100).ToString(".0") + "%\t" + (yr * 100).ToString(".0") + "%\n";
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
            MCP2221.M_Mcp2221_CloseAll();
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