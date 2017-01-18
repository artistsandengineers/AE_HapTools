using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SlimDX;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

using AE_HapTools;
using System.IO;

namespace VVVV.HapTreats.Nodes
{
    [PluginInfo(Name = "HapAVITexture", Category = "HapTreats", Version = "1.0", Author = "A&E")]
    public class AE_HapAVITextureNode : IPluginEvaluate, IDisposable, IDX11ResourceProvider
    {

        [Input("Filename", StringType = StringType.Filename, IsSingle = true)]
        protected IDiffSpread<string> inputFilename;

        [Input("Frame Index", IsSingle = true)]
        protected IDiffSpread<int> frameIndex;

        [Output("Texture")]
        protected Pin<DX11Resource<DX11Texture2D>> outputTexture;

        [Output("Current Frame Format")]
        protected ISpread<SlimDX.DXGI.Format> currentFrameFormat;

        [Output("Size")]
        protected ISpread<Vector2> size;

        [Output("Frame Rate")]
        protected ISpread<float> frameRate;

        [Output("Frame Count")]
        protected ISpread<int> frameCount;

        private AE_HapAVI avi;
        private bool isValid = false;
        private bool currentFrameChanged = false;
        private AE_HapFrame currentFrame;

        private void reset()
        {
            isValid = false;
            size[0] = new Vector2();
            frameCount[0] = 0;
            outputTexture[0] = new DX11Resource<DX11Texture2D>();

            currentFrame = null;
        }

        private void init()
        {
            if (inputFilename[0] == null || ! File.Exists(inputFilename[0]))
            {
                reset();
                return;
            }

            avi = new AE_HapAVI(inputFilename[0]);

            size[0] = new Vector2(avi.imageWidth, avi.imageHeight);
            frameRate[0] = avi.frameRate;
            frameCount[0] = avi.frameCount;

            outputTexture[0] = new DX11Resource<DX11Texture2D>();

            isValid = true;

            getFrameAtIndex(0);
        }

        private void getFrameAtIndex(int index)
        {
            if (!isValid) return;

            currentFrame = avi.getHapFrameAndDDSHeaderAtIndex(index % avi.frameCount);
            currentFrameFormat[0] = (SlimDX.DXGI.Format)currentFrame.compressionType;

            currentFrameChanged = true;
        }

        public void Evaluate(int spreadMax)
        {
            if (inputFilename.IsChanged) init();

            if (frameIndex.IsChanged) getFrameAtIndex(frameIndex[0]);

        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (! isValid) return;
            if (!currentFrameChanged) return;

            var tex = outputTexture[0][context];

            if (tex != null) tex.Dispose();

            SlimDX.Direct3D11.ImageLoadInformation li = SlimDX.Direct3D11.ImageLoadInformation.FromDefaults();
            li.MipLevels = 1;
            outputTexture[0][context] = DX11Texture2D.FromMemory(context, currentFrame.frameData, li);

            currentFrameChanged = false;
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (outputTexture[0] != null) outputTexture[0].Dispose();
        }

        public void Dispose()
        {

        }
    }
}
