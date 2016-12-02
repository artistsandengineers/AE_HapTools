using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V2;

namespace VVVV.HapTreats.Nodes
{
    [PluginInfo(Name = "HapAVIPlayer", Category = "HapTreats", Version = "1.0", Author = "arron")]
    public class AE_HapAVIPlayerNode : IPluginEvaluate, IDisposable
    {
        protected ISpread<bool> FValid;

        public void Evaluate(int spreadMax)
        {

        }

        public void Dispose()
        {

        }
    }
}
