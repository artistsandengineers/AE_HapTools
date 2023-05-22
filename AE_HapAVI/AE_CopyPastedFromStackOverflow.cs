using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace AE_HapTools
{
    public static class AE_CopyPastedFromStackOverflow
    {

        //http://stackoverflow.com/questions/4159184/c-read-structures-from-binary-file
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            stream.Read(buffer, 0, sz);
            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(
                pinnedBuffer.AddrOfPinnedObject(), typeof(T));
            pinnedBuffer.Free();
            return structure;
        }

        //Inspired by https://stackoverflow.com/questions/17338571/writing-bytes-from-a-struct-into-a-file-with-c-sharp not terribly happy about the copy but WHATEVERRRRRRRRRRR
        public static void WriteStruct<T>(this Stream stream, T structure) where T: struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(sz);
            byte[] bufferToWrite = new byte[sz];

            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, bufferToWrite, 0, sz);
            Marshal.FreeHGlobal(ptr);

            stream.Write(bufferToWrite, 0, sz);
        }

        public static byte[] structToByteArray<T>(T structure) where T: struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(sz);
            byte[] bufferToWrite = new byte[sz];

            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, bufferToWrite, 0, sz);
            Marshal.FreeHGlobal(ptr);

            return bufferToWrite;
        }

        //https://www.codeproject.com/articles/10613/c-riff-parser
        public static string fourCC2String(UInt32 fourcc)
        {
            char[] chars = new char[4];
            chars[0] = (char)(fourcc & 0xFF);
            chars[1] = (char)((fourcc >> 8) & 0xFF);
            chars[2] = (char)((fourcc >> 16) & 0xFF);
            chars[3] = (char)((fourcc >> 24) & 0xFF);

            return new string(chars);
        }

        //http://stackoverflow.com/questions/19496825/single-quoted-string-to-uint-in-c-sharp
        public static uint string2FourCC(string s)
        {
            if (s.Length != 4) throw new ArgumentException("Must be a four character string");
            var bytes = Encoding.UTF8.GetBytes(s);
            if (bytes.Length != 4) throw new ArgumentException("Must encode to exactly four bytes");
            return BitConverter.ToUInt32(bytes, 0);
        }

        //http://stackoverflow.com/questions/11642210/computing-padding-required-for-n-byte-alignment
        public static long calculatePad(long count, long align)
        {
            return (align - (count % align)) % align;

        }
    }
}
