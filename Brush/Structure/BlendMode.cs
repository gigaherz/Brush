using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brush.Structure
{
    public enum BlendMode
    {
        Ignore,

        Replace,

        Normal,

        Add,
        Multiply,

        LighterColor,
        DarkerColor,

        KeepColor,
        KeepLightness,
        KeepSaturation,

        ReplaceColor,
        ReplaceLightness,
        ReplaceSaturation
    }
}
