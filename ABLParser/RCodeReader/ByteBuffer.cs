using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader
{
    public class ByteBuffer
    {
        private readonly byte[] data;
        private readonly int length;
        
        public ByteBuffer(byte[] src)
        {
            data = src ?? throw new NullReferenceException("Source data cant't be null");
            length = src.Length;
        }

        public int Length => length;

        public byte[] GetData() => data;

        public short GetShort() => GetShort(0);
        public short GetShort(int pos) => BitConverter.ToInt16(data, pos);

        public int GetInt() => GetInt(0);
        public int GetInt(int pos) => BitConverter.ToInt32(data, pos);

        public long GetLong() => GetLong(0);
        public long GetLong(int pos) => BitConverter.ToInt64(data, pos);

        public ByteBuffer Order(bool isLittleEndian)
        {
            byte[] dst = new byte[length];
            data.CopyTo(dst, 0);            
            if (isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(dst);
            }
            return new ByteBuffer(dst);
        }

        public static ByteBuffer Wrap(byte[] src, uint pos, uint length)
        {
            byte[] dst = new byte[length];
            Array.Copy(src, pos, dst, 0, length);
            return new ByteBuffer(dst);
        }        
    }
}
