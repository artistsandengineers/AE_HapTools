using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using AE_HapTools;

namespace dds2hap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: dds2hap <input_dds_directory> <output_avi> <fps>");
                Environment.Exit(1);
            }

            var inputDirectory = args[0];
            var outputAVIPath = args[1];
            var fps = (float)Convert.ToDouble(args[2]);

            var ddsFiles = Directory.GetFiles(inputDirectory, "*.dds").OrderBy(f => f);

            Console.WriteLine("Found " + ddsFiles.Count() + " dds files");

            //Read the first DDS so that we know how big things are:
            var firstFrameFileStream = new FileStream(ddsFiles.First(), FileMode.Open, FileAccess.Read);
            var firstFrameHeader = AE_CopyPastedFromStackOverflow.ReadStruct<AE_DDSHeaderStruct>(firstFrameFileStream);
            firstFrameFileStream.Close();

            Console.WriteLine("Using resolution " + firstFrameHeader.width + "x" + firstFrameHeader.height + " and pixel format: " + AE_CopyPastedFromStackOverflow.fourCC2String(firstFrameHeader.pixelFormat.fourCC) + " (based on first frame in sequence)");

            AE_HapAVIWriter writer = new AE_HapAVIWriter(outputAVIPath, firstFrameHeader.width, firstFrameHeader.height, fps);


            for (int i = 0; i < ddsFiles.Count(); i++)
            {
                writer.writeFrameFromDDS(ddsFiles.ElementAt(i));
                Console.WriteLine("Wrote frame " + i + 1);
            }

            Console.ReadLine();

        }
    }
}