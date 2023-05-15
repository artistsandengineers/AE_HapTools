using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace AE_HapTools
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_RIFFChunkHeader
    {
        public UInt32 fourcc;
        public UInt32 size;

        public string fourCCString
        {
            get
            {
                return AE_CopyPastedFromStackOverflow.fourCC2String(fourcc);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_RIFFListHeader
    {
        public UInt32 listFourCC;
        public UInt32 size;
        public UInt32 typeFourCC;

        public string listFourCCString
        {
            get
            {
                return AE_CopyPastedFromStackOverflow.fourCC2String(listFourCC);
            }
        }

        public string typeFourCCString
        {
            get
            {
                return AE_CopyPastedFromStackOverflow.fourCC2String(typeFourCC);
            }
        }


    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_AVIMainHeader
    {
        public UInt32 fourcc;
        public UInt32 size;
        public UInt32 microSecondsPerFrame;
        public UInt32 maxBytesPerSecond;
        public UInt32 paddingGranulatiry;
        public UInt32 flags;
        public UInt32 frameCount;
        public UInt32 initialFrames;
        public UInt32 streamCount;
        public UInt32 suggestedBufferSize;
        public UInt32 width;
        public UInt32 height;
        public UInt32 reserved0;
        public UInt32 reserved1;
        public UInt32 reserved2;
        public UInt32 reserved3;
    }

    struct AE_HapAVIframeIndexItem
    {
        public long position;
        public long length;
    }

    //This a bit erk, but:
    //* The names here match the 4CCs required in a DDS header.
    public enum AE_SurfaceCompressionType
    {
        DXT1 = 70, //Hap, 'Hap1', BC1
        DXT5 = 76 //Hap Alpha, 'Hap5', BC3
    }
    class AE_HapTypes
    {

    }
}
