using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace Brush.Structure
{
    public class BackgroundLayer : LayerBase
    {
        public override bool CanToggleTransparencyLock => false;
        public override bool TransparencyLocked
        {
            get => false;
            set => throw new NotSupportedException();
        }

        public BackgroundLayer(RawRectangleF defaultPlacement) : base(defaultPlacement)
        {
        }
    }
}
