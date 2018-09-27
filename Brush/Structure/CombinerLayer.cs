using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace Brush.Structure
{
    public class CombinerLayer : LayerBase
    {
        public override bool IsContainer => true;

        public CombinerLayer(RawRectangleF defaultPlacement) : base(defaultPlacement)
        {
        }
    }
}
