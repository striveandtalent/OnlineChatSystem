using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Socket socketSend;
        private async void btnStart_Click(object sender, EventArgs e)
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint port = new IPEndPoint(IPAddress.Parse(txtServerIP.Text), Convert.ToInt32(txtServerPort.Text));
            await Task.Run(() =>
            {
                socketSend.Connect(port);
                ShowMsg("连接成功!");
                labDevicePort.Text = ((IPEndPoint)socketSend.LocalEndPoint).Port.ToString();
                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start(socketSend);
            });
        }

        private void Recive(object obj)
        {
            Socket socketSend = obj as Socket;
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = socketSend.Receive(buffer);
                if (r == 0)
                {
                    break;
                }
                else
                {
                    int type = buffer[0];
                    if (type == 0)
                    {//文字消息
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        string ip = socketSend.RemoteEndPoint.ToString();
                        ShowMsg($"{ip}:{str}");
                    }
                    else if (type == 1)
                    {//文件消息 
                        try
                        {
                            SaveFileDialog sfd = new SaveFileDialog();
                            sfd.Title = "选择要保存的路径";
                            sfd.InitialDirectory = @"C:\Users\zhanglifu\Desktop";
                            sfd.Filter = "文本文件|*.txt|全部文件|*.*";
                            sfd.ShowDialog(this);
                            string path = sfd.FileName;
                            using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fsWrite.Write(buffer, 1, r - 1);
                            }
                            MessageBox.Show("文件保存成功!");
                            ShowMsg("文件保存成功!");
                        }
                        catch (Exception ex)
                        {
                            ShowMsg(ex.Message);
                        }
                    }
                    else if (type == 2)
                    {//震动消息
                        ShowMsg($"{socketSend.RemoteEndPoint}:发送了一个震动给你!");
                        ZD();
                    }
                }
            }
        }

        private async void ZD()
        {
            Random rand = new Random();
            Point originalLocation = this.Location;

            for (int i = 0; i < 50; i++) // 减少循环次数，增加延迟
            {
                int offsetX = rand.Next(-10, 10);
                int offsetY = rand.Next(-10, 10);

                this.Location = new Point(originalLocation.X + offsetX,
                                         originalLocation.Y + offsetY);

                await Task.Delay(10); // 添加延迟以便看到效果
            }

            this.Location = originalLocation; // 恢复原始位置
        }

        private void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string str = txtMsg.Text.Trim();
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                socketSend.Send(buffer);
                txtMsg.Clear();
                ShowMsg($"{socketSend.LocalEndPoint}:{str}");
            }
            catch (Exception ex)
            {
                txtMsg.Clear();
                ShowMsg(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
