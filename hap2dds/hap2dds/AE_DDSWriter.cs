using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace AE_Hap2DDS
{

    public class AE_DDSDXGIExtension
    {
        public uint dxgiFormat = 56; //DXGI_FORMAT_R16_UNORM
        public uint resourceDimension = 3; //2D texture (1 = 1D, 4 = 2D)
        public uint miscFlags;
        public uint arraySize = 1; //I /think/ 1 = single texture, but check.
        public uint miscFlags2;

        public void write(BinaryWriter w)
        {
            w.Write(dxgiFormat);
            w.Write(resourceDimension);
            w.Write(miscFlags);
            w.Write(arraySize);
            w.Write(miscFlags2);
        }
    }

    public class AE_DDSPixelFormat
    {
        public uint size = 32; //Structure size, bytes, always 32
        public uint flags = 0x40; //Texture contains uncompressed RGB data.
        public uint fourCC = 0x00;
        public uint RGBBitCount = 32; //Bits per pixel
        //Bitmasks for reading colour data - e.g. ARGB8 images go thus:
        public uint RBitMask = 0x00ff0000;
        public uint GBitMask = 0x0000ff00;
        public uint BBitMask = 0x000000ff;
        public uint ABitMask = 0xff000000;

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
        public uint size = 124; //Structure size, bytes, always 124;
        public uint flags = 0x06; //File contains both width and height data. This is wrong per the spec but vvvv doesn't mind.
        public uint height;
        public uint width;
        public uint pitchOrLinearSize;
        public uint Depth;
        public uint mipMapCount;
        public uint[] reserved1 = new uint[11];
        public AE_DDSPixelFormat pixelFormat;
        public uint caps = 0x1000; //I think this implies that we have a single texture up in here, but a more thorough reading of the spec. is recommended.
        public uint caps2;
        public uint caps3;
        public uint caps4;
        public uint reserved2;

        public AE_DDSHeader()
        {
            pixelFormat = new AE_DDSPixelFormat();
        }

        public void write(BinaryWriter w)
        {
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
        public uint magic = 0x20534444; //'DDS '
        public AE_DDSHeader header;
        public AE_DDSDXGIExtension dx10Extension;
        public byte[] data;

        public AE_DDS(uint width, uint height, uint bitsPerPixel, bool withDX10Extension = false)
        {
            header = new AE_DDSHeader();
            header.width = width;
            header.height = height;
            header.pixelFormat.RGBBitCount = bitsPerPixel;
            header.pitchOrLinearSize = width * header.pixelFormat.RGBBitCount;

            if (withDX10Extension)
            {
                dx10Extension = new AE_DDSDXGIExtension();
                header.pixelFormat.fourCC = 0x30315844; //"DX10"

            }

        }
    }

    public class AE_DDSWriter
    {
        public void writeDDS(AE_DDS dds, string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            BinaryWriter w = new BinaryWriter(new FileStream(path, FileMode.Create));
            w.Write(dds.magic);
            dds.header.write(w);
            if (dds.dx10Extension != null)
            {
                dds.dx10Extension.write(w);
            }
            w.Write(dds.data);
        }
    }
}