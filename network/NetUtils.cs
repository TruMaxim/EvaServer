using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace network
{
    public static class NetUtils
    {
        public static string getRemoteIP(Socket socket)
        {
            return Convert.ToString(((IPEndPoint)socket.RemoteEndPoint).Address);
        }
    }
}
