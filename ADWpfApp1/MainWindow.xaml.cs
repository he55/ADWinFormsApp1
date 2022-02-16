using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IDataObject_Com = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PORT = 12500;

        Socket socket1;
        bool isInitServer;
        IPAddress ipAddress;
        IPEndPoint selectEP;
        string filePath;

        public ObservableCollection<UserInfo> Devices { get; set; } = new ObservableCollection<UserInfo>();


        public MainWindow()
        {
            InitializeComponent();


            Devices.Add(new UserInfo { Name = "qwe", IPString = "ip" });
            Devices.Add(new UserInfo { Name = "asd", IPString = "cp" });
            Devices.Add(new UserInfo { Name = "zxc", IPString = "hp",IsSel=true });
            Devices.Add(new UserInfo { Name = "rty", IPString = "up" });
            Devices.Add(new UserInfo { Name = "fgh", IPString = "dp" });
            Devices.Add(new UserInfo { Name = "vbn", IPString = "yp" });

            ListBoxDragDropManager listBoxDragDropManager = new ListBoxDragDropManager(this.listBox1);
            listBoxDragDropManager.DataAction = DataActionMet;

            this.DataContext = this;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ipAddress = GetIPAddr() ?? IPAddress.Any;

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

                    Debug.WriteLine($"{ep} => {msg.type} : {msg.ToStringData()}");
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

        void DataActionMet(int sel, bool isfile, string str)
        {
            selectEP = new IPEndPoint(Devices[sel].IP, PORT);
            if (isfile)
            {
                filePath = str;
                ADMsg msg = new ADMsg(ADMsgType.sendFile);
                msg.AddFileData(str);
                byte[] buf = msg.ToArr();

                socket1.SendTo(buf, selectEP);
            }
            else
            {
                ADMsg msg = new ADMsg(ADMsgType.sendString);
                msg.AddStringData(str);
                byte[] buf = msg.ToArr();

                socket1.SendTo(buf, selectEP);
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            if (selectedIndex != -1)
            {
                selectEP = new IPEndPoint(Devices[selectedIndex].IP, PORT);
            }
        }


        #region DragDropHelper

        private IDropTargetHelper ddHelper = (IDropTargetHelper)new DragDropHelper();

        Win32Point GetWin32Point(DragEventArgs e)
        {
            Point p = this.PointToScreen(e.GetPosition(this));
            Win32Point wp;
            wp.x = (int)p.X;
            wp.y = (int)p.Y;
            return wp;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;

            Win32Point wp = GetWin32Point(e);

            ddHelper.DragEnter(new WindowInteropHelper(this).Handle, e.Data as IDataObject_Com, ref wp, (int)e.Effects);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            Win32Point wp = GetWin32Point(e);

            ddHelper.DragOver(ref wp, (int)e.Effects);
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            ddHelper.DragLeave();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Win32Point wp = GetWin32Point(e);

            ddHelper.Drop(e.Data as IDataObject_Com, ref wp, (int)e.Effects);
        }

        #endregion

    }
}
