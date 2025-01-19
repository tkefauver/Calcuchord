using Avalonia.Controls;
using Avalonia.Input;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public MainView() {
            InitializeComponent();
            YoBlock.PointerReleased += YoBlockOnPointerReleased;

        }

        void YoBlockOnPointerReleased(object sender,PointerReleasedEventArgs e) {
            var test = Prefs.Instance.Test;
            Prefs.Instance.Test = "Howdy";
        }
    }
}