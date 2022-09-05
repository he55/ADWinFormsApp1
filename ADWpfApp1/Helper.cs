using System.Net;
using System.Net.Sockets;

namespace ADWpfApp1
{
    public static class Helper
    {
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
