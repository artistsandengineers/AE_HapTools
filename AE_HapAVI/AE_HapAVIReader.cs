﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using IronSnappy;

namespace AE_HapTools
{

    public class AE_HapAVIReader : IDisposable
    {
        private FileStream riffFileStream;

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

        public float frameRate
        {
            get
            {
                return (1.0f / (float)aviMainHeader.microSecondsPerFrame) * 1000000;
            }
        }

        private AE_AVIMainHeader aviMainHeader;

        private byte[] compressedFrameData;

        private AE_DDSHeaderStruct ddsHeader;

        public AE_HapAVIReader(string path)
        {
            riffFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

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

            riffFileStream.Seek(headerListEnd, SeekOrigin.Begin); //Skip the rest of the header list.

            //Nasty RIFF parsing here - basically we're trying to skip chunks until we encounter the one tagged 'movi'.
            //Now keep skipping chunks (dragons: by pretending they are lists) until we find our 'movi' chunk:
            AE_RIFFListHeader moviList;
            string l = "";
            while((moviList = AE_CopyPastedFromStackOverflow.ReadStruct<AE_RIFFListHeader>(riffFileStream)).typeFourCCString != "movi")
            {
                l += " " + moviList.typeFourCCString;
                if (riffFileStream.Seek(moviList.size - 4, SeekOrigin.Current) > riffFileStream.Length)
                    throw new AE_HapAVIParseException("Did not find movi list before EOF."); //-4 because the size property of a RIFF list header doesn't include the type FourCC...
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

            if (ddsHeader.pixelFormat.fourCC == 0) {
                ddsHeader.magic = AE_CopyPastedFromStackOverflow.string2FourCC("DDS ");
                ddsHeader.size = 124;
                ddsHeader.width = aviMainHeader.width;
                ddsHeader.height = aviMainHeader.height;
                ddsHeader.flags = (UInt32)(AE_DDSFlags.CAPS | AE_DDSFlags.HEIGHT | AE_DDSFlags.WIDTH | AE_DDSFlags.PIXELFORMAT | AE_DDSFlags.LINEARSIZE);
                ddsHeader.caps = (UInt32)AE_DDSCaps.TEXTURE;
                ddsHeader.pixelFormat.flags = (UInt32)AE_DDSPixelFormats.FOURCC;
                ddsHeader.pixelFormat.fourCC = AE_CopyPastedFromStackOverflow.string2FourCC(SurfaceCompressionTypeFromHapSectionType(hapInfo.sectionType).ToString());
            }

            byte[] uncompressedSnappyData;
            uncompressedSnappyData = new byte[hapInfo.sectionLength + ddsHeader.size + 4];

            if (AE_HapHelpers.SecondStageCompressorFromSectionType(hapInfo.sectionType) == AE_HapSecondStageCompressor.UNCOMPRESSED)
            {
                Array.Copy(compressedFrameData, (int)hapInfo.headerLength, uncompressedSnappyData, ddsHeader.size + 4, hapInfo.sectionLength);
            } else if (AE_HapHelpers.SecondStageCompressorFromSectionType(hapInfo.sectionType) == AE_HapSecondStageCompressor.SNAPPY)
            {
                byte[] snappyData = new byte[hapInfo.sectionLength];    

                Array.Copy(compressedFrameData, hapInfo.headerLength, snappyData, 0, hapInfo.sectionLength);
                uncompressedSnappyData = Snappy.Decode(snappyData);
            }
            else if (AE_HapHelpers.SecondStageCompressorFromSectionType(hapInfo.sectionType) == AE_HapSecondStageCompressor.COMPLEX)
            {
                var hapDecodeInstructions = AE_HapHelpers.readSectionHeader(compressedFrameData, hapInfo.headerLength);

                if (hapDecodeInstructions.sectionType != AE_HapSectionType.DECODE_INSTRUCTION_CONTAINER)
                {
                    throw new AE_HapAVICodecException("Did not find expected decode instructions container.");
                }

                //Build a list of chunks that make up this frame:
                AE_HapChunkDescriptor[] chunkList = null;
                uint bytesRead = 0;

                //In theory the decode instruction compressor table and size tables may appear in any order.
                while (bytesRead < hapDecodeInstructions.sectionLength)
                {
                    var s = AE_HapHelpers.readSectionHeader(compressedFrameData, hapInfo.headerLength + hapDecodeInstructions.headerLength + bytesRead);

                    if ((AE_HapDecodeInstructionType)s.sectionType == AE_HapDecodeInstructionType.DECODE_INSTRUCTION_CHUNK_SECOND_STAGE_COMPRESSOR_TABLE)
                    {
                        if (chunkList == null) chunkList = new AE_HapChunkDescriptor[s.sectionLength];
                        for (int i = 0; i < s.sectionLength; i++)
                        {
                            chunkList[i].compressorType = (AE_HapSecondStageCompressor)compressedFrameData[hapInfo.headerLength + hapDecodeInstructions.headerLength + s.headerLength + bytesRead + i];
                        }
                    }
                    else if ((AE_HapDecodeInstructionType)s.sectionType == AE_HapDecodeInstructionType.DECODE_INSTRUCTION_CHUNK_SIZE_TABLE)
                    {
                        if (chunkList == null) chunkList = new AE_HapChunkDescriptor[s.sectionLength / 4];

                        for (int i = 0; i < s.sectionLength / 4; i++)
                        {
                            chunkList[i].size = (uint)BitConverter.ToInt32(compressedFrameData, (int)(hapInfo.headerLength + hapDecodeInstructions.headerLength + s.headerLength + bytesRead + (i * 4)));
                        }
                    }

                    bytesRead += s.headerLength + s.sectionLength;
                }

                //Now decompress the chunks into a single frame:
                //TODO: This could be parallelised
                int compressedFrameDataOffset = (int)(hapInfo.headerLength + hapDecodeInstructions.headerLength + hapDecodeInstructions.sectionLength);
                int decomressedFrameDataOffset = (int)ddsHeader.size + 4;

                if (chunkList.Count() > 1) {
                    throw new NotImplementedException("Multi-chunk decoding not implemented yet.");
                }

                for (int i = 0; i < chunkList.Count(); i++)
                {
                    var snappyData = new byte[chunkList[i].size];
                    Array.Copy(compressedFrameData, compressedFrameDataOffset, snappyData, 0, chunkList[i].size);
                    compressedFrameDataOffset += (int)chunkList[i].size;
                    uncompressedSnappyData = Snappy.Decode(snappyData);
                    //Console.WriteLine(uncompressedSnappyData.Length);
                }
            }


            var ddsHeaderBytes =  AE_CopyPastedFromStackOverflow.structToByteArray(ddsHeader);
            
            var result = ddsHeaderBytes.Concat(uncompressedSnappyData).ToArray();
            Console.WriteLine(ddsHeaderBytes.Length);
            return new AE_HapFrame(SurfaceCompressionTypeFromHapSectionType(hapInfo.sectionType), result);
            

            throw new AE_HapAVICodecException("Could not decode frame at index" + frameIndex);
        }

        private static AE_SurfaceCompressionType SurfaceCompressionTypeFromHapSectionType(AE_HapSectionType sectionType)
        {

            switch ((byte)sectionType & 0x0f)
            {
                case 0x0b: //DXT1/BC1, RGB
                    return AE_SurfaceCompressionType.DXT1;
                case 0x0e: //DXT5/BC3, RGBA
                    return AE_SurfaceCompressionType.DXT5;
                case 0x0f: //DXT5/BC3, Scaled YCoCg
                    throw new NotImplementedException();
                case 0x0c: //BC7, RGBA
                    throw new NotImplementedException();
                default:
                    throw new AE_HapAVICodecException("Unsupported format: " + sectionType.ToString());
            }
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
