using System.Windows.Input;

namespace Calcuchord {
    public class DialogViewModel : ViewModelBase {
        public string Label { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
    }
}