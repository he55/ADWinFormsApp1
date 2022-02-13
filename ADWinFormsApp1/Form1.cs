using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADWinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const int port = 12500;

        Socket socket1;
        bool isInitServer;
        Dictionary<long, string> ipkv = new Dictionary<long, string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            IPEndPoint iPEndPoint1 = new IPEndPoint(IPAddress.Any, port);
            socket1.Bind(iPEndPoint1);

            Task.Run(() => OnRec());
        }

        void OnRec()
        {
            byte[] buf = new byte[1024];

            while (true)
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int len = socket1.ReceiveFrom(buf, ref ep);
                if (len >= 8 && ADMsg.IsMSG(buf))
                {
                    ADMsg msg = ADMsg.ToMSG(buf);
                    if (msg.type == ADMsgType.hello)
                    {
                        ADMsg msg2 = new ADMsg(ADMsgType.helloOK);
                        string v = Dns.GetHostName();
                        msg2.AddNameData(v);
                        byte[] buf2 = msg2.ToArr();

                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                    }
                    else if (msg.type == ADMsgType.helloOK)
                    {
                        iPEndPoint2 = new IPEndPoint(((IPEndPoint)ep).Address, port);
                        ipkv[((IPEndPoint)ep).Address.Address] = msg.ToNameData();

                        this.Invoke(new Action(() =>
                        {
                            listBox2.Items.Add($"{ep}");
                        }));
                    }
                    else if (msg.type == ADMsgType.sendFile)
                    {
                        msg.ToFileData();

                        if (true)
                        {
                            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                            if (!isInitServer)
                            {
                                TcpServer.StartServerTcp(remoteEP);
                                isInitServer = true;
                            }

                            ADMsg msg2 = new ADMsg(ADMsgType.sendFileOK);
                            msg2.AddIPData(remoteEP);
                            byte[] buf2 = msg2.ToArr();
                            socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                        }
                    }
                    else if (msg.type == ADMsgType.sendFileOK)
                    {
                        string filePath = @"C:\Users\luckh\Desktop\vmware.exe";
                        IPEndPoint remoteEP = msg.ToIPData();

                        Task.Run(() =>
                        {
                            TcpServer.StartClientTcp(filePath, remoteEP);
                        });
                    }
                    else if (msg.type == ADMsgType.sendUrl)
                    {
                        string v = msg.ToUrlData();
                    }
                    else if (msg.type == ADMsgType.sendString)
                    {
                        string v = msg.ToStringData();
                    }

                    // log
                    this.Invoke(new Action(() =>
                    {
                        listBox1.Items.Add($"{ep} => {msg.type} : {msg.ToStringData()}");
                    }));
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            ipkv.Clear();

            byte[] buf = new ADMsg(ADMsgType.hello).ToArr();
            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }


        IPEndPoint iPEndPoint2;
        private void button3_Click(object sender, EventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendString);
            msg.AddStringData(textBox2.Text);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendUrl);
            msg.AddUrlData("https://devblogs.microsoft.com/");
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        string filePath = @"C:\Users\luckh\Desktop\vmware.exe";
        private void button5_Click(object sender, EventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendFile);
            msg.AddFileData(filePath);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }
    }
}
