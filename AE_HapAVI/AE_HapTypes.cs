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

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_AVIStreamHeader
    {
        public UInt32 fourcc;
        public UInt32 fccHandler;
        public UInt32 flags;
        public UInt16 priority;
        public UInt16 language;
        public UInt32 initialFrames;
        public UInt32 scale;
        public UInt32 rate;
        public UInt32 start;
        public UInt32 length;
        public UInt32 suggestedBufferSize;
        public UInt32 quality;
        public UInt32 sampleSize;
        public UInt32 left;
        public UInt32 top;
        public UInt32 right;
        public UInt32 bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_AVIBitmapInfoHeader
    {
        public UInt32 size;
        public UInt32 width;
        public UInt32 height;
        public UInt16 planes;
        public UInt16 bitCount;
        public UInt32 compression;
        public UInt32 sizeImage;
        public UInt32 xPixelsPerMeter;
        public UInt32 yPixelsPerMeter;
        public UInt32 clrUsed;
        public UInt32 clrrImportant;
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

    public class AE_HapAVIFileTypeException : Exception
    {
        public AE_HapAVIFileTypeException()
        {

        }

        public AE_HapAVIFileTypeException(string message) : base(message)
        {

        }
    }

    public class AE_HapAVIParseException : Exception
    {
        public AE_HapAVIParseException()
        {

        }

        public AE_HapAVIParseException(string message) : base(message)
        {

        }
    }

    public class AE_HapAVICodecException : Exception
    {
        public AE_HapAVICodecException()
        {

        }

        public AE_HapAVICodecException(string message) : base(message)
        {

        }
    }

    public class AE_HapFrame
    {
        public AE_SurfaceCompressionType compressionType;
        public byte[] frameData;

        public AE_HapFrame(AE_SurfaceCompressionType compressionType, byte[] frameData)
        {
            this.compressionType = compressionType;
            this.frameData = frameData;
        }
    }
}
