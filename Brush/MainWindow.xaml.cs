using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Brush.Annotations;
using Brush.Structure;
using Microsoft.Win32;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using SolidColorBrush = SharpDX.Direct2D1.SolidColorBrush;

namespace Brush
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly Factory Direct2DFactory = new Factory();

        private ImageDocument _currentDocument = new ImageDocument(new DocumentProperties() { Width = 800, Height = 600 });

        private RenderTarget _rt;
        private TimeSpan _lastRender;

        public ImageDocument CurrentDocument
        {
            get => _currentDocument;
            set
            {
                var oldProps = _currentDocument.Properties;
                _currentDocument.Properties.PropertyChanged -= PropertiesOnPropertyChanged;
                if (Equals(value, _currentDocument)) return;
                _currentDocument = value;
                OnPropertyChanged();
                _currentDocument.Properties.PropertyChanged += PropertiesOnPropertyChanged;
                if (oldProps.Width != _currentDocument.Properties.Width ||
                    oldProps.Height != _currentDocument.Properties.Height)
                    DocumentSizeChanged();
            }
        }

        private void PropertiesOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(DocumentProperties.Width)
                || propertyChangedEventArgs.PropertyName == nameof(DocumentProperties.Height))
            {
                DocumentSizeChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ImageHost.Loaded += this.Host_Loaded;
            //ImageHost.SizeChanged += this.Host_SizeChanged;
            ImageHost.Unloaded += Host_Unloaded;
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            UninitializeRendering();
        }

        private void Host_Unloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            UninitializeRendering();
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeRendering();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void DocumentSizeChanged()
        {
            _rt?.Dispose();
            _rt = null;

            ImageBitmap.SetPixelSize(CurrentDocument.Properties.Width, CurrentDocument.Properties.Height);

            ImageBitmap.RequestRender();
        }

        #region Helpers
        private void InitializeRendering()
        {
            ImageBitmap.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
            ImageBitmap.OnRender = OnRender;

            //DocumentSizeChanged();
            ImageBitmap.SetPixelSize(CurrentDocument.Properties.Width, CurrentDocument.Properties.Height);
            ImageBitmap.RequestRender();
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (this._lastRender != args.RenderingTime)
            {
                ImageBitmap.RequestRender();
                this._lastRender = args.RenderingTime;
            }
        }

        private void UninitializeRendering()
        {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
        }
        #endregion Helpers

        private void OnRender(IntPtr handle, bool isNewSurface)
        {
            try
            {
                if (_rt == null)
                {
                    var r = new SharpDX.ComObject(handle);
                    var rgi = r.QueryInterface<SharpDX.DXGI.Resource>();
                    var t = rgi.QueryInterface<SharpDX.Direct3D11.Texture2D>();
                    using (var surface = t.QueryInterface<Surface>())
                    {
                        var properties = new RenderTargetProperties
                        {
                            DpiX = 96,
                            DpiY = 96,
                            MinLevel = FeatureLevel.Level_DEFAULT,
                            PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                            Type = RenderTargetType.Default,
                            Usage = RenderTargetUsage.None
                        };
                        _rt = new RenderTarget(Direct2DFactory, surface, properties);
                    }
                }

                _rt.BeginDraw();
                CurrentDocument.Draw(_rt);
                _rt.EndDraw();
            }
            catch(Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void StructureTree_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (StructureTree.SelectedValue is LayerBase lb)
                {
                    lb.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void LayerItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var tvi = sender as TreeViewItem;
                if (tvi.Header is LayerBase lb)
                {
                    if (lb.HasChildren)
                    {
                        lb.IsExpanded = !lb.IsExpanded;
                    }
                }
            }
            catch (Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void CreateNewLayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tl = new BitmapLayer(CurrentDocument.Placement);
                do
                {
                    tl.Name = $"Layer {++CurrentDocument.LastLayerNumber}";
                }
                while (CurrentDocument.FindLayer(tl.Name) != null);
                CreateNewLayer(tl);
            }
            catch (Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void CreateNewGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cl = new CombinerLayer(CurrentDocument.Placement);
                do
                {
                    cl.Name = $"Group {++CurrentDocument.LastGroupNumber}";
                }
                while (CurrentDocument.FindLayer(cl.Name) != null);
                CreateNewLayer(cl);
            }
            catch (Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void CreateNewLayer(LayerBase newLayer)
        {
            if (StructureTree.SelectedValue is LayerBase lb)
            {
                if (lb.IsContainer)
                {
                    CurrentDocument.AddLayer(lb, newLayer);
                }
                else
                {
                    CurrentDocument.AddLayer(lb.Parent, newLayer, lb);
                }
            }
            else
            {
                CurrentDocument.AddLayer(CurrentDocument, newLayer);
            }
            newLayer.IsSelected = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentDocument = new ImageDocument(new DocumentProperties() { Width = 302, Height = 405 });
            }
            catch (Exception ex)
            {
                PresentError.Show(ex);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    CurrentDocument = ImageDocument.FromFile(_rt, openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    PresentError.Show(ex);
                }
            }
        }
    }
}
