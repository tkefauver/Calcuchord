using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public static class CloseFlyoutExtension {
        static CloseFlyoutExtension() {
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
                Extensions.CloseFlyout(control);
            }

            void ControlOnUnloaded(object sender,RoutedEventArgs routedEventArgs) {
                control.Unloaded -= ControlOnUnloaded;
                if(FlyoutBase.GetAttachedFlyout(control) is not { } fl) {
                    return;
                }

                fl.Opened -= FlOnOpened;
                fl.Closed -= FlOnClosed;
            }

            void FlOnOpened(object sender,EventArgs eventArgs) {
                if(sender is not Flyout fl ||
                   fl.Content is not Control c ||
                   c.GetVisualDescendants<MenuItem>() is not { } mil) {
                    return;
                }

                mil.ForEach(x => x.PointerReleased += ControlOnPointerReleased);
            }

            void FlOnClosed(object sender,EventArgs eventArgs) {
                if(sender is not Flyout fl ||
                   fl.Content is not Control c ||
                   c.GetVisualDescendants<MenuItem>() is not { } mil) {
                    return;
                }

                mil.ForEach(x => x.PointerReleased -= ControlOnPointerReleased);
            }

            async void HostOnPointerReleased(object sender,PointerReleasedEventArgs pointerReleasedEventArgs) {
                await Task.Delay(300);
                if(control is not Button b ||
                   b.Flyout is not { } fl) {
                    return;
                }

                fl.Opened += FlOnOpened;
                fl.Closed += FlOnClosed;
                FlOnOpened(fl,EventArgs.Empty);
            }

            if(isEnabled) {
                control.AddHandler(
                    InputElement.PointerReleasedEvent,HostOnPointerReleased,RoutingStrategies.Tunnel,true);
                control.AddHandler(
                    InputElement.PointerReleasedEvent,HostOnPointerReleased,RoutingStrategies.Bubble,true);
                control.Unloaded += ControlOnUnloaded;
            } else {
                ControlOnUnloaded(control,null);
            }
        }

        #endregion

        #endregion

    }
}