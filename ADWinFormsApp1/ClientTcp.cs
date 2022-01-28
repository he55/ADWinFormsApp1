using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ADWinFormsApp1
{
    class ClientTcp
    {
        static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main2(string[] args)
        {
            sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            Console.WriteLine("Connect successfully");
            while (true)
            {
                Console.WriteLine("please input the path of the file which you want to send：");
                string path = Console.ReadLine();
                try
                {
                    using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        long send = 0L, length = reader.Length;
                        string sendStr = "namelength," + Path.GetFileName(path) + "," + length.ToString();

                        string fileName = Path.GetFileName(path);
                        sock.Send(Encoding.Default.GetBytes(sendStr));

                        int BufferSize = 1024;
                        byte[] buffer = new byte[32];
                        sock.Receive(buffer);
                        string mes = Encoding.Default.GetString(buffer);

                        if (mes.Contains("OK"))
                        {
                            Console.WriteLine("Sending file:" + fileName + ".Plz wait...");
                            byte[] fileBuffer = new byte[BufferSize];
                            int read, sent;
                            while ((read = reader.Read(fileBuffer, 0, BufferSize)) != 0)
                            {
                                sent = 0;
                                while ((sent += sock.Send(fileBuffer, sent, read, SocketFlags.None)) < read)
                                {
                                    send += (long)sent;
                                }
                            }
                            Console.WriteLine("Send finish.\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}
