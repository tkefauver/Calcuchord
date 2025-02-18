using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class AboutView : UserControl {
        public AboutView() {
            InitializeComponent();
        }

        DispatcherTimer dt;

        void PopupZone_OnLoaded(object sender,RoutedEventArgs e) {
            dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            dt.Tick += DtOnTick;
            dt.Start();
        }

        void DtOnTick(object sender,EventArgs e) {
            if(PopupZone.BorderBrush is not LinearGradientBrush lgb) {
                return;
            }

            double x = lgb.StartPoint.Point.X + 0.01;
            if(x > 1) {
                x = 0;
            }

            lgb.StartPoint = new RelativePoint(x,lgb.StartPoint.Point.Y,RelativeUnit.Relative);
            PopupZone.InvalidateVisual();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            if(dt == null) {
                return;
            }

            dt.Stop();
            dt.Tick -= DtOnTick;
            dt = null;

        }
    }
}