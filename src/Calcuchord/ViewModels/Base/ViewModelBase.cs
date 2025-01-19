using System.ComponentModel;

namespace Calcuchord {
    public abstract class ViewModelBase : INotifyPropertyChanged {
        public bool HasModelChanged { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase() {
            PropertyChanged += OnPropertyChanged_internal;
        }

        void OnPropertyChanged_internal(object sender,PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName,from_internal: true);
        }

        public virtual void OnPropertyChanged(string propertyName,bool from_internal = false) {
            if(propertyName == nameof(HasModelChanged) && HasModelChanged) {
                Prefs.Instance.Save();
                HasModelChanged = false;
            }

            if(from_internal) {
                return;
            }

            PropertyChanged?.Invoke(this,new(propertyName));
        }
    }

    public abstract class ViewModelBase<T> : ViewModelBase where T : ViewModelBase {
        public T Parent { get; set; }

        public ViewModelBase(T parent) {
            Parent = parent;
        }
    }
}