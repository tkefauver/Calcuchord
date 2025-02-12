using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Labs.Controls;
using Material.Icons.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class LeftDrawerView : UserControl {
        public LeftDrawerView() {
            InitializeComponent();
        }

        void InputElement_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(sender is not Control c ||
               c.GetVisualAncestor<Swipe>() is not { } swipe) {
                return;
            }

            if(swipe.SwipeState == SwipeState.Hidden) {
                swipe.SwipeState = SwipeState.LeftVisible;
            } else {
                swipe.SwipeState = SwipeState.Hidden;
            }
        }

        void Button_OnClick(object sender,RoutedEventArgs e) {
            Enumerable.Range(0,InstrumentListBox.ItemCount).ForEach(
                x => InputElement_OnPointerReleased(
                    InstrumentListBox.ContainerFromIndex(x).GetVisualDescendant<MaterialIcon>(),null));
        }
    }
}