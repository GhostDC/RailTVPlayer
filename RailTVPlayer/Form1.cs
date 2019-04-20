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
        Thread t_TimerLoop;
        int txtAddr;
        int diNum;

        public Form1()
        {
            InitializeComponent();
        }
        
        //窗口加载
        private void Form1_Load(object sender, EventArgs e)
        {
            OpenPort();

            t_TimerLoop = new Thread(TimerLoop);
            t_TimerLoop.Start();
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

        //循环读取输入数据
        void TimerLoop()
        {
            ReadDI();

            TimerLoop();
        }

        //对输入的数据进行读取
        void ReadDI()
        {
            byte[] info = CModbusDll.ReadDI(Convert.ToInt16(txtAddr), Convert.ToInt16(diNum));
            byte[] rst = SendInfo(info);
        }

        //向继电器发送数据
        int errrcvcnt = 0;
        private byte[] SendInfo(byte[] info)
        {
            try
            {
                byte[] data = new byte[2048];
                int len = 0;

                comm.Write(info, 0, info.Length);
                //DebugInfo("发送", info, info.Length);

                try
                {
                    Thread.Sleep(50);
                    Stream ns = comm.BaseStream;
                    ns.ReadTimeout = 50;
                    len = ns.Read(data, 0, 2048);

                    //DebugInfo("接收", data, len);
                }
                catch (Exception)
                {
                    return null;
                }
                errrcvcnt = 0;
                return AnalysisRcv(data, len);
            }
            catch (Exception)
            {

            }
            return null;
        }

        //分析从继电器收到的信息
        private byte[] AnalysisRcv(byte[] src, int len)
        {
            if (len < 6) return null;
            if (src[0] != 254) return null;

            switch (src[1])
            {
                case 0x01:
                    if (CMBRTU.CalculateCrc(src, src[2] + 5) == 0x00)
                    {
                        byte[] dst = new byte[src[2]];
                        for (int i = 0; i < src[2]; i++)
                            dst[i] = src[3 + i];
                        return dst;
                    }
                    break;
                case 0x02:
                    if (CMBRTU.CalculateCrc(src, src[2] + 5) == 0x00)
                    {
                        byte[] dst = new byte[src[2]];
                        for (int i = 0; i < src[2]; i++)
                            dst[i] = src[3 + i];
                        return dst;
                    }
                    break;
                case 0x04:
                    if (CMBRTU.CalculateCrc(src, src[2] + 5) == 0x00)
                    {
                        byte[] dst = new byte[src[2]];
                        for (int i = 0; i < src[2]; i++)
                            dst[i] = src[3 + i];
                        return dst;
                    }
                    break;
                case 0x05:
                    if (CMBRTU.CalculateCrc(src, 8) == 0x00)
                    {
                        byte[] dst = new byte[1];
                        dst[0] = src[4];
                        return dst;
                    }
                    break;
                case 0x0f:
                    if (CMBRTU.CalculateCrc(src, 8) == 0x00)
                    {
                        byte[] dst = new byte[1];
                        dst[0] = 1;
                        return dst;
                    }
                    break;
            }
            return null;
        }
    }
}
