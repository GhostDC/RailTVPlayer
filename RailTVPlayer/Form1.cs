﻿using System;
using System.Threading;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;

namespace RailTVPlayer
{
    public partial class Form1 : Form
    {
        static string currentPath = Application.StartupPath;
        string MoviePath;
        IniFiles iniPath = new IniFiles(currentPath + "Config.ini");
        SerialPort comm = new SerialPort();
        Thread t_TimerLoop;
        int txtAddr;
        int diNum;
        string portName;
        int currentNum = 0;
        int currentNumB = 0;

        public Form1()
        {
            InitializeComponent();
        }
        
        //窗口加载
        private void Form1_Load(object sender, EventArgs e)
        {
            if(iniPath.ExistIniFile())
            {
                portName = iniPath.iniReadValue("Config", "PortName");
                txtAddr = Convert.ToInt32(iniPath.iniReadValue("Config", "PortAddr"));
                diNum = Convert.ToInt32(iniPath.iniReadValue("Config", "DataInputNum"));
                axShockwaveFlash1.Movie = iniPath.iniReadValue("LastFile", "FilePath");

                OpenPort();
            }
            else
            {
                iniPath.IniWriteValue("Config", "PortName", "Com3");
                iniPath.IniWriteValue("Config", "PortAddr", "254");
                iniPath.IniWriteValue("Config", "DataInputNum", "10");
            }

            t_TimerLoop = new Thread(TimerLoop);
            t_TimerLoop.Start();
        }

        //开启端口
        void OpenPort()
        {
            comm.PortName = portName;//定义串口号
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
            Thread.Sleep(50);

            TimerLoop();
        }

        //对输入的数据进行读取 并计算相对应的端口号
        void ReadDI()
        {
            byte[] info = CModbusDll.ReadDI(Convert.ToInt16(txtAddr), Convert.ToInt16(diNum));
            byte[] rst = SendInfo(info);
            if (comm.IsOpen)
            {
                if (rst.Length <= 1)
                {
                    if (rst[0] != 0)
                    {
                        currentNumB = Convert.ToInt32(rst[0]);
                        currentNum = BinToDec(currentNumB);
                        this.Invoke(new EventHandler(delegate
                        {
                            Play(currentNum);
                        }));
                    }
                }
                else
                {
                    if (rst[0] != 0)
                    {
                        currentNumB = Convert.ToInt32(rst[0]);
                        currentNum = BinToDec(currentNumB);
                    }
                    else if (rst[0] == 0 && rst[1] != 0)
                    {
                        currentNumB = Convert.ToInt32(rst[1]);
                        currentNum = BinToDec(currentNumB);
                        currentNum += 8;
                    }
                    this.Invoke(new EventHandler(delegate
                    {
                        Play(currentNum);
                    }));

                }
            }
            else
            {
                MessageBox.Show("请设置正确的COM端口");
            }
        }

        //对所计算出的文件进行播放
        void Play(int MovieNum)
        {
            label1.Text = Convert.ToString(MovieNum);
            if(MovieNum!=0)
            {
                axShockwaveFlash1.Movie = currentPath + "\\movie\\" + MovieNum + ".swf";
            }
        }

        //将二进制数转换为10进制数
        int BinToDec(int currentNumB)
        {
            if (currentNumB > 2)
            {
                currentNum = Convert.ToInt32(Math.Log(currentNumB, 2) + 1);
                return currentNum;
            }
            else
            {
                return currentNumB;
            }
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

        //窗口关闭 释放线程
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (currentNum != 0)
            {
                MoviePath = currentPath + "\\movie\\" + currentNum + ".swf";
                iniPath.IniWriteValue("LastFile", "FilePath", MoviePath);
            }
            t_TimerLoop.Abort();
        }
    }
}
