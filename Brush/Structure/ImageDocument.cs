using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace Brush.Structure
{
    public class ImageDocument : LayerBase
    {
        public DocumentProperties Properties { get; } = new DocumentProperties();
        public DocumentHistory History { get; } = new DocumentHistory();

        public override bool IsContainer => true;

        public int LastLayerNumber = 0;
        public int LastGroupNumber = 0;

        public ImageDocument(DocumentProperties startupProperties) 
            : base(new RawRectangleF(0, 0, startupProperties.Width, startupProperties.Height))
        {
            Properties.Assign(startupProperties);
            History.RecordAction(new UndoableAction()
            {
                Name = "Document Created",
                Do = () => throw new InvalidOperationException(),
                Undo = () => throw new InvalidOperationException()
            });
            AddLayer(this, new TextLayer(Placement));
        }

        public ImageDocument(Bitmap bitmap)
            : base(new RawRectangleF(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height))
        {
            Properties.Width = bitmap.PixelSize.Width;
            Properties.Height = bitmap.PixelSize.Height;
            Layers.Add(new BitmapLayer(Placement)
            {
                Bitmap = bitmap,
                Placement = new RawRectangleF(0, 0, Properties.Width, Properties.Height)
            });
            History.RecordAction(new UndoableAction()
            {
                Name = "Document Loaded",
                Do = () => throw new InvalidOperationException(),
                Undo = () => throw new InvalidOperationException()
            });
        }

        public LayerBase FindLayer(string name)
        {
            return FindLayer(this, name);
        }

        private LayerBase FindLayer(LayerBase parent, string name)
        {
            foreach (var layer in parent.Layers)
            {
                if (string.CompareOrdinal(layer.Name, name) == 0)
                    return layer;
            }
            foreach (var layer in parent.Layers.Where(lb => lb.IsContainer))
            {
                var l = FindLayer(layer, name);
                if (l != null)
                    return l;
            }
            return null;
        }

        public void AddLayer(LayerBase parent, LayerBase newLayer, LayerBase insertAfter = null)
        {
            if (insertAfter != null)
            {
                parent.Layers.Insert(parent.Layers.IndexOf(insertAfter) + 1, newLayer);
            }
            else
            {
                parent.Layers.Add(newLayer);
            }
            History.RecordAction(new UndoableAction()
            {
                Name = $"Create Layer {newLayer.Name}",
                Do = () => AddLayer(parent, newLayer, insertAfter),
                Undo = () => parent.Layers.Remove(newLayer)
            });
        }

        private static Bitmap BitmapFromFile(RenderTarget renderTarget, string filename)
        {
            Bitmap result;
            using (var img = System.Drawing.Image.FromFile(filename))
            {
                if (!(img is System.Drawing.Bitmap bmp))
                {
                    throw new NotSupportedException("The loaded image is not a Bitmap");
                }

                BitmapData bmpData = null;
                try
                {
                    bmpData = bmp.LockBits(
                        new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    using (var stream = new DataStream(bmpData.Scan0, bmpData.Stride * bmpData.Height, true, false))
                    {
                        var pFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
                        var bmpProps = new BitmapProperties(pFormat);

                        result = new Bitmap(
                                renderTarget,
                                new Size2(bmp.Width, bmp.Height),
                                stream,
                                bmpData.Stride,
                                bmpProps);
                    }
                }
                finally
                {
                    if (bmpData != null)
                        bmp.UnlockBits(bmpData);
                }
            }

            return result;
        }

        public static ImageDocument FromFile(RenderTarget rt, string filename)
        {
            var bitmap = BitmapFromFile(rt, filename);
            return new ImageDocument(bitmap);
        }
    }

    public class HistoryException : Exception
    {
    }
}
