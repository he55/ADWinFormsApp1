using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ADWinFormsApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        const int PORT = 12500;

        Socket socket1;
        bool isInitServer;
        IPAddress ipAddress;

        public ObservableCollection<UserInfo> Devices { get; set; } = new ObservableCollection<UserInfo>();

        public Window1()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ipAddress = GetIPAddr();

            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            socket1.Bind(localEndPoint);

            Task.Run(() => OnRec());
        }

        static IPAddress GetIPAddr()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var item in ipHostInfo.AddressList)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte v = (byte)(item.Address >> 16);
                    if (v == 0)
                    {
                        return item;
                    }
                }
            }

            return null;
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

                        socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, PORT));
                    }
                    else if (msg.type == ADMsgType.helloOK)
                    {
                        UserInfo userInfo = new UserInfo();
                        userInfo.Name = msg.ToNameData();
                        userInfo.IP = ((IPEndPoint)ep).Address.Address;
                        userInfo.IPString = ((IPEndPoint)ep).Address.ToString();

                        this.Dispatcher.Invoke(() =>
                        {
                            Devices.Add(userInfo);
                        });

                        iPEndPoint2 = new IPEndPoint(((IPEndPoint)ep).Address, PORT);
                    }
                    else if (msg.type == ADMsgType.sendFile)
                    {
                        msg.ToFileData();

                        if (true)
                        {
                            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8080);
                            if (!isInitServer)
                            {
                                TcpServer.StartServerTcp(remoteEP);
                                isInitServer = true;
                            }

                            ADMsg msg2 = new ADMsg(ADMsgType.sendFileOK);
                            msg2.AddIPData(remoteEP);
                            byte[] buf2 = msg2.ToArr();
                            socket1.SendTo(buf2, new IPEndPoint(((IPEndPoint)ep).Address, PORT));
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
                    //this.Invoke(new Action(() =>
                    //{
                    //    listBox1.Items.Add($"{ep} => {msg.type} : {msg.ToStringData()}");
                    //}));
                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Devices.Clear();

            byte[] buf = new ADMsg(ADMsgType.hello).ToArr();
            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Broadcast, PORT);
            socket1.SendTo(buf, iPEndPoint2);
        }


        IPEndPoint iPEndPoint2;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendString);
            msg.AddStringData(textBox1.Text);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendUrl);
            msg.AddUrlData("https://devblogs.microsoft.com/");
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }

        string filePath = @"C:\Users\luckh\Desktop\vmware.exe";
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ADMsg msg = new ADMsg(ADMsgType.sendFile);
            msg.AddFileData(filePath);
            byte[] buf = msg.ToArr();

            socket1.SendTo(buf, iPEndPoint2);
        }
    }
}
