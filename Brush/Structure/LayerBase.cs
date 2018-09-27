using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Brush.Annotations;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Brush.Structure
{
    public abstract class LayerBase : INotifyPropertyChanged
    {
        private LayerBase _parent;
        private bool _transparencyLocked;
        private string _name;
        private bool _isExpanded = true;
        private bool _isSelected;
        private float _opacity = 1;
        private RawRectangleF _placement;

        public virtual bool CanToggleTransparencyLock => true;

        public virtual bool IsContainer => false;

        public ObservableCollection<LayerBase> Layers { get; } = new ObservableCollection<LayerBase>();
        public bool HasChildren => Layers.Count > 0;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public virtual bool TransparencyLocked
        {
            get { return _transparencyLocked; }
            set
            {
                if (value == _transparencyLocked) return;
                _transparencyLocked = value;
                OnPropertyChanged();
            }
        }

        public LayerBase Parent
        {
            get { return _parent; }
            set
            {
                if (Equals(value, _parent)) return;
                _parent = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value.Equals(_opacity)) return;
                _opacity = value;
                OnPropertyChanged();
            }
        }

        public RawRectangleF Placement
        {
            get => _placement;
            set
            {
                if (value.Equals(_placement)) return;
                _placement = value;
                OnPropertyChanged();
            }
        }

        protected LayerBase(RawRectangleF defaultPlacement)
        {
            Placement = defaultPlacement;
            Layers.CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (LayerBase item in args.OldItems)
                    {
                        item.Parent = null;
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (LayerBase item in args.NewItems)
                    {
                        item.Parent = this;
                    }
                }
                OnPropertyChanged(nameof(HasChildren));
            };
        }

        public virtual void Draw(RenderTarget rt)
        {
            if (HasChildren)
            {
                foreach (var layer in Layers)
                {
                    layer.Draw(rt);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
