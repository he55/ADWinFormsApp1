using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using IDataObject_Com = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const int PORT = 12500;

        Socket socket1;
        readonly IPEndPoint BroadcastEP = new IPEndPoint(IPAddress.Broadcast, PORT);
        IPAddress ipAddress;
        IPEndPoint remoteEP;
        IPEndPoint selectEP;
        string filePath;
        int selectedIndex;

        public ObservableCollection<UserInfo> Devices { get; set; } = new ObservableCollection<UserInfo>();

        #region LocalUserInfo

        private string userName;

        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        UserName = MachineName;
                    }
                    else
                    {
                        userName = value;
                        NotifyPropertyChanged();
                        SendInfo();
                    }
                }
            }
        }

        public string MachineName { get; set; }
        public string IPString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            ipAddress = Helper.GetIPAddr();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            socket1.Bind(localEndPoint);

            remoteEP = new IPEndPoint(ipAddress, PORT);
            TcpServer.StartServerTcp(remoteEP);

            UserName = MachineName = Dns.GetHostName();
            IPString = ipAddress.ToString();
            NotifyPropertyChanged("MachineName");
            NotifyPropertyChanged("IPString");

            Task.Run(() => { OnReceive(); });
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            listBox1.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                listBox1.Focus();
        }

        void OnReceive()
        {
            byte[] buf = new byte[1024];

            while (true)
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int len = socket1.ReceiveFrom(buf, ref ep);
                if (len >= 8 && ADMsg.IsMSG(buf))
                {
                    ADMsg msg = ADMsg.ToMSG(buf);
                    ADMsgType msgType = (ADMsgType)msg.GetMsgType();

                    IPAddress address2 = ((IPEndPoint)ep).Address;
                    IPEndPoint remoteEP2 = new IPEndPoint(address2, PORT);

                    if (msgType == ADMsgType.hello)
                    {
                        UserInfo userInfo = new UserInfo();
                        userInfo.UserName = msg.ToStringData();
                        userInfo.IP = address2.Address;
                        userInfo.IPString = address2.ToString();

                        this.Dispatcher.Invoke(() =>
                        {
                            Devices.Add(userInfo);
                        });


                        byte[] buf2 = ADMsg.helloOKData(UserName).ToArr();
                        socket1.SendTo(buf2, remoteEP2);
                    }
                    else if (msgType == ADMsgType.helloOK || msgType == ADMsgType.sendInfo)
                    {
                        long address = address2.Address;
                        if (ipAddress.Address != address)
                        {
                            UserInfo userInfo = new UserInfo();
                            userInfo.UserName = msg.ToStringData();
                            userInfo.IP = address;
                            userInfo.IPString = address2.ToString();

                            this.Dispatcher.Invoke(() =>
                            {
                                for (int i = 0; i < Devices.Count; i++)
                                {
                                    if (Devices[i].IP == address)
                                    {
                                        Devices.RemoveAt(i);
                                        Devices.Insert(i, userInfo);
                                        return;
                                    }
                                }
                                Devices.Add(userInfo);
                            });
                        }
                    }
                    else if (msgType == ADMsgType.sendFile)
                    {
                        MyDownloadFileInfo downloadFileInfo = msg.ToFileData();

                        this.Dispatcher.Invoke(async () =>
                        {
                            this.Activate();

                            ContentDialogExample dialog = new ContentDialogExample();
                            dialog.PrimaryButtonText = "保存到桌面";
                            dialog.TextBlock1.Text = "接收来自 {0} 的文件";
                            dialog.TextBlock2.Text = downloadFileInfo.FileName;
                            ContentDialogResult result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                MyDownloadFileInfo.DownloadFileInfos.Add(downloadFileInfo);

                                byte[] buf2 = ADMsg.sendFileOKData(remoteEP).ToArr();
                                socket1.SendTo(buf2, remoteEP2);
                            }
                            else
                            {
                                byte[] buf2 = ADMsg.sendFileCancelData().ToArr();
                                socket1.SendTo(buf2, remoteEP2);
                            }
                        });
                    }
                    else if (msgType == ADMsgType.sendFileOK)
                    {
                        IPEndPoint remoteEP = msg.ToIPData();
                        TcpServer.StartClientTcp(filePath, remoteEP);
                    }
                    else if (msgType == ADMsgType.sendFileCancel)
                    {
                    }
                    else if (msgType == ADMsgType.sendUrl)
                    {
                        string url = msg.ToStringData();

                        this.Dispatcher.Invoke(async () =>
                        {
                            this.Activate();

                            ContentDialogExample dialog = new ContentDialogExample();
                            dialog.PrimaryButtonText = "在浏览器中打开";
                            dialog.TextBlock1.Text = "接收来自 {0} 的链接";
                            dialog.TextBlock2.Text = url;
                            ContentDialogResult result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                Process.Start(url);
                            }
                        });
                    }
                    else if (msgType == ADMsgType.sendString)
                    {
                        string v = msg.ToStringData();
                    }

                    Debug.WriteLine($"{ep} => {msgType} : {msg.ToStringData()}");
                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Devices.Clear();

            byte[] buf = ADMsg.helloData(UserName).ToArr();
            socket1.SendTo(buf, BroadcastEP);
        }

        void SendInfo()
        {
            byte[] buf = ADMsg.sendInfoData(UserName).ToArr();
            socket1.SendTo(buf, BroadcastEP);
        }

        void DataActionMet(DataObject data)
        {
            selectEP = new IPEndPoint(Devices[selectedIndex].IP, PORT);

            if (data.ContainsFileDropList())
            {
                filePath = data.GetFileDropList()[0];
                byte[] buf = ADMsg.sendFileData(filePath).ToArr();
                socket1.SendTo(buf, selectEP);
            }
            else if (data.ContainsText())
            {
                string str = data.GetText();
                if (str.StartsWith("http"))
                {
                    byte[] buf = ADMsg.sendUrlData(str).ToArr();
                    socket1.SendTo(buf, selectEP);
                }
                else
                {
                    byte[] buf = ADMsg.sendStringData(str).ToArr();
                    socket1.SendTo(buf, selectEP);
                }
            }
        }


        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
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

            Point point = e.GetPosition(listBox1);
            int v = MyHelper.GetIndexAtPoint(listBox1, point);

            if (selectedIndex != -1)
                Devices[selectedIndex].IsSel = false;

            if (v != -1)
                Devices[v].IsSel = true;

            selectedIndex = v;

            Debug.WriteLine($"sel: {selectedIndex} {point}");
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            ddHelper.DragLeave();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Win32Point wp = GetWin32Point(e);
            ddHelper.Drop(e.Data as IDataObject_Com, ref wp, (int)e.Effects);

            if (selectedIndex != -1)
            {
                Devices[selectedIndex].IsSel = false;
                DataActionMet((DataObject)e.Data);
            }
        }

        #endregion

    }
}
