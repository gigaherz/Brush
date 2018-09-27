using System.ComponentModel;
using System.Runtime.CompilerServices;
using Brush.Annotations;

namespace Brush.Structure
{
    public class DocumentProperties : INotifyPropertyChanged
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Assign(DocumentProperties startupProperties)
        {
            // todo
            Width = startupProperties.Width;
            Height = startupProperties.Height;
        }
    }
}