using System;
using System.Collections.Generic;
using System.Text;

namespace network
{
    public class SpamFilter
    {
        private static SpamFilter _instance;
        private SpamFilter() { }
        private static object syncRoot = new Object();
        private Dictionary<string, long> _connections = new Dictionary<string, long>();

        public static SpamFilter getInstance()
        {
            if (_instance == null)
                lock (syncRoot)
                {
                    if (_instance == null)
                        _instance = new SpamFilter();
                }
            return _instance;
        }

        public long getTimeSecConnection(string host)
        {
            long value;
            if (_connections.TryGetValue(host, out value))
                return value;
            else
                return -1;
        }

        public void setTimeSecConnection(string host)
        {
            if (getTimeSecConnection(host) != -1)
                RemoveHost(host);
            _connections.Add(host, DateTime.Now.Ticks / 10000000L);
        }

        public void RemoveHost(string host)
        {
            _connections.Remove(host);
        }
    }
}
