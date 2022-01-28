using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ADWinFormsApp1
{
    class ServerTcp
    {
        const int BufferSize = 1024;
        static string path = @"D:\";

        static void StartServerTcp()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(ip);
            sock.Listen(1);
            Console.WriteLine("Begin listen...");

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
        }

        static void myClient(object oSocket)
        {
            Socket clientSocket = (Socket)oSocket;
            string clientName = clientSocket.RemoteEndPoint.ToString();
            Console.WriteLine("新来一个客户:" + clientName);
            try
            {

                byte[] buffer = new byte[BufferSize];
                int count = clientSocket.Receive(buffer);
                Console.WriteLine("收到" + clientName + ":" + Encoding.Default.GetString(buffer, 0, count));
                string[] command = Encoding.Default.GetString(buffer, 0, count).Split(',');
                string fileName;
                long length;
                if (command[0] == "namelength")
                {
                    fileName = command[1];
                    length = Convert.ToInt64(command[2]);
                    clientSocket.Send(Encoding.Default.GetBytes("OK"));
                    long receive = 0L;
                    Console.WriteLine("Receiveing file:" + fileName + ".Plz wait...");
                    using (FileStream writer = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        int received;
                        while (receive < length)
                        {
                            received = clientSocket.Receive(buffer);
                            writer.Write(buffer, 0, received);
                            writer.Flush();
                            receive += (long)received;
                        }
                    }
                    Console.WriteLine("Receive finish.\n");
                }

            }
            catch
            {
                Console.WriteLine("客户:" + clientName + "退出");
            }

        }
    }

}
