using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using AE_HapTools;

namespace hap2dds
{
    class Program
    {
        static void Main(string[] args)
        {
            AE_HapAVI avi = new AE_HapAVI("C:/Users/arron/Downloads/long.avi");

            Console.WriteLine("Width: " + avi.imageWidth + " Height: " + avi.imageHeight);

            var s = Stopwatch.StartNew();

            for (int i = 0; i < avi.frameCount; i++)
            {
                var f = avi.getHapFrameAtIndex(i);
            }

            s.Stop();

            Console.WriteLine("Read " + avi.frameCount + " frames in " + s.Elapsed);

            Console.ReadLine();

        }
    }
}
