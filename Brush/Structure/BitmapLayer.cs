using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Brush.Structure
{
    public class BitmapLayer : LayerBase
    {
        private Bitmap _bitmap;

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                if (Equals(value, _bitmap)) return;
                _bitmap = value;
                OnPropertyChanged();
            }
        }

        public override void Draw(RenderTarget rt)
        {
            if (Bitmap != null)
            {
                rt.DrawBitmap(Bitmap, Placement, Opacity, BitmapInterpolationMode.Linear);
                //rt.DrawTextLayout(new RawVector2(50, 50), tl, brush);
            }
            base.Draw(rt);
        }

        public BitmapLayer(RawRectangleF defaultPlacement) : base(defaultPlacement)
        {
        }
    }
}
