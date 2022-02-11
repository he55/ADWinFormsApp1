using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //byte[] buf = Encoding.UTF8.GetBytes(textBox2.Text);

            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }

    public struct MSG
    {
        const uint HEADER = 0xadadadad;
        uint header;
        uint type;

        public MSG(uint type)
        {
            this.header = HEADER;
            this.type = type;
        }

        public byte[] ToArr()
        {
            byte[] buf = new byte[8];
            byte[] vs = BitConverter.GetBytes(this.header);
            byte[] vs2 = BitConverter.GetBytes(this.type);
            Array.Copy(vs, 0, buf, 0, 4);
            Array.Copy(vs2, 0, buf, 4, 4);

            return buf;
        }

        public static MSG hello = new MSG(0x1);
        public static MSG helloOK = new MSG(0x2);
        public static MSG send = new MSG(0x3);
        public static MSG sendOK = new MSG(0x4);

        public static MSG ToMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 4);
            return new MSG(v);
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
