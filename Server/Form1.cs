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

namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint port = new IPEndPoint(IPAddress.Any, Convert.ToInt32(txtPort.Text));
                socketWatch.Bind(port);
                socketWatch.Listen(1000);
                ShowMsg("监听成功!");
                await Task.Run(() =>
                {
                    Thread th = new Thread(Listen);
                    th.IsBackground = true;
                    th.Start(socketWatch);
                });
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();
        private async void Listen(object obj)
        {
            Socket socketWatch = obj as Socket;
            while (true)
            {
                try
                {
                    Socket socketSend = socketWatch.Accept();
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    cboUsers.Items.Add(socketSend.RemoteEndPoint.ToString());
                    cboUsers.SelectedItem = socketSend.RemoteEndPoint.ToString();
                    ShowMsg($"{socketSend.RemoteEndPoint}连接成功!");
                    await Task.Run(() =>
                    {
                        Thread th = new Thread(Receive);
                        th.IsBackground = true;
                        th.Start(socketSend);
                    });
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                }
            }
        }


        private void Receive(object obj)
        {
            Socket socketSend = obj as Socket;

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 5];
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg($"{socketSend.RemoteEndPoint}:{str}");
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                }
            }
        }

        private void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboUsers.SelectedItem != null)
                {
                    string str = txtMsg.Text.Trim();
                    byte[] buffer = Encoding.UTF8.GetBytes(str);
                    List<byte> list = new List<byte>();
                    list.Add(0);//文件标识
                    list.AddRange(buffer);
                    byte[] newBuffer = list.ToArray();
                    string ip = cboUsers.SelectedItem.ToString();
                    dicSocket[ip].Send(newBuffer);
                    ShowMsg($"{dicSocket[ip].LocalEndPoint}:{str}");
                    txtMsg.Clear();
                }
                else
                {
                    ShowMsg("未选择客户端");
                }
            }
            catch (Exception ex)
            {
                txtMsg.Clear();
                ShowMsg(ex.Message);
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog pfd = new OpenFileDialog();
            pfd.Title = "强选择要发送的文件";
            pfd.InitialDirectory = @"C:\Users\zhanglifu\Desktop";
            pfd.Filter = "文本文件|*.txt|全部文件|*.*";
            pfd.ShowDialog();
            txtPath.Text = pfd.FileName;
        }

        private void txtSendFile_Click(object sender, EventArgs e)
        {
            if (cboUsers.SelectedItem != null)
            {
                try
                {
                    using (FileStream fsRead = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[1024 * 1024 * 5];
                        int r = fsRead.Read(buffer, 0, buffer.Length);
                        List<byte> list = new List<byte>();
                        list.Add(1);//文件类型
                        list.AddRange(buffer);
                        byte[] newBuffer = list.ToArray();
                        string ip = cboUsers.SelectedItem.ToString();
                        dicSocket[ip].Send(newBuffer);
                        ShowMsg($"{dicSocket[ip].LocalEndPoint}:文件发送成功!");
                    }
                }
                catch
                {
                    ShowMsg("未选择文件");
                }
            }
            else
            {
                ShowMsg("未选择客户端");
            }
        }

        private void btnZD_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1] { 2 };
            string ip = cboUsers.SelectedItem.ToString();
            dicSocket[ip].Send(buffer);
            ShowMsg($"{dicSocket[ip].LocalEndPoint}震动发送成功!");
        }
    }
}
