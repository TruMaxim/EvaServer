using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NLog;

namespace network
{
    public abstract class NetConnection : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected Socket _socket = null;
        protected bool _connect = true;
        protected string _address;
        protected int _port;
        protected NetworkStream _networkStream = null;
        protected bool _authentification = false;
        private int _firstpacklimit = 1024;

        public abstract bool PacketParse(PacketReader pack, NetConnection connection);

        public abstract void Disconnect();

        public abstract void SendFirstPacket(NetworkStream stream);

        public NetConnection()
        {
            _connect = false;
        }

        public void setFirstPackLimit(int LimitBytes)
        {
            _firstpacklimit = LimitBytes;
        }

        public NetConnection(Socket socket)
        {
            _socket = socket;
            Thread clientThread = new Thread(this.Run);
            clientThread.IsBackground = true;
            clientThread.Start();
            _connect = true;
        }

        public bool CreateConnection(string address, int port)
        {
            _address = address;
            _port = port;
            IPEndPoint _ipPoint = new IPEndPoint(IPAddress.Parse(_address), _port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(_ipPoint);
                if (!_socket.Connected)
                    return false;
                _connect = true;
                _networkStream = new NetworkStream(_socket);
                //Thread ClientThread = new Thread(delegate () { Run(networkStream); });
                Thread ClientThread = new Thread(Run);
                ClientThread.IsBackground = true;
                ClientThread.Start();
            }
            catch (Exception e) { logger.Error("NetConnection > CreateConnection : " + e.Message); _connect = false; return false; }
            logger.Info("NetConnection : Connect to " + _address + " : " + port.ToString());
            //Console.WriteLine("NetConnection : Connect to " + _address + " : " + port.ToString());
            return true;
        }

        public void Dispose()
        {
            CloseConnection();
            try
            {
                _networkStream.Dispose();
                _socket.Dispose();
            }
            catch (Exception e) { logger.Error("NetConnection > Dispose : " + e.Message); }
        }

        public void CloseConnection()
        {
            try
            {
                _connect = false;
                if (_networkStream != null)
                    _networkStream.Close();
                if (_socket != null)
                    _socket.Close();
            }
            catch (Exception e) { logger.Error("NetConnection > CloseConnection : " + e.Message); }
        }

        private void Run()
        {
            byte[] buf;
            int newBytes = 0;
            int receivedBytes = 0;
            int length = 0;

            int lengthHi = 0;
            int lengthMidHi = 0;
            int lengthMidLo = 0;
            int lengthLo = 0;

            try
            {
                if (_socket == null)
                    return;
                _networkStream = new NetworkStream(_socket);
                if (_networkStream == null)
                    return;

                logger.Info("Client Connect from : " + _socket.RemoteEndPoint.AddressFamily.ToString());

                if (_socket.Connected && _connect)
                    SendFirstPacket(_networkStream);

                while (_socket.Connected && _connect)
                {
                    // Read Low to High
                    lengthLo = _networkStream.ReadByte();
                    lengthMidLo = _networkStream.ReadByte();
                    lengthMidHi = _networkStream.ReadByte();
                    lengthHi = _networkStream.ReadByte();

                    if (lengthHi < 0 || lengthMidLo < 0 || lengthMidHi < 0 || lengthLo < 0)
                    {
                        logger.Warn("NetConnection > Run : Connection terminated.");
                        break;
                    }

                    length = (lengthHi << 24) + (lengthMidHi << 16) + (lengthMidLo << 8) + lengthLo;

                    if (!_authentification && _firstpacklimit < length)
                    {
                        logger.Warn("NetConnection > Run : First Packet too much.");
                        break;
                    }

                    logger.Debug("Recive Packet > length : " + length.ToString());
                    receivedBytes = 0;
                    newBytes = 0;
                    buf = new byte[length];
                    while (newBytes != -1 && receivedBytes < length)
                    {
                        newBytes = _networkStream.Read(buf, receivedBytes, length - receivedBytes);
                        receivedBytes += newBytes;
                    }

                    // Command to Close Connection
                    if (buf[0] == 0x00 && buf[1] == 0x00)
                    {
                        break;
                    }

                    if (receivedBytes != length)
                    {
                        logger.Error("NetConnection  : Run > Incomplete Packet, closing connection.");
                        break;
                    }

                    // TODO Authorization
                    if (!PacketParse(new PacketReader(buf), this))
                        _connect = false;

                    if (!_authentification)
                    {
                        _connect = false;
                        logger.Error("NetConnection : Authorization > Authorization failed...");
                    }
                }

                logger.Warn("NetConnection : Run > Connection the ending...");
            }
            catch (Exception ex) { logger.Error("NetConnection  : Run > " + ex.Message); }
            finally
            {
                try
                {
                    CloseConnection();
                }
                catch (Exception ex) { logger.Error("NetConnection Finally  : Run > " + ex.Message); }
                Disconnect();
            }
        }

        public NetworkStream getNetworkStream()
        {
            return _networkStream;
        }

        public void setAuth(bool FLG)
        {
            _authentification = FLG;
            if(_authentification)
            {
                SpamFilter.getInstance().RemoveHost(NetUtils.getRemoteIP(_socket));
            }
        }

        public bool isAuth()
        {
            return _authentification;
        }

        public bool isConnected()
        {
            return _connect;
        }
    }
}
