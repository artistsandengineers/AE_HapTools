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
using AE_Hap2DDS;
using System.IO;

namespace VVVV.HapTreats.Nodes
{
    [PluginInfo(Name = "HapAVIPlayer", Category = "HapTreats", Version = "1.0", Author = "arron")]
    public class AE_HapAVIPlayerNode : IPluginEvaluate, IDisposable, IDX11ResourceProvider
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

        [Output("Frame Count")]
        protected ISpread<int> frameCount;

        private AE_HapAVI avi;
        private bool isValid = false;
        private bool currentFrameChanged = false;
        private AE_HapFrame currentFrame;

        private AE_DDS dds;
        private byte[] frameData;

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
            frameCount[0] = avi.frameCount;

            outputTexture[0] = new DX11Resource<DX11Texture2D>();

            getFrameAtIndex(0);

            dds = new AE_DDS(avi.imageWidth, avi.imageHeight);
            dds.header.flags = (UInt32)(AE_DDSFlags.CAPS | AE_DDSFlags.HEIGHT | AE_DDSFlags.WIDTH | AE_DDSFlags.PIXELFORMAT | AE_DDSFlags.LINEARSIZE);
            dds.header.pixelFormat.flags = (UInt32)AE_DDSPixelFormats.FOURCC;
            dds.header.caps = (UInt32)AE_DDSCaps.TEXTURE;
            dds.header.pixelFormat.fourCC = AE_CopyPastedFromStackOverflow.string2FourCC("DXT1");

            frameData = new byte[256 + currentFrame.frameData.Length];

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    dds.header.write(writer);
                }
                stream.Flush();
                byte[] bytes = stream.GetBuffer();
                bytes.CopyTo(frameData, 0);
            }

            isValid = true;
        }

        private void getFrameAtIndex(int index)
        {
            if (index > avi.frameCount) return;

            currentFrame = avi.getHapFrameAtIndex(index);
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

            currentFrame.frameData.CopyTo(frameData, 256);

            outputTexture[0][context] = DX11Texture2D.FromMemory(context, frameData);

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
