using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace AE_HapTools
{
    public class AE_HapAVIWriter : IDisposable
    {
        private FileStream riffFileStream;
        private AE_RIFFListHeader riffHeader;
        private AE_AVIMainHeader aviMainHeader;

        AE_HapAVIWriter(string path, float frameRate, UInt32 width, UInt32 height)
        {
            riffFileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

            //RIFF Header
            riffHeader = new AE_RIFFListHeader();
            riffHeader.typeFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("RIFF");
            riffHeader.listFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("LIST");
            riffHeader.size = (uint)Marshal.SizeOf(typeof(AE_RIFFListHeader));
            
            //header LIST
            var headerList = new AE_RIFFListHeader();
            headerList.listFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("LIST");
            headerList.typeFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("hdrl");
            headerList.size = (uint)Marshal.SizeOf(typeof(AE_AVIMainHeader)) + (uint)Marshal.SizeOf(typeof(AE_RIFFListHeader)) - 8; //i.e. the size of the avi header + the size of the RIFFListHeader not including its 4cc or size fields :(

            //AVI main header
            aviMainHeader.fourcc = AE_CopyPastedFromStackOverflow.string2FourCC("AVI ");
            aviMainHeader.microSecondsPerFrame = (uint)((1.0f / frameRate) / 1000000);
            aviMainHeader.width = width;
            aviMainHeader.height = height;

            //stream LIST
            var streamList = new AE_RIFFListHeader();
            streamList.listFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("LIST");
            streamList.typeFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("strl");

            //AVI Stream header
            var streamHeader = new AE_AVIStreamHeader();
            streamHeader.fourcc = AE_CopyPastedFromStackOverflow.string2FourCC("vids");
            streamHeader.rate = 30; //dividing rate by scale = fps
            streamHeader.scale = 1;

            //AVI stream format header (bitmap format)
            var streamFormatHeader = new AE_AVIBitmapInfoHeader();
            streamFormatHeader.size = (uint)Marshal.SizeOf(typeof(AE_AVIBitmapInfoHeader));
            streamFormatHeader.width = width;
            streamFormatHeader.height = height;
            streamFormatHeader.planes = 1;
            streamFormatHeader.compression = AE_CopyPastedFromStackOverflow.string2FourCC("Hap1");

            streamList.size = (uint)Marshal.SizeOf(typeof(AE_AVIBitmapInfoHeader)) + (uint)Marshal.SizeOf(typeof(AE_AVIStreamHeader)) + (uint)Marshal.SizeOf(typeof(AE_RIFFListHeader)) - 8;

            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, riffHeader);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, headerList);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, aviMainHeader);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, streamList);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, streamHeader);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, streamFormatHeader);

            //AVI movi LIST
            var moviList = new AE_RIFFListHeader();
            moviList.listFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("LIST");
            moviList.typeFourCC = AE_CopyPastedFromStackOverflow.string2FourCC("movi");

        }

        public void writeFrameFromDDS(string ddsPath)
        {
            var ddsFileStream = new FileStream(ddsPath, FileMode.Open, FileAccess.Read);
            var ddsHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_DDSHeaderStruct>(ddsFileStream);
            var size = ddsHeader.pitchOrLinearSize * ddsHeader.height;

            byte[] sectionHeader = null;
            if (size > 0x00FFFFFF)
            {
                sectionHeader = new byte[8];
                var sectionSize = AE_HapHelpers.write_four_byte_int(size);
                sectionHeader[4] = sectionSize[0];
                sectionHeader[5] = sectionSize[1];
                sectionHeader[6] = sectionSize[2];
                sectionHeader[7] = sectionSize[3];
            } else
            {
                sectionHeader = new byte[4];
                var sectionSize = AE_HapHelpers.write_three_byte_uint(size);
                sectionHeader[0] = sectionSize[0];
                sectionHeader[1] = sectionSize[1];
                sectionHeader[2] = sectionSize[2];
            }

            sectionHeader[3] = 0xbb; //DXT1 snappy compressed. TODO: Add support for other formats.

            byte[] ddsData = new byte[size];
            ddsFileStream.Read(ddsData, 0, (int)size);
            ddsFileStream.Close();

/*
            var snappyCompressedDDS = Snappy.SnappyCodec.Compress(ddsData);

            var recList = new AE_RIFFChunkHeader();
            recList.fourcc = AE_CopyPastedFromStackOverflow.string2FourCC("rec ");
            recList.size = (uint)snappyCompressedDDS.Length;

            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, riffHeader);
            riffFileStream.Write(snappyCompressedDDS, 0, snappyCompressedDDS.Length);
            */
        }
        
        public void closeFile()
        {
            //re-write the RIFF header now that we know how big the file is
            riffHeader.size = (UInt32)riffFileStream.Position - 8;
            riffFileStream.Seek(0, SeekOrigin.Begin);
            AE_CopyPastedFromStackOverflow.WriteStruct(riffFileStream, riffHeader);

            riffFileStream.Flush();
            riffFileStream.Close();
        }

        public void Dispose()
        {
            if (riffFileStream != null)
            {
                riffFileStream.Close();
                riffFileStream = null;
            }

        }

    }
}
