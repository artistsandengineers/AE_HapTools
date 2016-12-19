using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AE_HapTools
{
    enum AE_HapSectionType : byte {
        RGB_DXT1_NONE = 0xAB,
        RGB_DXT1_SNAPPY = 0xBB,
        RGB_DXT1_CONSULT_DECODE_INSTRUCTIONS = 0xCB,
        RGBA_DXT5_NONE = 0xAE,
        RGBA_DXT5_SNAPPY = 0xBE,
        RGBA_DXT_5_CONSULT_DECODE_INSTRUCTIONS = 0xCE,
        YCoCg_DXT_5_NONE = 0xAF,
        YCoCg_DXT_5_SNAPPY = 0xBF,
        YCoCg_DXT_5_CONSULT_DECODE_INSTRUCTIONS = 0xCF,
        RGBA_BC7_NONE = 0xAC,
        RGBA_BC7_SNAPPY = 0xBC,
        RGBA_BC7_CONSULT_DECODE_INSTRUCTIONS = 0xCC,
        ALPHA_BC4_NONE = 0xA1,
        ALPHA_BC4_SNAPPY = 0xB1,
        ALPHA_BC4_CONSULT_DECODE_INSTRUCTIONS = 0xC1,
        MULTIPLE_IMAGES = 0x0D,
        //This arguably doesn't belong here, but it doesn't collide with the above so it is:
        DECODE_INSTRUCTION_CONTAINER = 0x01
    }

    enum AE_HapSecondStageCompressor
    {
        UNCOMPRESSED = 0x0A,
        SNAPPY = 0x0B,
        COMPLEX = 0x0C
    }

    enum AE_HapDecodeInstructionType
    {
        DECODE_INSTRUCTION_CHUNK_SECOND_STAGE_COMPRESSOR_TABLE = 0x02,
        DECODE_INSTRUCTION_CHUNK_SIZE_TABLE = 0x03,
        DECODE_INSTRUCTION_CHUNK_OFFSET_TABLE = 0x04
    }

    struct AE_HapSectionDescriptor
    {
        public UInt32 headerLength;
        public UInt32 sectionLength;
        public AE_HapSectionType sectionType;
    }

    struct AE_HapChunkDescriptor
    {
        public UInt32 size;
        public AE_HapSecondStageCompressor compressorType;
    }

    class AE_HapHelpers
    {

        private static uint read_three_byte_uint(byte[] buffer, uint offset)
        {
            return (uint)buffer[0 + offset] + ((uint)buffer[1 + offset] << 8) + ((uint)buffer[2 + offset] << 16);
        }

        private static uint read_four_byte_uint(byte[] buffer, uint offset)
        {
            return (uint)buffer[0 + offset] + ((uint)buffer[1 + offset] << 8) + ((uint)buffer[2 + offset] << 16) + ((uint)buffer[3 + offset] << 24);
        }

        public static AE_HapSectionDescriptor readSectionHeader(byte[] buffer, uint offset = 0)
        {
            AE_HapSectionDescriptor result;

            result.sectionLength = read_three_byte_uint(buffer, offset);

            if (result.sectionLength == 0)
            {
                result.sectionLength = read_four_byte_uint(buffer, offset + 4);
                result.headerLength = 8;
            } else
            {
                result.headerLength = 4;
            }

            result.sectionType = (AE_HapSectionType)buffer[offset + 3];

            return result;

        }

        public static AE_HapSecondStageCompressor SecondStageCompressorFromSectionType(AE_HapSectionType type)
        {
            if (((byte)type & 0xF0) == 0xA0)
            {
                return AE_HapSecondStageCompressor.UNCOMPRESSED;
            } else if (((byte)type & 0xF0) == 0xB0) {
                return AE_HapSecondStageCompressor.SNAPPY;
            } else if (((byte)type & 0xF0) == 0xC0)
            {
                return AE_HapSecondStageCompressor.COMPLEX;
            }

            throw new AE_HapAVICodecException("Unknown second stage compressor type.");
        }
    }
}
