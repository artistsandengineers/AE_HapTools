using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using Snappy;

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
    //* The values here match the values in the SlimDX format enum, which makes things a little bit easier later.
    //* The names here match the 4CCs required in a DDS header.
    public enum AE_SurfaceCompressionType
    {
        DXT1 = 70
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

    public class AE_HapAVI
    {
        private FileStream riffFileStream;
        private BinaryReader riffFileReader;

        private List<AE_HapAVIframeIndexItem> frameIndex; //We'll store offsets (relative to start of file) of our frames in here, so that we can seek them later.

        public uint imageWidth {
            get
            {
                return aviMainHeader.width;
            }
        }

        public uint imageHeight
        {
            get
            {
                return aviMainHeader.height;
            }
        }

        public int frameCount
        {
            get
            {
                if (frameIndex != null)
                {
                    return frameIndex.Count;
                } else
                {
                    return 0;
                }
            }
        }

        private AE_AVIMainHeader aviMainHeader;

        private byte[] compressedFrameData;

        private byte[] uncompressedFrameDataWithHeader;

        private AE_DDS ddsHeader;

        public AE_HapAVI(string path)
        {
            riffFileStream = new FileStream(path, FileMode.Open);
            riffFileReader = new BinaryReader(riffFileStream);

            var riffHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

            if (riffHeader.listFourCCString != "RIFF") //minor abuse here, since if this was a true list listFourCC would be "LIST"
            {
                throw new AE_HapAVIFileTypeException("Input file does not start with RIFF chunk.");
            }

            if (riffHeader.typeFourCCString != "AVI ")
            {
                throw new AE_HapAVIFileTypeException("RIFF is not an AVI.");
            }

            var headerList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);
            var headerListEnd = riffFileStream.Position + headerList.size - 4;

            if (headerList.typeFourCCString != "hdrl")
            {
                throw new AE_HapAVIParseException("Could not read header list (hdrl).");
            }

            aviMainHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_AVIMainHeader>(riffFileStream);

            riffFileStream.Seek(headerListEnd + AE_CopyPastedFromStackOverflow.calculatePad(headerListEnd, 4), SeekOrigin.Begin); //Skip the rest of the header list.

            //Nasty RIFF parsing here - basically we're trying to skip chunks until we encounter the one tagged 'movi'.
            //Now keep skipping chunks (dragons: by pretending they are lists) until we find our 'movi' chunk:
            AE_RIFFListHeader moviList;

            while((moviList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream)).typeFourCCString != "movi")
            {
                riffFileStream.Seek(moviList.size - 4, SeekOrigin.Current); //-4 because the size property of a RIFF list header doesn't include the type FourCC...
            }


            //...start building a frame list:
            frameIndex = new List<AE_HapAVIframeIndexItem>();

            appendFramesToIndex(moviList.size);

            //Now let's go looking for frames in subsequent RIFF chunks, if they exist:
            riffFileStream.Seek(riffHeader.size + Marshal.SizeOf(typeof(AE_RIFFChunkHeader)) + AE_CopyPastedFromStackOverflow.calculatePad(riffHeader.size, 2), SeekOrigin.Begin); //Seek to the end of the first RIFF chunk.

            if (riffFileStream.Position == riffFileStream.Length) //If the end of the first RIFF chunk aligns with the end of the file then we're done.
            {
                return;
            }

            //If we got this far then we're dealing with a 'long' RIFF - i.e. something with at least one AVIX list in it...
            while (riffFileStream.Position < riffFileStream.Length)
            {
                riffHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

                if (riffHeader.typeFourCCString != "AVIX")
                {
                    throw new AE_HapAVIParseException("Did not find list of type AVIX where we were expecting one.");
                }

                moviList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

                if (moviList.typeFourCCString != "movi")
                {
                    throw new AE_HapAVIParseException("Did not find expected movi list.");
                }

                appendFramesToIndex(moviList.size);
            }
        }

        private AE_RIFFListHeader readRIFFHeaderList()
        {
            var listHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

            if (listHeader.listFourCCString != "LIST")
            {
                throw new AE_HapAVIParseException("Did not find LIST where we expected one.");
            }

            return listHeader;
        }

        //Skims through the filestream looking for chunks in the movi list tagged as compressed video.
        private void appendFramesToIndex(uint moviListSize)
        {
            long startOffset = riffFileStream.Position;

            AE_RIFFChunkHeader c;

            while (riffFileStream.Position < startOffset + moviListSize - 4) //-4 because the size property of a LIST includes the list subtype...
            {
                c = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFChunkHeader>(riffFileStream);

                if (c.fourCCString == "00dc") //The first compressed video stream in an AVI has chunk type 00dc.
                {
                    AE_HapAVIframeIndexItem frameIndexItem;
                    frameIndexItem.length = c.size;
                    frameIndexItem.position = riffFileStream.Position;
                    frameIndex.Add(frameIndexItem);
                }

                riffFileStream.Seek(c.size + AE_CopyPastedFromStackOverflow.calculatePad(c.size, 2), SeekOrigin.Current);
            }
        }

        public AE_HapFrame getHapFrameAtIndex(int index)
        {
            if (index > frameCount - 1)
            {
                throw new IndexOutOfRangeException("Requested frame does not exist.");
            }

            if (compressedFrameData == null || compressedFrameData.Length < frameIndex[index].length)
            {
                compressedFrameData = new byte[frameIndex[index].length];
            }

            riffFileStream.Seek(frameIndex[index].position, SeekOrigin.Begin);
            riffFileStream.Read(compressedFrameData, 0, (int)frameIndex[index].length);

            var hapInfo = AE_HapHelpers.readSectionHeader(compressedFrameData);

            byte[] uncompressedFrameData = new byte[Snappy.SnappyCodec.GetUncompressedLength(compressedFrameData, (int)hapInfo.headerLength, (int)hapInfo.sectionLength)];

            Snappy.SnappyCodec.Uncompress(compressedFrameData, (int)hapInfo.headerLength, (int)hapInfo.sectionLength, uncompressedFrameData, 0);

            return new AE_HapFrame(SurfaceCompressionTypeFromHapSectionType(hapInfo.sectionType), uncompressedFrameData);

        }

        public AE_HapFrame getHapFrameAndDDSHeaderAtIndex(int index)
        {
            if (index > frameCount - 1)
            {
                throw new IndexOutOfRangeException("Requested frame does not exist.");
            }

            if (compressedFrameData == null || compressedFrameData.Length < frameIndex[index].length)
            {
                compressedFrameData = new byte[frameIndex[index].length];
            }

            riffFileStream.Seek(frameIndex[index].position, SeekOrigin.Begin);
            riffFileStream.Read(compressedFrameData, 0, (int)frameIndex[index].length);

            var hapInfo = AE_HapHelpers.readSectionHeader(compressedFrameData);

            if (hapInfo.sectionType != AE_HapSectionType.RGB_DXT1_SNAPPY)
            {
                throw new AE_HapAVICodecException("Hap section type unsupported: " + hapInfo.sectionType.ToString());
            }

            if (ddsHeader == null || AE_CopyPastedFromStackOverflow.fourCC2String(ddsHeader.header.pixelFormat.fourCC) != hapInfo.sectionType.ToString())
            {
                ddsHeader = new AE_DDS(aviMainHeader.width, aviMainHeader.height);
                ddsHeader.header.flags = (UInt32)(AE_DDSFlags.CAPS | AE_DDSFlags.HEIGHT | AE_DDSFlags.WIDTH | AE_DDSFlags.PIXELFORMAT | AE_DDSFlags.LINEARSIZE);
                ddsHeader.header.pixelFormat.flags = (UInt32)AE_DDSPixelFormats.FOURCC;
                ddsHeader.header.caps = (UInt32)AE_DDSCaps.TEXTURE;
                ddsHeader.header.pixelFormat.fourCC = AE_CopyPastedFromStackOverflow.string2FourCC(SurfaceCompressionTypeFromHapSectionType(hapInfo.sectionType).ToString());

                uncompressedFrameDataWithHeader = new byte[256 + Snappy.SnappyCodec.GetUncompressedLength(compressedFrameData, (int)hapInfo.headerLength, (int)hapInfo.sectionLength)]; //DDS header is 124 bytes + the 'DDS ' fourcc at the front of the file.

                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        ddsHeader.header.write(writer);
                    }
                    stream.Flush();
                    byte[] bytes = stream.GetBuffer();
                    bytes.CopyTo(uncompressedFrameDataWithHeader, 0);
                }

            }

            Snappy.SnappyCodec.Uncompress(compressedFrameData, (int)hapInfo.headerLength, (int)hapInfo.sectionLength, uncompressedFrameDataWithHeader, 256);

            return new AE_HapFrame(AE_SurfaceCompressionType.DXT1, uncompressedFrameDataWithHeader);
        }

        private static AE_SurfaceCompressionType SurfaceCompressionTypeFromHapSectionType(AE_HapSectionType sectionType)
        {
            switch (sectionType)
            {
                case AE_HapSectionType.RGB_DXT1_SNAPPY:
                    return AE_SurfaceCompressionType.DXT1;
                default:
                    throw new AE_HapAVICodecException("Unsupported format: " + sectionType.ToString());
            }
        }
    }
}
