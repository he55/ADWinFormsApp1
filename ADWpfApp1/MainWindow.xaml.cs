using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        Settings _settings = Settings.Load();

        #region LocalUserInfo

        public string UserName
        {
            get => _settings.UserName ?? MachineName;
            set
            {
                if (_settings.UserName == value)
                    return;

                if (!string.IsNullOrWhiteSpace(value))
                    _settings.UserName = value;
                else
                    _settings.UserName = MachineName;

                NotifyPropertyChanged();
                SendTo(ADMsg.sendInfoData(_settings.UserName), BroadcastEP);
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
            MachineName = Dns.GetHostName();
            this.DataContext = this;
        }

        private  void Window_Loaded(object sender, RoutedEventArgs e)
        {
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            ipAddress = Helper2.GetIPAddr();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            socket1.Bind(localEndPoint);
            Task.Run(() => { OnReceive(); });

            remoteEP = new IPEndPoint(ipAddress, PORT);
            TcpServer.StartServerTcp(remoteEP);

            IPString = ipAddress.ToString();
            NotifyPropertyChanged("IPString");

            SendTo(ADMsg.helloData(UserName), BroadcastEP);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            textBox1.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                textBox1.Focus();
        }

        ContentDialogExample dialog1;
        void OnReceive()
        {
            byte[] buf = new byte[1024];

            while (true)
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int len = socket1.ReceiveFrom(buf, ref ep);
                if (len < 8 || !ADMsg.IsMSG(buf))
                    continue;

                ADMsg msg = ADMsg.ToMSG(buf);
                ADMsgType msgType = (ADMsgType)msg.GetMsgType();

                IPAddress address2 = ((IPEndPoint)ep).Address;
                IPEndPoint remoteEP2 = new IPEndPoint(address2, PORT);

                if (msgType == ADMsgType.hello)
                {
                    AddDevice(address2, msg);
                    SendTo(ADMsg.helloOKData(UserName), remoteEP2);
                }
                else if (msgType == ADMsgType.helloOK || msgType == ADMsgType.sendInfo)
                {
                    AddDevice(address2, msg);
                }
                else if (msgType == ADMsgType.sendFile)
                {
                    MyDownloadFileInfo downloadFileInfo = msg.ToFileData();
                    string name = leida.GetUserName(address2.Address);

                    this.Dispatcher.Invoke(async () =>
                    {
                        this.Activate();

                        dialog1 = new ContentDialogExample();
                        dialog1.PrimaryButtonText = "保存到桌面";
                        dialog1.TextBlock1.Text = $"接收来自 {name} 的文件";
                        dialog1.TextBlock2.Text = downloadFileInfo.FileName;
                        ContentDialogResult result = await dialog1.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            ContentDialogExample2 dialog2 = new ContentDialogExample2();
                            dialog2.TextBlock1.Text = "正在接收文件...";
                            dialog2.TextBlock2.Text = downloadFileInfo.FileName;
                            dialog2.ShowAsync();

                            TcpServer.ReceiveFileProgressCallback = (double val) =>
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    dialog2.ProgressBar1.Value = val * 100;
                                    if (val == 1.0)
                                    {
                                        TcpServer.ReceiveFileProgressCallback = null;
                                        dialog2.Hide();

                                        Process.Start("explorer.exe", $"/select,{TcpServer.CurrentSaveFilePath}");
                                        TcpServer.CurrentSaveFilePath = null;
                                    }
                                });
                            };

                            MyDownloadFileInfo.DownloadFileInfos.Add(downloadFileInfo);

                            SendTo(ADMsg.sendFileOKData(remoteEP), remoteEP2);
                        }
                        else if (result == ContentDialogResult.Secondary)
                        {
                            SendTo(ADMsg.sendFileCancelData(), remoteEP2);
                        }
                    });
                }
                else if (msgType == ADMsgType.sendFileOK)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Activate();
                        contentDialog2.Hide();

                        ContentDialogExample2 dialog2 = new ContentDialogExample2();
                        dialog2.TextBlock1.Text = "正在传送文件...";
                        dialog2.TextBlock2.Text = Path.GetFileName(filePath);
                        dialog2.ShowAsync();

                        TcpServer.SendFileProgressCallback = (double val) =>
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                dialog2.ProgressBar1.Value = val * 100;
                                if (val == 1.0)
                                {
                                    TcpServer.SendFileProgressCallback = null;
                                    dialog2.Hide();
                                }
                            });
                        };
                    });

                    IPEndPoint remoteEP = msg.ToIPData();
                    TcpServer.StartClientTcp(filePath, remoteEP);
                }
                else if (msgType == ADMsgType.sendFileCancel)
                {
                    this.Dispatcher.Invoke(async () =>
                    {
                        if (contentDialog2 == null)
                            return;

                        this.Activate();
                        contentDialog2.Hide();

                        ContentDialog contentDialog = new ContentDialog();
                        contentDialog.Content = "对方已取消文件传送";
                        contentDialog.PrimaryButtonText = "好";
                        contentDialog.DefaultButton = ContentDialogButton.Primary;
                        await contentDialog.ShowAsync();
                    });
                }
                else if (msgType == ADMsgType.sendFileCancel2)
                {
                    this.Dispatcher.Invoke(async () =>
                    {
                        if (dialog1 == null)
                            return;

                        this.Activate();
                        dialog1.Hide();

                        ContentDialog contentDialog = new ContentDialog();
                        contentDialog.Content = "对方已取消文件传送";
                        contentDialog.PrimaryButtonText = "好";
                        contentDialog.DefaultButton = ContentDialogButton.Primary;
                        await contentDialog.ShowAsync();
                    });
                }
                else if (msgType == ADMsgType.sendUrl)
                {
                    string url = msg.ToStringData();
                    string name = leida.GetUserName(address2.Address);

                    this.Dispatcher.Invoke(async () =>
                    {
                        this.Activate();

                        ContentDialogExample dialog = new ContentDialogExample();
                        dialog.PrimaryButtonText = "在浏览器中打开";
                        dialog.TextBlock1.Text = $"接收来自 {name} 的链接";
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

        void AddDevice(IPAddress address2, ADMsg msg)
        {
            long address = address2.Address;
            //if (ipAddress.Address == address)
            //    return;

            UserInfo userInfo = new UserInfo();
            userInfo.UserName = msg.ToStringData();
            userInfo.IP = address;
            userInfo.IPString = address2.ToString();

            this.Dispatcher.Invoke(() =>
            {
                leida.AddDevice(userInfo);
            });
        }

        void SendTo(ADMsg msg, IPEndPoint endPoint)
        {
            byte[] buf = msg.ToArr();
            socket1.SendTo(buf, endPoint);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            leida.Clean();
            SendTo(ADMsg.helloData(UserName), BroadcastEP);
        }

        ContentDialog contentDialog2;
        async void DataActionMet(DataObject data,UserInfo userInfo)
        {
            selectEP = new IPEndPoint(userInfo.IP, PORT);

            if (data.ContainsFileDropList())
            {
                filePath = data.GetFileDropList()[0];
                SendTo(ADMsg.sendFileData(filePath), selectEP);

                this.Activate();
                contentDialog2 = new ContentDialog();
                contentDialog2.Title = "AirDrop";
                contentDialog2.Content = "等待对方接收文件";
                contentDialog2.PrimaryButtonText = "取消";
                contentDialog2.DefaultButton = ContentDialogButton.Primary;
                if (await contentDialog2.ShowAsync() == ContentDialogResult.Primary)
                {
                    SendTo(ADMsg.sendFileCancel2Data(), selectEP);
                }
            }
            else if (data.ContainsText())
            {
                string str = data.GetText();
                if (str.StartsWith("http"))
                {
                    SendTo(ADMsg.sendUrlData(str), selectEP);

                    this.Activate();
                    ContentDialog contentDialog = new ContentDialog();
                    contentDialog.Title = "AirDrop";
                    contentDialog.Content = "已发送";
                    contentDialog.PrimaryButtonText = "好";
                    contentDialog.DefaultButton = ContentDialogButton.Primary;
                    await contentDialog.ShowAsync();
                }
                else
                {
                    SendTo(ADMsg.sendStringData(str), selectEP);
                }
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

            Point point = e.GetPosition(leida);
            leida.SetPoint(point);

            Debug.WriteLine($"point: {point}");
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            ddHelper.DragLeave();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Win32Point wp = GetWin32Point(e);
            ddHelper.Drop(e.Data as IDataObject_Com, ref wp, (int)e.Effects);

            if (leida.SelectUserInfo!=null)
            {
                DataActionMet((DataObject)e.Data,leida.SelectUserInfo);
                leida.ResetBackground();
            }
        }

        #endregion

    }
}
