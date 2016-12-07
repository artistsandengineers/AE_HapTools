﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AE_HapTools
{
    enum AE_HapSectionType{
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

    enum AE_HapDecodeInstructionType
    {
        DECODE_INSTURCITON_CHUNK_SECOND_STAGE_TABLE = 0x02,
        DECODE_INSTRUCTION_CHUNK_SIZE_TABLE = 0x03,
        DECODE_INSTRUCTION_CHUNK_OFFSET_TABLE = 0x04
    }

    struct AE_HapSectionDescriptor
    {
        public UInt32 headerLength;
        public UInt32 sectionLength;
        public AE_HapSectionType sectionType;
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
    }
}
