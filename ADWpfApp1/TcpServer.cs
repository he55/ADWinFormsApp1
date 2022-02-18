using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADWpfApp1
{
    public class TcpServer
    {
        const int BufferSize = 8192;
        static string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static void StartClientTcp(string path, IPEndPoint remoteEP)
        {
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);

            int hash = 0;
            byte[] hBuffer =BitConverter.GetBytes(hash);
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
                        }
                    }
                }
            }

            // Release the socket.  
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public static void StartServerTcp(IPEndPoint remoteEP)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(remoteEP);
            sock.Listen(1);

            Task.Run(() =>
            {
                while (true)
                {
                    Socket client = sock.Accept();
                    if (client.Connected)
                    {
                        Thread cThread = new Thread(new ParameterizedThreadStart(myClient));
                        cThread.IsBackground = true;
                        cThread.Start(client);
                    }
                }
            });
        }

        static void myClient(object oSocket)
        {
            Socket handler = (Socket)oSocket;

            byte[] hashBuffer = new byte[4];
            handler.Receive(hashBuffer);
            int hash=BitConverter.ToInt32(hashBuffer, 0);

            MyDownloadFileInfo downloadFileInfo = MyDownloadFileInfo.DownloadFileInfo;
            if(downloadFileInfo?.Hash == hash)
            {
                byte[] okBuffer = new byte[4] {1,1,1,1};
                handler.Send(okBuffer);

                string path1 = Path.Combine(SavePath, downloadFileInfo.FileName);
                using (FileStream writer = new FileStream(path1, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    long receive = 0L;
                    int received;
                    byte[] fileBuffer = new byte[BufferSize];

                    while (receive < downloadFileInfo.Len)
                    {
                        received = handler.Receive(fileBuffer);
                        writer.Write(fileBuffer, 0, received);
                        receive += received;
                    }
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}
