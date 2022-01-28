using System;
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
        Socket socket2;
        IPEndPoint iPEndPoint1 = new IPEndPoint(IPAddress.Any, port);
        IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, port);

        private void Form1_Load(object sender, EventArgs e)
        {
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.Bind(iPEndPoint1);
            socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket2.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        }

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

                    string text = $"{ep} => {str}";
                    this.Invoke(new Action(() =>
                    {
                        listBox1.Items.Add(text);
                        if (str == "ok")
                        {
                            listBox2.Items.Add(text);
                        }
                    }));
                }
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] buf = Encoding.UTF8.GetBytes(textBox2.Text);
            socket2.SendTo(buf, iPEndPoint2);
        }
    }
}
