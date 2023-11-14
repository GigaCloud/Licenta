using mcp2221_dll_m;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.XPath;



namespace Licenta
{
    public partial class Form1 : Form
    {
        async void getDataLoop()
        {
            byte[] i2cPosData = new byte[5];

            const uint VID = 0x4D8;
            const uint PID = 0xDD; //default MCP2221 vals
            const uint I2CSpeed = 100000; //seems stable at this speed
            const byte AR1000Address = 0x4D; //7 bit address
            const uint NO_POS_BYTES = 5; //AR1000 datasheet, 5 bytes for pos

            const uint xMax = 8192;
            const uint yMax = 8192;
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
                int err = MCP2221.M_Mcp2221_I2cRead(MCPHandle, NO_POS_BYTES, AR1000Address, (byte)(1), i2cPosData);
                //Debug.WriteLine(err);

                Int16 y = BitConverter.ToInt16(i2cPosData, 1); //these will have a max of 8192 because of the 8th bit
                Int16 x = BitConverter.ToInt16(i2cPosData, 3); //refer to the AR1000 doc, table 7.1

                float xr = x / (float)xMax;
                float yr = y / (float)yMax;

                if (x != 0 && y != 0)
                {  //new coordinates reported
                    int xCursor = (int)(xr * screenRes.Width * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Width / 100.0f);
                    int yCursor = (int)(yr * screenRes.Height * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenRes.Height / 100.0f);

                    SetCursorPos(xCursor, yCursor);
                }

                Debug.WriteLine((xr * 100).ToString() + "%\t" + (yr * 100).ToString() + "%");

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
    }
}