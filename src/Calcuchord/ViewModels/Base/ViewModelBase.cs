using System;
using System.ComponentModel;

namespace Calcuchord;

public abstract class ViewModelBase : INotifyPropertyChanged {
    public bool HasModelChanged{ get; set; }
    public event PropertyChangedEventHandler PropertyChanged;

    protected ViewModelBase() {
        PropertyChanged += OnPropertyChanged_internal;
    }

    private void OnPropertyChanged_internal(object sender, PropertyChangedEventArgs e) {
        throw new NotImplementedException();
    }

    public virtual void OnPropertyChanged(string propertyName, bool from_internal = false) {
        if (propertyName == nameof(HasModelChanged) && HasModelChanged)
            // save here
            HasModelChanged = false;

        if (from_internal) return;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public abstract class ViewModelBase<T> : ViewModelBase where T : ViewModelBase {
    public T Parent{ get; set; }

    protected ViewModelBase(T parent) {
        Parent = parent;
    }
}