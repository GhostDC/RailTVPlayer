using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;

namespace RailTVPlayer
{
    public partial class Form1 : Form
    {
        SerialPort comm = new SerialPort();

        public Form1()
        {
            InitializeComponent();
        }
        
        //窗口加载
        private void Form1_Load(object sender, EventArgs e)
        {
            OpenPort();
        }

        //开启端口
        void OpenPort()
        {
            comm.PortName = "COM3";//定义串口号
            comm.BaudRate = 9600;//波特率
            comm.DataBits = 8;//数据位
            comm.ReadBufferSize = 4096;
            comm.StopBits = StopBits.One;
            comm.Parity = Parity.None;
            comm.Open();
        }
    }
}
