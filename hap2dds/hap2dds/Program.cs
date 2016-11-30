using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using AE_HapTools;

namespace AE_Hap2DDS
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: hap2dds <input_hap_avi> <output_directory>");
                Environment.Exit(1);
            }

            var avi = new AE_HapAVI(args[0]);
            var outputPath = args[1];

            Console.WriteLine("AVI has " + avi.frameCount + " frames.");
            Console.WriteLine("Width: " + avi.imageWidth + " Height: " + avi.imageHeight);

            var timer = Stopwatch.StartNew();

            //Build a DDS header:
            var dds = new AE_DDS(avi.imageWidth, avi.imageHeight);
            dds.header.flags = (UInt32)(AE_DDSFlags.CAPS | AE_DDSFlags.HEIGHT | AE_DDSFlags.WIDTH | AE_DDSFlags.PIXELFORMAT | AE_DDSFlags.LINEARSIZE);
            dds.header.pixelFormat.flags = (UInt32)AE_DDSPixelFormats.FOURCC;
            dds.header.caps = (UInt32)AE_DDSCaps.TEXTURE;

            for (int i = 0; i < avi.frameCount; i++)
            {
                var path = Path.Combine(outputPath, Path.GetFileName(args[0]) + "." + i.ToString("0000000") + ".dds");
                var f = avi.getHapFrameAtIndex(i);
                dds.header.pixelFormat.fourCC = AE_CopyPastedFromStackOverflow.string2FourCC(f.compressionType.ToString());

                try
                {
                    var s = new FileStream(path, FileMode.CreateNew);
                    var writer = new BinaryWriter(s);

                    dds.header.write(writer);

                    writer.Write(f.frameData);

                    writer.Close();
                    s.Close();
                } catch (Exception e)
                {
                    Console.WriteLine("Couldn't write " + path + ": " + e.Message);
                    Console.ReadLine();
                    Environment.Exit(2);
                }
                Console.Write("\r" + i.ToString() + " of " + avi.frameCount.ToString());
            }

            timer.Stop();

            Console.WriteLine("");
            Console.WriteLine("Wrote " + avi.frameCount + ".dds files in " + timer.ElapsedMilliseconds / 1000 + " seconds.");

            Console.ReadLine();
        }
    }
}
