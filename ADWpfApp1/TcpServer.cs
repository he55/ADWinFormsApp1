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

        public static Action<ProgressData> SendFileProgressCallback;

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
                        ProgressData progress = new ProgressData();
                        progress.Length = reader.Length;
                        progress.Position =0;
                        SendFileProgressCallback?.Invoke(progress);

                        byte[] fileBuffer = new byte[BufferSize];
                        int read, sent;
                        while ((read = reader.Read(fileBuffer, 0, BufferSize)) != 0)
                        {
                            sent = 0;
                            while ((sent += sender.Send(fileBuffer, sent, read, SocketFlags.None)) < read)
                            {
                                read -= sent;
                            }

                            progress.Position = reader.Position;
                            SendFileProgressCallback?.Invoke(progress);
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
                downloadFileInfo.SaveFilePath = saveFilePath;
                using (FileStream writer = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    ProgressData progress = new ProgressData();
                    progress.Length = downloadFileInfo.Len;
                    progress.Position = 0;
                    downloadFileInfo.ProgressCallback(progress);

                    long receive = 0;
                    int received;
                    byte[] fileBuffer = new byte[BufferSize];

                    while (receive < downloadFileInfo.Len)
                    {
                        received = handler.Receive(fileBuffer);
                        writer.Write(fileBuffer, 0, received);
                        receive += received;

                        progress.Position = receive;
                        downloadFileInfo.ProgressCallback(progress);
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
