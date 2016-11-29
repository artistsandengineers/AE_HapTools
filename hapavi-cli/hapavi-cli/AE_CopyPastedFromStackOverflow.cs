using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace hapavi_cli
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

        //https://www.codeproject.com/articles/10613/c-riff-parser
        public static string FourCCFromUInt32(UInt32 fourcc)
        {
            char[] chars = new char[4];
            chars[0] = (char)(fourcc & 0xFF);
            chars[1] = (char)((fourcc >> 8) & 0xFF);
            chars[2] = (char)((fourcc >> 16) & 0xFF);
            chars[3] = (char)((fourcc >> 24) & 0xFF);

            return new string(chars);
        }

        //http://stackoverflow.com/questions/11642210/computing-padding-required-for-n-byte-alignment
        public static long calculatePad(long count, long align)
        {
            return (align - (count % align)) % align;

        }
    }
}
