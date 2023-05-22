using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.IO;

namespace AE_HapTools
{
    [Flags]
    public enum AE_DDSCaps : UInt32
    {
        TEXTURE = 0x00001000
    }

    [Flags]
    public enum AE_DDSFlags : UInt32
    {
        CAPS = 0x00000001,
        HEIGHT = 0x00000002,
        WIDTH = 0x00000004,
        PITCH = 0x00000008,
        PIXELFORMAT = 0x00001000,
        MIPMAPCOUNT = 0x00020000,
        LINEARSIZE =  0x00080000,
        DEPTH = 0x00800000
    }

    [Flags]
    public enum AE_DDSPixelFormats : UInt32
    {
        ALPHAPIXELS = 0x00000001,
        ALPHA = 0x00000002,
        FOURCC = 0x00000004,
        RGB = 0x00000040,
        YUV = 0x00000200,
        LUMA = 0x00020000
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]

    public struct AE_DDSPixelFormatStruct
    {
        public UInt32 size;
        public UInt32 flags;
        public UInt32 fourCC;
        public UInt32 RGBBitCount;
        public UInt32 RBitMask;
        public UInt32 GBitMask;
        public UInt32 BBitMask;
        public UInt32 ABitMask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct AE_DDSHeaderStruct
    {
        public UInt32 magic;
        public UInt32 size;
        public UInt32 flags;
        public UInt32 height;
        public UInt32 width;
        public UInt32 pitchOrLinearSize;
        public UInt32 Depth;
        public UInt32 mipMapCount;
        public UInt32 reserved1;
        public UInt32 reserved2;
        public UInt32 reserved3;
        public UInt32 reserved4;
        public UInt32 reserved5;
        public UInt32 reserved6;
        public UInt32 reserved7;
        public UInt32 reserved8;
        public UInt32 reserved9;
        public UInt32 reserved10;
        public UInt32 reserved11;
        public AE_DDSPixelFormatStruct pixelFormat;
        public UInt32 caps;
        public UInt32 caps2;
        public UInt32 caps3;
        public UInt32 caps4;
        public UInt32 reserved12;
    }

    public class AE_DDSPixelFormat
    {
        public UInt32 size = 32; //Structure size, bytes, always 32
        public UInt32 flags;
        public UInt32 fourCC;
        public UInt32 RGBBitCount;
        //Bitmasks for reading colour data - e.g. ARGB8 images go thus:
        public UInt32 RBitMask = 0x00ff0000;
        public UInt32 GBitMask = 0x0000ff00;
        public UInt32 BBitMask = 0x000000ff;
        public UInt32 ABitMask = 0xff000000;

        public void write(BinaryWriter w)
        {
            w.Write(size);
            w.Write(flags);
            w.Write(fourCC);
            w.Write(RGBBitCount);
            w.Write(RBitMask);
            w.Write(GBitMask);
            w.Write(BBitMask);
            w.Write(ABitMask);
        }
    }

    public class AE_DDSHeader
    {
        public UInt32 magic;
        public UInt32 size = 124; //Structure size, bytes, always 124;
        public UInt32 flags;
        public UInt32 height;
        public UInt32 width;
        public UInt32 pitchOrLinearSize;
        public UInt32 Depth;
        public UInt32 mipMapCount;
        public UInt32[] reserved1 = new UInt32[11];
        public AE_DDSPixelFormat pixelFormat;
        public UInt32 caps;
        public UInt32 caps2;
        public UInt32 caps3;
        public UInt32 caps4;
        public UInt32 reserved2;

        /// <summary>
        /// Hackhacksorryhack
        /// Returns the actual size of the header, in bytes, because the size field doesn't include the magic.
        /// </summary>
        public int actualSize
        {
            get
            {
                return (int)size + System.Runtime.InteropServices.Marshal.SizeOf(magic);
            }
        }

        public AE_DDSHeader()
        {
            pixelFormat = new AE_DDSPixelFormat();
        }

        public void write(BinaryWriter w)
        {
            w.Write(magic);
            w.Write(size);
            w.Write(flags);
            w.Write(height);
            w.Write(width);
            w.Write(pitchOrLinearSize);
            w.Write(Depth);
            w.Write(mipMapCount);
            for (int i = 0; i < reserved1.Count(); i++)
            {
                w.Write(reserved1[i]);
            }
            pixelFormat.write(w);
            w.Write(caps);
            w.Write(caps2);
            w.Write(caps3);
            w.Write(caps4);
            w.Write(reserved2);
        }
    }

    public class AE_DDS
    {
        public AE_DDSHeader header;

        public AE_DDS(UInt32 width, UInt32 height)
        {
            header = new AE_DDSHeader();
            header.magic = AE_HapTools.AE_CopyPastedFromStackOverflow.string2FourCC("DDS ");
       
            header.width = width;
            header.height = height;
        }
    }


}