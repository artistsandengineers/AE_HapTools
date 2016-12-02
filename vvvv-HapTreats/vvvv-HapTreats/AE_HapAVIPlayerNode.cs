using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using AE_HapTools;

namespace VVVV.HapTreats.Nodes
{
    [PluginInfo(Name = "HapAVIPlayer", Category = "HapTreats", Version = "1.0", Author = "arron")]
    public class AE_HapAVIPlayerNode : IPluginEvaluate, IDisposable
    {

        [Input("Filename", StringType = StringType.Filename, IsSingle = true)]
        protected IDiffSpread<string> inputFilename;

        [Output("Size")]
        protected ISpread<Vector2> size;

        [Output("Frame Count")]
        protected ISpread<int> frameCount;



        private AE_HapAVI avi;

        private void init()
        {
            if (inputFilename[0] == null) return;

            avi = new AE_HapAVI(inputFilename[0]);

            size[0] = new Vector2(avi.imageWidth, avi.imageHeight);
            frameCount[0] = avi.frameCount;
           
        }

        public void Evaluate(int spreadMax)
        {
            if (inputFilename.IsChanged) init();
        }

        public void Dispose()
        {

        }
    }
}
