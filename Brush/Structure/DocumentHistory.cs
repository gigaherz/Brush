using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Brush.Annotations;

namespace Brush.Structure
{
    public class DocumentHistory: INotifyPropertyChanged
    {
        private int _currentStateIndex;
        public ObservableCollection<UndoableAction> History { get; } = new ObservableCollection<UndoableAction>();

        public UndoableAction CurrentState => History[_currentStateIndex];

        public int CurrentStateIndex
        {
            get => _currentStateIndex;
            set
            {
                if (value == _currentStateIndex) return;
                _currentStateIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentState));
            }
        }

        public void RecordAction(UndoableAction action)
        {
            while (_currentStateIndex < History.Count - 1)
            {
                History.RemoveAt(History.Count - 1);
            }
            History.Add(action);
            CurrentStateIndex = History.Count - 1;
        }

        public void WalkBack()
        {
            if (_currentStateIndex > 0)
            {
                try
                {
                    CurrentState.Undo();
                    CurrentStateIndex--;
                }
                catch (HistoryException e)
                {
                    PresentError.Show(e);
                }
            }
        }

        public void WalkForward()
        {
            if (_currentStateIndex < History.Count - 1)
            {
                try
                {
                    History[CurrentStateIndex + 1].Do();
                    CurrentStateIndex++;
                }
                catch (HistoryException e)
                {
                    PresentError.Show(e);
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