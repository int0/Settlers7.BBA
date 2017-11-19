using System;
using System.Runtime.InteropServices;

namespace Settlers6.BBA
{
	class BBACrypt
	{
        #region Native
        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool DecryptHeader(byte[] Header, int HeaderSize, byte[] BBAHeader, int BBAHeaderSize);

        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool EncryptHeader(byte[] Header, int HeaderSize, byte[] BBAHeader, int BBAHeaderSize);

        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool DecryptIndexTable(byte[] Table, int TableSize, byte[] reserved1, int reserved2);

        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool EncryptIndexTable(byte[] Table, int TableSize, byte[] Key, int KeySize);

        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool DecryptFileHeader(byte[] FileHeader, int FileSize, byte[] IndexTableEntry, int EntrySize, bool UseTableKey );

        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern bool EncryptFileHeader(byte[] FileHeader, int FileSize, byte[] IndexTableEntry, int EntrySize);
        
        [DllImport("s7crypto.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern void SetMode(int IsDemo);        
        #endregion
	}
}