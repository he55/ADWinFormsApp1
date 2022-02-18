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

            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                long length = reader.Length;

                string sendStr = "namelength," + Path.GetFileName(path) + "," + length.ToString();
                sender.Send(Encoding.Default.GetBytes(sendStr));

                byte[] buffer = new byte[32];
                sender.Receive(buffer);

                string mes = Encoding.Default.GetString(buffer);
                if (mes.Contains("OK"))
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

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }
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

            byte[] buffer = new byte[1024];
            int count = handler.Receive(buffer);

            string[] command = Encoding.Default.GetString(buffer, 0, count).Split(',');

            if (command[0] == "namelength")
            {
                string fileName = command[1];
                long length = Convert.ToInt64(command[2]);
                handler.Send(Encoding.Default.GetBytes("OK"));

                string path1 = Path.Combine(SavePath, fileName);
                using (FileStream writer = new FileStream(path1, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    long receive = 0L;
                    int received;
                    byte[] buffer2 = new byte[BufferSize];

                    while (receive < length)
                    {
                        received = handler.Receive(buffer2);
                        writer.Write(buffer2, 0, received);
                        receive += received;
                    }
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}
