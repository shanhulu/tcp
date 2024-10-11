using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Socket
{
    public partial class Form1 : Form
    {
        // 保存客户端的Socket，用于后续发送数据
        private System.Net.Sockets.Socket socketSend;
        private System.Net.Sockets.Socket socketWatch;
        private Thread receiveThread; // 用于接收数据的线程
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }



        #region 服务端
        /// <summary>
        /// 连接触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnServerConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //当点击连接开始监听的时候，在服务端创建一个负责监听IP地址根端口号的Socket
                System.Net.Sockets.Socket socketWatch = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //创建IP地址和端口号对象
                //网络终结点（即一个 IP 地址和一个端口号）
                IPAddress ipAddress = IPAddress.Any;
                IPEndPoint IPandPort = new IPEndPoint(ipAddress, Convert.ToInt32(serverPort.Text));
                //绑定端口号
                socketWatch.Bind(IPandPort);
                MessageBox.Show("连接成功");
                ShowMsg("连接成功");

                socketWatch.Listen(10);
                //等待客户连接，并创建一个负责通讯的socket并设置为后台线程
                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);

            }
            catch
            {
            }
        }
        /// <summary>
        /// 消息显示
        /// </summary>
        /// <param name="str"></param>
        void ShowMsg(string str)
        {
            textBox1.AppendText(str + "\r\n");
        }

        void Listen(Object o)
        {
            System.Net.Sockets.Socket socketWatch = o as System.Net.Sockets.Socket; // 将参数 o 转换为 Socket 类型

            while (true) // 进入无限循环，持续监听客户端连接
            {
                socketSend = socketWatch.Accept(); // 接受客户端的连接请求，返回一个新的 Socket 对象
                ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功"); // 显示连接成功的消息

                Thread th = new Thread(Rivice); // 创建一个新线程来处理客户端通信
                th.IsBackground = true; // 将线程设置为后台线程
                th.Start(socketSend); // 启动新线程，并传递 socketSend 作为参数
            }
        }

        void Rivice(object o)
        {
            System.Net.Sockets.Socket socketSend = o as System.Net.Sockets.Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 2];

                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                }
                catch { }
            }
        }

        private void btnServerSend_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取要发送的消息
                string message = textBoxMessage.Text.Trim();

                // 将消息转换为字节数组
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                // 发送消息给客户端
                socketSend.Send(buffer);

                // 显示发送的消息
                ShowMsg("服务器: " + message);
            }
            catch (Exception ex)
            {
                ShowMsg("发送失败: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
        #endregion





        #region 客户端
        private void btnClientConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //当点击连接开始监听的时候，在客户端创建一个负责监听IP地址根端口号的Socket
                socketWatch = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(clientIP.Text);
                //创建端口号对象
                IPEndPoint Port = new IPEndPoint(ipAddress, Convert.ToInt32(clientPort.Text));

                //连接端口号
                socketWatch.Connect(Port);
                MessageBox.Show("连接成功");

                // 启动接收数据的线程
                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true; // 设置为后台线程
                receiveThread.Start();
            }
            catch { }

        }

        private void btnClientSend_Click(object sender, EventArgs e)
        {
            string str = textBoxClientMessage.Text.Trim();
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            socketWatch.Send(buffer);
        }

        /// <summary>
        /// 接收数据的方法
        /// </summary>
        private void ReceiveData()
        {
            while (true)
            {
                try
                {

                    // 创建接收数据的缓冲区
                    byte[] buffer = new byte[1024];

                    if (socketWatch.Poll(1000, SelectMode.SelectRead))
                    {
                        // 连接已关闭
                        if (socketWatch.Available == 0)
                        {
                            Show("连接已关闭");
                            break; // 退出循环
                        }
                    }
                    int bytesRead = socketWatch.Receive(buffer);

                    // 将接收到的数据转换为字符串并显示
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Show("接收到: " + receivedData);
                }
                catch (Exception ex)
                {
                    Show("接收数据失败: " + ex.Message);
                    break; // 发生异常，退出循环
                }
            }
        }

        void Show(string str)
        {
            if (textBox5.InvokeRequired)
            {
                textBox5.Invoke(new Action(() => Show(str)));
            }
            else
            {
                textBox5.AppendText(str + "\r\n");
            }
        }

        #endregion




    }
}
