using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainDrawerView : UserControl {
        public MainDrawerView() {
            InitializeComponent();

        }


        void InstrumentListBox_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(!Enumerable.Range(0,InstrumentListBox.ItemCount).Select(x => InstrumentListBox.ContainerFromIndex(x))
                   .Where(x => x != null)
                   .Any(x => x.Bounds.Contains(e.GetPosition(x))) ||
               !e.IsLeftRelease(e.Source as Visual)) {
                return;
            }

            MainViewModel.Instance.ForwardCommand.Execute(null);

        }
    }
}