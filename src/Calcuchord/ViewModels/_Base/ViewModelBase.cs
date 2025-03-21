using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Calcuchord {
    public abstract partial class ViewModelBase : INotifyPropertyChanged {

        public void RaisePropertyChanged(string propertyName) {
            OnPropertyChanged(propertyName);
        }

        // [JsonIgnore]
        // public bool HasModelChanged { get; set; }

        //public event PropertyChangedEventHandler PropertyChanged;

        // void OnPropertyChanged_internal(object sender,PropertyChangedEventArgs e) {
        //     OnPropertyChanged(e.PropertyName,true);
        // }
        //
        // public virtual void OnPropertyChanged(string propertyName,bool from_internal = false) {
        //     // if(propertyName == nameof(HasModelChanged) && HasModelChanged) {
        //     //     Prefs.Instance.Save();
        //     //     HasModelChanged = false;
        //     // }
        //     //
        //     // if(from_internal) {
        //     //     return;
        //     // }
        //
        //     PropertyChanged?.Invoke(this,new(propertyName));
        // }
    }

    public abstract partial class ViewModelBase<T> : ViewModelBase where T : ViewModelBase {
        [JsonIgnore]
        public T Parent { get; set; }

        protected ViewModelBase(T parent) {
            Parent = parent;
        }

        protected ViewModelBase() {
        }
    }
}