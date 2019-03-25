using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace network
{
    public abstract class NetServer
    {
        private string _address;
        private int _port;
        private bool _enable = true;
        private Socket listenSocket;
        public bool UseSpamFilter;
        public long SpamFilterTimeOutSec = 60;
        private SpamFilter _spamFilter;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public abstract void NewClient(Socket handler);

        public NetServer()
        {

        }

        public void StartServer(string address, int port)
        {
            _address = address;
            _port = port;
            Thread clientThread = new Thread(this.Run);
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void Run()
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_address), _port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(100);

                logger.Info("Server start.");
                _enable = true;

                while (_enable)
                {
                    Socket handler = listenSocket.Accept();
                    string RemoteHost = NetUtils.getRemoteIP(handler);
                    if (UseSpamFilter)
                    {
                        long _TimeSec = SpamFilter.getInstance().getTimeSecConnection(RemoteHost);
                        if (_TimeSec < 0)
                            CreateNewClient(handler);
                        else if (DateTime.Now.Ticks / 10000000L - _TimeSec > SpamFilterTimeOutSec)
                            CreateNewClient(handler);
                        else
                            logger.Warn("Connection block by IP : " + RemoteHost);
                    }
                    else
                        NewClient(handler);
                }
            }
            catch (Exception e) { Console.WriteLine("ERROR Server Listen > " + e.Message); return; }
            finally
            {
                CloseSocket();
            }
        }

        private void CreateNewClient(Socket handler)
        {
            if(UseSpamFilter)
            {
                SpamFilter.getInstance().setTimeSecConnection(NetUtils.getRemoteIP(handler));
                NewClient(handler);
            }
            else
                NewClient(handler);
        }

        public virtual void StopConnection()
        {
            _enable = false;
            CloseSocket();
        }

        private void CloseSocket()
        {
            try
            {
                if (listenSocket != null)
                    listenSocket.Close();
                listenSocket = null;
            }
            catch (Exception e) { Console.WriteLine("ERROR Server Listen : CloseSocket > " + e.Message); return; }
        }

    }
}
