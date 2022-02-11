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
                byte[] buf = new byte[100];
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    int len = socket1.ReceiveFrom(buf, ref ep);
                    string str = Encoding.UTF8.GetString(buf, 0, len);
                    if (str == "hello")
                    {
                        byte[] buf2 = Encoding.UTF8.GetBytes("ok");
                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, port));
                    }
                    else if (str == "ok")
                    {
                        ips.Add($"{ep}");
                        this.Invoke(new Action(() =>
                        {
                            listBox2.Items.Add($"{ep}");
                        }));
                    }
                    else if (str == "send")
                    {
                        ServerTcp.StartServerTcp();
                    }
                    else if (str == "sendOK")
                    {
                        ClientTcp.StartClientTcp("");
                    }

                    // log
                    this.Invoke(new Action(() =>
                {
                    listBox1.Items.Add($"{ep} => {str}");
                }));
                }
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] buf = Encoding.UTF8.GetBytes(textBox2.Text);

            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);
            socket1.SendTo(buf, iPEndPoint2);
        }
    }
}
