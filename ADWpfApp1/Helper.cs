using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ADWpfApp1
{
    public static class Helper
    {
        static readonly string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string GetSafeFileName(string fileName)
        {
            string saveFilePath = Path.Combine(SavePath, fileName);
            if (File.Exists(saveFilePath))
            {
                int nameIndex = 1;
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);

                do
                {
                    saveFilePath = Path.Combine(SavePath, $"{name} - {nameIndex}{ext}");
                    nameIndex++;
                } while (File.Exists(saveFilePath));
            }
            return saveFilePath;
        }

        public static IPAddress GetIPAddr()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var item in ipHostInfo.AddressList)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
