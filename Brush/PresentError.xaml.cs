using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brush.Annotations;

namespace Brush
{
    /// <summary>
    /// Interaction logic for PresentError.xaml
    /// </summary>
    public partial class PresentError : INotifyPropertyChanged
    {
        private Exception _reportingException;
        private string _message;
        private string _details;
        private bool _showDetails;

        public Exception ReportingException
        {
            get => _reportingException;
            private set
            {
                if (Equals(value, _reportingException)) return;
                _reportingException = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public string Details
        {
            get => _details;
            private set
            {
                if (value == _details) return;
                _details = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDetails
        {
            get => _showDetails;
            set
            {
                if (value == _showDetails) return;
                _showDetails = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DetailsVisibility));
            }
        }

        public Visibility DetailsVisibility => ShowDetails ? Visibility.Visible : Visibility.Collapsed;

        protected PresentError(Exception e)
        {
            InitializeComponent();
            this.ReportingException = e;
            this.Title = $"Caught exception {e.GetType().Name}";
            this.Message = $"The operation ended because of an exception:{Environment.NewLine}{e.Message}";
            this.Details = $"{e}{Environment.NewLine}{e.StackTrace}";
        }

        public static bool? Show(Exception e)
        {
            return new PresentError(e).ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(1);
            DialogResult = false;
            Close();
        }
    }
}
