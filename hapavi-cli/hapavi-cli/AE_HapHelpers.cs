using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hapavi_cli
{
    enum AE_HapSectionType{
        RGB_DXT1_NONE = 0xAB,
        RGB_DXT1_SNAPPY = 0xBB,
        RGB_DXT1_COMPLICATED = 0xCB
    }

    struct AE_HapSectionDescriptor
    {
        public UInt32 headerLength;
        public UInt32 sectionLength;
        public AE_HapSectionType sectionType;
    }

    class AE_HapHelpers
    {

        private static uint read_three_byte_uint(byte[] buffer)
        {
            return (uint)buffer[0] + ((uint)buffer[1] << 8) + ((uint)buffer[2] << 16);
        }

        private static uint read_four_byte_uint(byte[] buffer, int offset)
        {
            return (uint)buffer[0 + offset] + ((uint)buffer[1 + offset] << 8) + ((uint)buffer[2 + offset] << 16) + ((uint)buffer[3 + offset] << 24);
        }

        public static AE_HapSectionDescriptor readSectionHeader(byte[] buffer)
        {
            AE_HapSectionDescriptor result;

            result.sectionLength = read_three_byte_uint(buffer);

            if (result.sectionLength == 0)
            {
                result.sectionLength = read_four_byte_uint(buffer, 4);
                result.headerLength = 8;
            } else
            {
                result.headerLength = 4;
            }

            result.sectionType = (AE_HapSectionType)buffer[3];

            return result;

        }
    }
}
