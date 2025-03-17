using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MonkeyPaste.Common;

namespace Calcuchord {
    public static class LoadMoreExtension {
        static readonly Dictionary<ScrollViewer,CompositeDisposable> _disposableLookup = [];

        static LoadMoreExtension() {
            IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>((x,y) => HandleIsEnabledChanged(x,y));
        }

        #region Properties

        #region Threshold AvaloniaProperty

        public static double GetThreshold(AvaloniaObject obj) {
            return obj.GetValue(ThresholdProperty);
        }

        public static void SetThreshold(AvaloniaObject obj,double value) {
            obj.SetValue(ThresholdProperty,value);
        }

        public static readonly AttachedProperty<double> ThresholdProperty =
            AvaloniaProperty.RegisterAttached<object,Control,double>(
                "Threshold");

        #endregion

        #region LoadMoreCommand AvaloniaProperty

        public static ICommand GetLoadMoreCommand(AvaloniaObject obj) {
            return obj.GetValue(LoadMoreCommandProperty);
        }

        public static void SetLoadMoreCommand(AvaloniaObject obj,ICommand value) {
            obj.SetValue(LoadMoreCommandProperty,value);
        }

        public static readonly AttachedProperty<ICommand> LoadMoreCommandProperty =
            AvaloniaProperty.RegisterAttached<object,Control,ICommand>(
                "LoadMoreCommand");

        #endregion

        #region LoadMoreCommandParameter AvaloniaProperty

        public static object GetLoadMoreCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LoadMoreCommandParameterProperty);
        }

        public static void SetLoadMoreCommandParameter(AvaloniaObject obj,object value) {
            obj.SetValue(LoadMoreCommandParameterProperty,value);
        }

        public static readonly AttachedProperty<object> LoadMoreCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object,Control,object>(
                "LoadMoreCommandParameter");

        #endregion

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

        static void HandleIsEnabledChanged(ScrollViewer sv,AvaloniaPropertyChangedEventArgs e) {
            if(e.NewValue is not bool isEnabled) {
                return;
            }

            void Dispose(ScrollViewer sv) {
                if(_disposableLookup.TryGetValue(sv,out CompositeDisposable disposable)) {
                    disposable.Dispose();
                    _disposableLookup.Remove(sv);
                }

            }

            void SvOnUnloaded(object sender,RoutedEventArgs routedEventArgs) {
                sv.Unloaded -= SvOnUnloaded;
                Dispose(sv);
            }

            if(isEnabled) {
                sv.Unloaded += SvOnUnloaded;

                Dispose(sv);
                CompositeDisposable sv_disp = new CompositeDisposable();
                _disposableLookup.AddOrReplace(sv,sv_disp);
                Vector lastOffset = default;

                sv.GetObservable(ScrollViewer.OffsetProperty)
                    .Subscribe(
                        offset => {
                            double bottom_offset = offset.Y + sv.Bounds.Height;
                            double percent = bottom_offset / sv.Extent.Height;
                            if(offset.Y > double.Epsilon &&
                               sv.Extent.Height > double.Epsilon &&
                               offset.Y > lastOffset.Y &&
                               percent >= GetThreshold(sv)) {
                                if(GetLoadMoreCommand(sv) is { } cmd) {
                                    cmd.Execute(GetLoadMoreCommandParameter(sv));
                                }
                            }

                            lastOffset = offset;
                        }).DisposeWith(sv_disp);
            } else {
                SvOnUnloaded(sv,null);
            }
        }

        #endregion

        #endregion

    }
}