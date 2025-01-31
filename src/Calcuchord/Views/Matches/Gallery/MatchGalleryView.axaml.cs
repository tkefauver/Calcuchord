using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchGalleryView : UserControl {
        int _lastCols;

        public MatchGalleryView() {
            InitializeComponent();
            MatchZoomSlider.Loaded += MatchZoomSliderOnLoaded;
            MatchItemsRepeater.Loaded += MatchItemsRepeaterOnLoaded;
        }

        void MatchZoomSliderOnLoaded(object sender,RoutedEventArgs e) {
            if(MatchZoomSlider.GetVisualDescendant<Thumb>() is { } thumb) {
                thumb.AddHandler(PointerReleasedEvent,MatchZoom_ThumbOnPointerReleased,RoutingStrategies.Bubble,true);
            }
            //MatchZoomSlider.GetObservable(RangeBase.ValueProperty).Subscribe(value => SizeMatches());
        }

        void MatchZoom_ThumbOnPointerReleased(object sender,PointerReleasedEventArgs e) {
            OnZoomSliderChanged();
        }


        void MatchItemsRepeaterOnLoaded(object sender,RoutedEventArgs e) {
            SizeMatches();
        }

        void OnZoomSliderChanged() {
            if(!MatchZoomSlider.IsLoaded ||
               MainViewModel.Instance is not { } mvm) {
                return;
            }

            mvm.MatchZoom = MatchZoomSlider.Value == 0 ? mvm.MatchZoom : MatchZoomSlider.Value;
            SizeMatches();
        }


        void SizeMatches() {
            int cols = GetZoomCols();
            if(cols == _lastCols) {
                return;
            }

            _lastCols = cols;
            MainViewModel.Instance.MaxMatchWidth = Math.Max(10,(GetCntrWidth() - (cols * 5)) / cols);
            Debug.WriteLine($"col: {cols}");
        }

        double GetCntrWidth() {
            return MatchItemsRepeater.Bounds.Right - MatchItemsRepeater.Margin.Right;
        }

        int GetZoomCols() {
            if(MainViewModel.Instance is not { } mvm ||
               MatchItemsRepeater is not { } mir ||
               mir.Layout is not WrapLayout wl) {
                return 1;
            }

            double pad = 0;
            double zoom = MatchZoomSlider.IsLoaded ? MatchZoomSlider.Value : mvm.MatchZoom;
            double raw_w = MainViewModel.DefaultMatchWidth * zoom;
            double cntr_w = GetCntrWidth();
            double cur_r = 0;
            int cols = 0;
            while(cur_r + raw_w + pad < cntr_w) {
                cur_r += raw_w + pad;
                cols++;
            }

            double final_w = raw_w;
            double diff = cntr_w - (raw_w * cols) - (pad * cols);
            if(diff > raw_w / 2d) {
                cols++;
            }

            return Math.Max(1,cols);
        }

    }
}