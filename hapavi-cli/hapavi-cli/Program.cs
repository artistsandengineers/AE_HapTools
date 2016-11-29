using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hapavi_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var avi = new AE_HapAVI("c:/Users/arron/Downloads/long.avi");

            Console.WriteLine("Width: " + avi.imageWidth + " Height: " + avi.imageHeight);

            Console.WriteLine("Frame Count: " + avi.frameCount);

            for (int i = 0; i < avi.frameCount; i++)
            {
                var f = avi.getHapFrameAtIndex(i);
            }

            Console.ReadLine();
        }
    }
}
