using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using Snappy;

namespace hapavi_cli
{
    
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_RIFFChunkHeader
    {
        public UInt32 fourcc;
        public UInt32 size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AE_RIFFListHeader
    {
        public UInt32 listFourCC;
        public UInt32 size;
        public UInt32 typeFourCC;
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

    struct AE_HapAVIframeIndexItem{
        public long position;
        public long length;
    }

    class AE_HapAVIFileTypeException : Exception
    {
        public AE_HapAVIFileTypeException()
        {

        }

        public AE_HapAVIFileTypeException(string message) : base(message)
        {

        }
    }

    class AE_HapAVIParseException : Exception
    {
        public AE_HapAVIParseException()
        {

        }

        public AE_HapAVIParseException(string message) : base(message)
        {

        }
    }

    class AE_HapAVICodecException : Exception
    {
        public AE_HapAVICodecException()
        {

        }

        public AE_HapAVICodecException(string message) : base(message)
        {

        }
    }

    class AE_HapAVI
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

        private AE_RIFFListHeader readRIFFHeaderList()
        {
            var listHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

            if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(listHeader.listFourCC) != "LIST")
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

                if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(c.fourcc) == "00dc") //The first compressed video stream in an AVI has chunk type 00dc.
                {
                    AE_HapAVIframeIndexItem frameIndexItem;
                    frameIndexItem.length = c.size;
                    frameIndexItem.position = riffFileStream.Position;
                    frameIndex.Add(frameIndexItem);
                }

                riffFileStream.Seek(c.size + AE_CopyPastedFromStackOverflow.calculatePad(c.size, 2), SeekOrigin.Current);
            }
        }

        public AE_HapAVI(string path)
        {
            riffFileStream = new FileStream(path, FileMode.Open);
            riffFileReader = new BinaryReader(riffFileStream);

            var riffHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

            if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(riffHeader.listFourCC) != "RIFF") //minor abuse here, since if this was a true list listFourCC would be "LIST"
            {
                throw new AE_HapAVIFileTypeException("Input file does not start with RIFF chunk.");
            }

            if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(riffHeader.typeFourCC) != "AVI ")
            {
                throw new AE_HapAVIFileTypeException("RIFF is not an AVI.");
            }

            var headerList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

            if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(headerList.typeFourCC) != "hdrl")
            {
                throw new AE_HapAVIParseException("Could not read header list (hdrl).");
            }

            aviMainHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_AVIMainHeader>(riffFileStream);

            /*So, we want to build an index of where all of the frames are in our video file. Impediments to 
            doing this "correctly" are many and life is short, so let's do it by brute force. This involves
            locating 'movi' lists, which contain frame data. There might be more than one because large AVI
            files are in fact multiple concatenated, RIFFs, sort of... Also let's assume that our AVI only
            contains a single video stream.*/

            //Find the first 'movi' fourcc:
            UInt32 fcc = 0;
            while (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(fcc) != "movi")
            {
                fcc = AE_CopyPastedFromStackOverflow.ReadStruct<UInt32>(riffFileStream);
            }

            //Now seek backwards 12 bytes and read that in again as a list, because we need to know the list size:
            riffFileStream.Seek(-12, SeekOrigin.Current);
            var moviList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

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

                if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(riffHeader.typeFourCC) != "AVIX")
                {
                    throw new AE_HapAVIParseException("Did not find list of type AVIX where we were expecting one.");
                }

                moviList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream);

                if (AE_CopyPastedFromStackOverflow.FourCCFromUInt32(fcc) != "movi")
                {
                    throw new AE_HapAVIParseException("Did not find expected movi list.");
                }

                appendFramesToIndex(moviList.size);
            }
        }

        public byte[] getHapFrameAtIndex(int index)
        {
            if (index > frameCount - 1)
            {
                throw new IndexOutOfRangeException("Requested frame does not exist.");
            }

            byte[] compressed = new byte[frameIndex[index].length];

            riffFileStream.Seek(frameIndex[index].position, SeekOrigin.Begin);
            riffFileStream.Read(compressed, 0, (int)frameIndex[index].length);

            var hapInfo = AE_HapHelpers.readSectionHeader(compressed);

            if (hapInfo.sectionType != AE_HapSectionType.RGB_DXT1_SNAPPY)
            {
                throw new AE_HapAVICodecException("Hap section type unsupported: " + hapInfo.sectionType.ToString());
            }

            byte[] uncompressed = new byte[Snappy.SnappyCodec.GetUncompressedLength(compressed, (int)hapInfo.headerLength, (int)hapInfo.sectionLength)];

            Snappy.SnappyCodec.Uncompress(compressed, (int)hapInfo.headerLength, (int)hapInfo.sectionLength, uncompressed, 0);

            return uncompressed;

        }
    }
}
