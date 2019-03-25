using System;
using System.Collections.Generic;
using System.Text;

namespace network
{
    public class PacketReader
    {
        private ushort _PacketID;
        private static byte[] _buf = null;
        private int readpos = 0; 

        public PacketReader(byte[] buf)
        {
            _buf = buf;
            _PacketID = readUShort();
        }

        public ushort getPacketID()
        {
            return _PacketID;
        }

        public bool checkdata(int length)
        {
            if (_buf == null || (_buf.Length - readpos - length) < 0)
                return false;
            return true;
        }

        public byte readByte()
        {
            if (!checkdata(1))
                return 0;
            readpos++;
            return _buf[readpos - 1];
        }

        public ushort readUShort()
        {
            if (!checkdata(2))
                return 0;
            readpos += 2;
            return BitConverter.ToUInt16(_buf, readpos - 2);
        }

        public Int16 readShort()
        {
            if (!checkdata(2))
                return 0;
            readpos += 2;
            return BitConverter.ToInt16(_buf, readpos - 2);
        }

        public Int32 readInteger()
        {
            if (!checkdata(4))
                return 0;
            readpos += 4;
            return BitConverter.ToInt32(_buf, readpos - 4);
        }

        public long readLong()
        {
            if (!checkdata(8))
                return 0;
            readpos += 8;
            return BitConverter.ToInt64(_buf, readpos - 8);
        }

        public float readFloat()
        {
            if (!checkdata(4))
                return 0;
            readpos += 4;
            return BitConverter.ToSingle(_buf, readpos - 4);
        }

        public byte[] readByteArray()
        {
            if (!checkdata(4))
                return null;
            int length = readInteger();
            if (!checkdata(length))
                return null;
            byte[] dest = new byte[length];
            Array.Copy(_buf, readpos, dest, 0, length);
            readpos += length;
            return dest;
        }

        public string readString()
        {
            if (!checkdata(4))
                return "";
            int length = readInteger();
            if (!checkdata(2 * length))
                return "";
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = Convert.ToChar(readShort());
            return new string(chars);
        }
    }
}
