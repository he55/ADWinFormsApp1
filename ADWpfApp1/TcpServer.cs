using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ADWpfApp1
{
    public class TcpServer
    {
        const int BufferSize = 8192;

        public static string CurrentSaveFilePath;
        public static Action<double> SendFileProgressCallback;
        public static Action<double> ReceiveFileProgressCallback;

        public static void StartClientTcp(string path, IPEndPoint remoteEP)
        {
            Task.Run(() =>
            {
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);

                int hash = 0;
                byte[] hBuffer = BitConverter.GetBytes(hash);
                sender.Send(hBuffer);

                byte[] okBuffer = new byte[4];
                sender.Receive(okBuffer);

                if (okBuffer[0] == 1)
                {
                    using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        byte[] fileBuffer = new byte[BufferSize];
                        int read, sent;
                        while ((read = reader.Read(fileBuffer, 0, BufferSize)) != 0)
                        {
                            sent = 0;
                            while ((sent += sender.Send(fileBuffer, sent, read, SocketFlags.None)) < read)
                            {
                                read -= sent;
                            }

                            SendFileProgressCallback?.Invoke((double)reader.Position / reader.Length);
                        }
                    }
                }

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            });
        }

        public static void StartServerTcp(IPEndPoint remoteEP)
        {
            Task.Run(() =>
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Bind(remoteEP);
                sock.Listen(1);

                while (true)
                {
                    Socket client = sock.Accept();
                    if (client.Connected)
                    {
                        Task.Run(() => { NewClient(client); });
                    }
                }
            });
        }

        static void NewClient(Socket handler)
        {
            byte[] hashBuffer = new byte[4];
            handler.Receive(hashBuffer);
            int hash = BitConverter.ToInt32(hashBuffer, 0);

            MyDownloadFileInfo downloadFileInfo =MyDownloadFileInfo.Get(hash);
            if (downloadFileInfo != null)
            {
                byte[] okBuffer = new byte[4] { 1, 1, 1, 1 };
                handler.Send(okBuffer);

                string saveFilePath = Helper2.GetSafeFileName(downloadFileInfo.FileName);
                CurrentSaveFilePath = saveFilePath;
                using (FileStream writer = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    long receive = 0L;
                    int received;
                    byte[] fileBuffer = new byte[BufferSize];

                    while (receive < downloadFileInfo.Len)
                    {
                        received = handler.Receive(fileBuffer);
                        writer.Write(fileBuffer, 0, received);
                        receive += received;

                        ReceiveFileProgressCallback?.Invoke((double)receive / downloadFileInfo.Len);
                    }
                }
            }
            else
            {
                byte[] cancelBuffer = new byte[4] { 0, 0, 0, 0 };
                handler.Send(cancelBuffer);
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
