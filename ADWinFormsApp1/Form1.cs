using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            IPEndPoint iPEndPoint1 = new IPEndPoint(IPAddress.Any, port);
            socket1.Bind(iPEndPoint1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            Task.Run(() => met());
        }


        bool isInitServer;
        Dictionary<long, string> kv = new Dictionary<long, string>();
        void met()
        {
            byte[] buf = new byte[100];

            while (true)
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int len = socket1.ReceiveFrom(buf, ref ep);
                if (len >= 8 && MSG.IsMSG(buf))
                {
                    MSG msg = MSG.ToMSG(buf);
                    if (msg.type == ADMsgType.hello)
                    {
                        MSG msg2 = new MSG(ADMsgType.helloOK);
                        string v = Dns.GetHostName();
                        msg2.AddNameData(v);
                        byte[] buf2 = msg2.ToArr();

                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                    }
                    else if (msg.type == ADMsgType.helloOK)
                    {
                        iPEndPoint2 = new IPEndPoint(((IPEndPoint)ep).Address, port);
                        kv[((IPEndPoint)ep).Address.Address] = msg.ToNameData();

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
                                ServerTcp.StartServerTcp(remoteEP);
                                isInitServer = true;
                            }

                            MSG msg2 = new MSG(ADMsgType.sendFileOK);
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
                            ClientTcp.StartClientTcp(filePath, remoteEP);
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
            kv.Clear();

            byte[] buf = new MSG(ADMsgType.hello).ToArr();
            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }


        IPEndPoint iPEndPoint2;
        private void button3_Click(object sender, EventArgs e)
        {
            MSG msg = new MSG(ADMsgType.sendString);
            msg.AddStringData(textBox2.Text);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MSG msg = new MSG(ADMsgType.sendUrl);
            msg.AddUrlData("https://devblogs.microsoft.com/");
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        string filePath = @"C:\Users\luckh\Desktop\vmware.exe";
        private void button5_Click(object sender, EventArgs e)
        {
            MSG msg = new MSG(ADMsgType.sendFile);
            msg.AddFileData(filePath);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }
    }

    public enum ADMsgType : int
    {
        hello = 0x1,
        helloOK,
        sendFile,
        sendFileOK,
        sendUrl,
        sendString
    }

    public struct MSG
    {
        const uint HEADER = 0xadadadad;
        uint header;
        public ADMsgType type;
        int len;
        byte[] data;

        public MSG(ADMsgType type)
        {
            this.header = HEADER;
            this.type = type;
            this.len = 0;
            this.data = new byte[0];
        }

        public void AddIPData(IPEndPoint ep)
        {
            byte[] addr = ep.Address.GetAddressBytes();
            this.len = 8;
            this.data = new byte[8] {
                addr[0],
                addr[1],
                addr[2],
                addr[3],
                (byte)ep.Port,
                (byte)(ep.Port>>8),
                (byte)(ep.Port>>16),
                (byte)(ep.Port>>24),
            };
        }

        public void AddStringData(string str)
        {
            this.data = Encoding.UTF8.GetBytes(str);
            this.len = this.data.Length;
        }

        public void AddUrlData(string url)
        {
            AddStringData(url);
        }

        public void AddNameData(string name)
        {
            AddStringData(name);
        }

        public void AddFileData(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long length = fileInfo.Length;

            byte[] vs1 = BitConverter.GetBytes(length);
            byte[] vs = Encoding.UTF8.GetBytes(fileInfo.Name);

            this.len = 8 + vs.Length;
            this.data = new byte[this.len];

            vs1.CopyTo(this.data, 0);
            vs.CopyTo(this.data, 8);
        }

        public IPEndPoint ToIPData()
        {
            int addr = BitConverter.ToInt32(this.data, 0);
            int port = BitConverter.ToInt32(this.data, 4);
            return new IPEndPoint(addr, port);
        }

        public string ToStringData()
        {
            return Encoding.UTF8.GetString(this.data);
        }

        public string ToUrlData()
        {
            return ToStringData();
        }

        public string ToNameData()
        {
            return ToStringData();
        }

        public void ToFileData()
        {
            long v = BitConverter.ToInt64(this.data, 0);
            string v1 = Encoding.UTF8.GetString(this.data, 8, this.len - 8);
        }

        public byte[] ToArr()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(this.header);
                    binaryWriter.Write((int)this.type);
                    binaryWriter.Write(this.len);
                    binaryWriter.Write(this.data);
                }
                return memoryStream.ToArray();
            }
        }

        public static MSG ToMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            if (v != HEADER)
            {
                throw new Exception();
            }

            MSG msg;
            msg.header = HEADER;
            msg.type = (ADMsgType)BitConverter.ToInt32(buf, 4);
            msg.len = BitConverter.ToInt32(buf, 8);
            msg.data = new byte[0];

            if (msg.len != 0)
            {
                msg.data = new byte[msg.len];
                Array.Copy(buf, 12, msg.data, 0, msg.len);
            }

            return msg;
        }

        public static bool IsMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            return v == HEADER;
        }
    }
}
