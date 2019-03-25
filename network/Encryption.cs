using System;
using System.Collections.Generic;
using System.Text;

namespace network
{
    public static class Encryption
    {
        private static byte[] _key = null;
        private static bool _EncryptEnable = false;

        // For OFF Encrypt, SET This function string is ""
        public static void SetKey(string key)
        {
            if(key.Length < 3)
            {
                _EncryptEnable = false;
                return;
            }
            _key = Encoding.ASCII.GetBytes(key);
            _EncryptEnable = true;
        }

        public static void Encrypt(byte[] arr)
        {
            if (!_EncryptEnable)
                return;
            crypt(arr, GetKeyPos());
            crypt(arr, 0);
        }

        public static void decryption(byte[] arr)
        {
            if (!_EncryptEnable)
                return;
            crypt(arr, 0);
            crypt(arr, GetKeyPos());
        }

        private static int GetKeyPos()
        {
            return _key.Length / 3;
        }

        private static void crypt(byte[] arr, int keystartpos)
        {
            int keypos = keystartpos;
            for (int i = 2; i < arr.Length; i++)
            {
                arr[i] ^= _key[keypos];
                keypos++;
                if (keypos > _key.Length)
                    keypos = 0;
            }
        }

    }
}
