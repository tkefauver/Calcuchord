using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class HintView : UserControl {
        public HintView() {
            InitializeComponent();
        }

        void InputElement_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(TopLevel.GetTopLevel(this) is not { } tl) {
                return;
            }

            void TopLevel_OnPointerPressed(object sender2,PointerPressedEventArgs e2) {
                e2.Handled = true;

                tl.RemoveHandler(PointerPressedEvent,TopLevel_OnPointerPressed);
                tl.RemoveHandler(PointerPressedEvent,TopLevel_OnPointerPressed);
                ToolTip.SetIsOpen(this,false);
            }

            tl.AddHandler(PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Tunnel,true);
            tl.AddHandler(PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Bubble,true);
            ToolTip.SetIsOpen(this,true);
        }
    }
}