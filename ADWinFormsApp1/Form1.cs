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

            if (Debugger.IsAttached)
            {
                button1.Enabled = true;
                IPEndPoint iPEndPoint1 = new IPEndPoint(IPAddress.Any, port);
                socket1.Bind(iPEndPoint1);
            }
        }

        List<string> ips = new List<string>();

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            Task.Run(() =>
            {
                met();
            });
        }


        void met()
        {
            byte[] buf = new byte[100];
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                int len = socket1.ReceiveFrom(buf, ref ep);
                if (len >= 8 && MSG.IsMSG(buf))
                {
                    MSG msg = MSG.ToMSG(buf);
                    if (msg == MSG.hello)
                    {
                        byte[] buf2 = MSG.helloOK.ToArr();
                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                    }
                    else if (msg == MSG.helloOK)
                    {
                        ips.Add($"{ep}");
                        this.Invoke(new Action(() =>
                        {
                            listBox2.Items.Add($"{ep}");
                        }));
                    }
                    else if (msg == MSG.send)
                    {
                        ServerTcp.StartServerTcp();

                        byte[] buf2 = MSG.sendOK.ToArr();
                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                    }
                    else if (msg == MSG.sendOK)
                    {
                        Task.Run(() =>
                        {
                            string filePath = @"C:\Users\luckh\Desktop\vmware.exe";
                            ClientTcp.StartClientTcp(filePath);
                        });
                    }
                    else if (msg == MSG.str)
                    {
                        string v = msg.ToStringData();
                    }

                    // log
                    this.Invoke(new Action(() =>
                    {
                        listBox1.Items.Add($"{ep} =>");
                    }));
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            byte[] buf = MSG.hello.ToArr();
            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MSG msg = MSG.str;
            msg.AddStringData(textBox2.Text);
            byte[] buf = msg.ToArr();

            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }
    }

    public struct MSG
    {
        const uint HEADER = 0xadadadad;
        uint header;
        uint type;
        int len;
        byte[] data;

        public MSG(uint type)
        {
            this.header = HEADER;
            this.type = type;
            this.len = 0;
            this.data = null;
        }

        public void AddStringData(string str)
        {
            byte[] vs = Encoding.UTF8.GetBytes(str);
            this.len = vs.Length;
            this.data = vs;
        }

        public string ToStringData()
        {
            return Encoding.UTF8.GetString(this.data);
        }

        public byte[] ToArr()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(this.header);
                    binaryWriter.Write(this.type);
                    binaryWriter.Write(this.len);
                    binaryWriter.Write(this.data);
                }
                return memoryStream.ToArray();
            }
        }

        public static MSG hello = new MSG(0x1);
        public static MSG helloOK = new MSG(0x2);
        public static MSG send = new MSG(0x3);
        public static MSG sendOK = new MSG(0x4);
        public static MSG str = new MSG(0x5);

        public static MSG ToMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            if (v != HEADER)
            {
                throw new Exception();
            }

            MSG msg;
            msg.header = HEADER;
            msg.type = BitConverter.ToUInt32(buf, 4);
            msg.len = BitConverter.ToInt32(buf, 8);
            msg.data = null;

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

        public override bool Equals(object obj)
        {
            return obj is MSG mSG &&
                   type == mSG.type;
        }

        public override int GetHashCode()
        {
            return 34944597 + type.GetHashCode();
        }

        public static bool operator ==(MSG left, MSG right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MSG left, MSG right)
        {
            return !(left == right);
        }
    }
}
