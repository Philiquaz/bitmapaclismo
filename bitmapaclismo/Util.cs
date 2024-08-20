using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;

namespace bitmapaclismo
{
    static class Util
    {
        public static byte readByte(byte[] bytes, int address)
        {
            return bytes[address];
        }
        public static byte[] readBytes(byte[] bytes, int address, int count)
        {
            return bytes.AsSpan(address, count).ToArray();
        }
        public static T[] readEnumBytes<T>(byte[] bytes, int address, int count) where T : struct, Enum
        {
            return MemoryMarshal.Cast<byte, T>(bytes.AsSpan(address, count)).ToArray();
        }
        public static int readInt(byte[] bytes, int address)
        {
            return BitConverter.ToInt32(bytes, address);
        }
        public static int[] readInts(byte[] bytes, int address, int count)
        {
            return MemoryMarshal.Cast<byte, int>(bytes.AsSpan(address, count * 4)).ToArray();
        }
        public static String readString(byte[] bytes, int address, int length)
        {
           return Encoding.ASCII.GetString(bytes, address, length); // asserting ascii usage
        }
        public static void writeByte(byte[] bytes, int address, byte newByte)
        {
            bytes[address] = newByte;
        }
        public static void writeBytes(byte[] bytes, int address, byte[] newBytes)
        {
            newBytes.CopyTo(bytes, address);
        }
        public static void writeEnumBytes<T>(byte[] bytes, int address, T[] newBytes) where T : struct, Enum
        {
            MemoryMarshal.Cast<T, byte>(newBytes).CopyTo(bytes.AsSpan(address, newBytes.Length));
        }
        public static void writeInt(byte[] bytes, int address, int value)
        {
            BitConverter.TryWriteBytes(bytes.AsSpan(address, 4), value);
        }
        public static void writeInts(byte[] bytes, int address, int[] newInts)
        {
            MemoryMarshal.Cast<int, byte>(newInts).CopyTo(bytes.AsSpan(address, newInts.Length * 4));
        }
        public static void writeString(byte[] bytes, int address, String value)
        {
            Encoding.ASCII.GetBytes(value, bytes.AsSpan(address, value.Length)); // asserting only ascii / single byte character usage
        }
    }
    public class ByteReader
    {
        int ptr;
        byte[] bytes;
        public ByteReader(byte[] data)
        {
            ptr = 0;
            bytes = data;
        }
        public byte readByte()
        {
            byte newByte = Util.readByte(bytes, ptr);
            ptr += 1;
            return newByte;
        }
        public byte[] readBytes(int length)
        {
            if (length == -1) length = bytes.Length - ptr;
            byte[] newBytes = Util.readBytes(bytes, ptr, length);
            ptr += length;
            return newBytes;
        }
        public T[] readEnumBytes<T>(int length) where T : struct, Enum
        {
            T[] newBytes = Util.readEnumBytes<T>(bytes, ptr, length);
            ptr += length;
            return newBytes;
        }
        public int readInt()
        {
            int r = Util.readInt(bytes, ptr);
            ptr += 4;
            return r;
        }
        public int[] readInts(int length)
        {
            int[] newInts = Util.readInts(bytes, ptr, length);
            ptr += length * 4;
            return newInts;
        }
        public String readString(int length)
        {
            String r = Util.readString(bytes, ptr, length);
            ptr += r.Length;
            return r;
        }
        public bool Finished()
        {
            return ptr == bytes.Length;
        }
    }
    public class ByteWriter
    {
        int size;
        byte[] bytes;
        public ByteWriter(int initialCapacity = 4096)
        {
            bytes = new byte[initialCapacity];
        }
        private void ensureNewCapacity(int addedLength)
        {
            if (size + addedLength >= bytes.Length)
                Array.Resize(ref bytes, (int)(bytes.Length * 1.5 + 1));
        }
        public void writeByte(byte newByte)
        {
            ensureNewCapacity(1);
            Util.writeByte(bytes, size, newByte);
            size += 1;
        }
        public void writeBytes(byte[] newBytes)
        {
            ensureNewCapacity(newBytes.Length);
            Util.writeBytes(bytes, size, newBytes);
            size += newBytes.Length;
        }
        public void writeEnumBytes<T>(T[] newBytes) where T : struct, Enum
        {
            ensureNewCapacity(newBytes.Length);
            Util.writeEnumBytes<T>(bytes, size, newBytes);
            size += newBytes.Length;
        }
        public void writeInt(int value)
        {
            ensureNewCapacity(4);
            Util.writeInt(bytes, size, value);
            size += 4;
        }
        public void writeInts(int[] ints)
        {
            ensureNewCapacity(ints.Length * 4);
            Util.writeInts(bytes, size, ints);
            size += ints.Length * 4;
        }
        public void writeString(String str)
        {
            ensureNewCapacity(str.Length);
            Util.writeString(bytes, size, str);
            size += str.Length;
        }
        public byte[] GetBytes() {
            return bytes.AsSpan(0, size).ToArray();
        }
    }
}