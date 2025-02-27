using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Calcuchord {
    public static class ToolTipHelperExtension {
        static ToolTipHelperExtension() {
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

            async void ControlOnPointerEntered(object sender,PointerEventArgs pointerEventArgs) {
                // Dispatcher.UIThread.Post(
                //     async () => {
                DateTime st = DateTime.Now;
                while(DateTime.Now - st < TimeSpan.FromMilliseconds(ToolTip.GetShowDelay(control))) {
                    await Task.Delay(10);
                }

                if(!control.IsPointerOver) {
                    return;
                }

                ToolTip.SetIsOpen(control,true);

                //});
            }

            void ControlOnPointerExited(object o,PointerEventArgs pointerEventArgs) {
                ToolTip.SetIsOpen(control,false);
            }

            void ControlOnUnloaded(object sender,RoutedEventArgs routedEventArgs) {
                control.PointerEntered -= ControlOnPointerEntered;
                control.PointerExited -= ControlOnPointerExited;
                control.Unloaded -= ControlOnUnloaded;
            }

            if(isEnabled) {
                control.PointerEntered += ControlOnPointerEntered;
                control.PointerExited += ControlOnPointerExited;
                control.Unloaded += ControlOnUnloaded;
            } else {
                ControlOnUnloaded(control,null);
            }
        }

        #endregion

        #endregion

    }
}