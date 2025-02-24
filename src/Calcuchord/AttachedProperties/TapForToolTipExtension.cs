using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Calcuchord {
    public static class TapForToolTipExtension {
        static TapForToolTipExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x,y) => HandleIsEnabledChanged(x,y));
        }

        #region Properties

        #region IsEnabled AvaloniaProperty

        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj,bool value) {
            obj.SetValue(IsEnabledProperty,value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object,Control,bool>(
                "IsEnabled");

        static void HandleIsEnabledChanged(Control control,AvaloniaPropertyChangedEventArgs e) {
            if(e.NewValue is not bool isEnabled) {
                return;
            }

            void ControlOnPointerReleased(object sender,PointerReleasedEventArgs pointerReleasedEventArgs) {
                // find control with a at/below attached control w/ a defined tooltip (shouldn't be more
                // than one or they're all the same for now at least)
                if(sender is not Control c ||
                   c.GetSelfAndVisualDescendants()
                       .OfType<Control>()
                       .FirstOrDefault(x => ToolTip.GetTip(x) != null) is not { } tt_host_c ||
                   TopLevel.GetTopLevel(c) is not { } tl) {
                    return;
                }

                void TopLevel_OnPointerPressed(object sender2,PointerPressedEventArgs e2) {
                    e2.Handled = true;
                    tl.RemoveHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed);
                    tl.RemoveHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed);
                    ToolTip.SetIsOpen(tt_host_c,false);
                }

                tl.AddHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Tunnel,true);
                tl.AddHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Bubble,true);
                ToolTip.SetIsOpen(tt_host_c,true);
            }

            void ControlOnUnloaded(object sender,RoutedEventArgs routedEventArgs) {
                control.PointerReleased -= ControlOnPointerReleased;
                control.Unloaded -= ControlOnUnloaded;
            }

            if(isEnabled) {
                control.PointerReleased += ControlOnPointerReleased;
                control.Unloaded += ControlOnUnloaded;
            } else {
                ControlOnUnloaded(control,null);
            }
        }

        #endregion

        #endregion

    }
}