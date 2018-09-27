using System;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace Brush.Structure
{
    public class TextLayer : LayerBase
    {
        private static readonly SharpDX.DirectWrite.Factory DWriteFactory = new SharpDX.DirectWrite.Factory();

        private string _text = "This is the text";
        private string _fontFamily = "Arial";
        private int _fontWeight = 0;
        private FontStyle _fontStyle = FontStyle.Normal;
        private float _fontSize = 14;
        private RawColor4 _color = new RawColor4(0,0,0,1);
        private TextLayout _tl;
        private SolidColorBrush _brush;

        public string Text
        {
            get => _text;
            set
            {
                if (Equals(value, _text)) return;
                _text = value;
                OnPropertyChanged();
                InvalidateTextRendering();
            }
        }

        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                if (value == _fontFamily) return;
                _fontFamily = value;
                OnPropertyChanged();
                InvalidateTextRendering();
            }
        }

        public int FontWeight
        {
            get => _fontWeight;
            set
            {
                if (value == _fontWeight) return;
                _fontWeight = value;
                OnPropertyChanged();
                InvalidateTextRendering();
            }
        }

        public FontStyle FontStyle
        {
            get => _fontStyle;
            set
            {
                if (value == _fontStyle) return;
                _fontStyle = value;
                OnPropertyChanged();
                InvalidateTextRendering();
            }
        }

        public float FontSize
        {
            get => _fontSize;
            set
            {
                if (value.Equals(_fontSize)) return;
                _fontSize = value;
                OnPropertyChanged();
                InvalidateTextRendering();
            }
        }

        public RawColor4 Color
        {
            get { return _color; }
            set
            {
                if (value.Equals(_color)) return;
                _color = value;
                OnPropertyChanged(nameof(Color));
                InvalidateBrush();
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
            case nameof(Opacity):
                InvalidateBrush();
                break;
            case nameof(Placement):
                InvalidateTextRendering();
                break;
            }
        }

        private void InvalidateTextRendering()
        {
            _tl?.Dispose();
            _tl = null;
        }

        private void InvalidateBrush()
        {
            _brush?.Dispose();
            _brush = null;
        }

        public override void Draw(RenderTarget rt)
        {
            if (Text != null)
            {
                if (_tl == null)
                {
                    _tl = new TextLayout(DWriteFactory, Text,
                        new TextFormat(DWriteFactory, FontFamily, ClosestFontWeight(FontWeight), FontStyle, FontSize),
                        Placement.Right - Placement.Left, Placement.Bottom - Placement.Top);
                }
                if (_brush == null)
                {
                    _brush = new SolidColorBrush(rt, new RawColor4(Color.R, Color.G, Color.B, Color.A * Opacity));
                }
                rt.DrawTextLayout(new RawVector2(Placement.Left, Placement.Top), _tl, _brush);
            }
            base.Draw(rt);
        }

        private static FontWeight ClosestFontWeight(int fontWeight)
        {
            FontWeight best = SharpDX.DirectWrite.FontWeight.Normal;
            int difference = int.MaxValue;
            foreach(FontWeight fw in Enum.GetValues(typeof(SharpDX.DirectWrite.FontWeight)))
            {
                int fwInt = (int)fw;
                int diff = Math.Abs(fwInt - fontWeight);
                if (diff < difference || (diff == difference && Math.Abs((int)best) < Math.Abs(fwInt)))
                {
                    best = fw;
                    difference = diff;
                }
            }
            return best;
        }

        public TextLayer(RawRectangleF defaultPlacement) : base(defaultPlacement)
        {
        }
    }
}