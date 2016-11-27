using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

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

    class AE_HapAVI
    {
        private FileStream riffFileStream;
        private BinaryReader riffFileReader;

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


        }



  
    }
}
