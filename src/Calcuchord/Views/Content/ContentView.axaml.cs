using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class ContentView : UserControl {
        public ContentView() {
            InitializeComponent();
        }

        public void FitInstrument(Control inst,Viewbox vb,bool forceFit) {
            double inner_ar = Math.Min(6.44,inst.Width / inst.Height);
            if(ThemeViewModel.Instance.IsExpandedLayout) {
                vb.HorizontalAlignment = HorizontalAlignment.Center;
                vb.Stretch = Stretch.UniformToFill;
            } else {
                vb.HorizontalAlignment = HorizontalAlignment.Left;
                vb.Stretch = Stretch.Uniform;
            }

            double outer_w = Bounds.Width - vb.Margin.Left - vb.Margin.Right;
            double outer_h = Bounds.Height - vb.Margin.Top - vb.Margin.Bottom;
            if(ThemeViewModel.Instance.IsLandscape) {
                outer_w *= 0.5;
            } else {
                outer_h *= 0.3;
            }

            if(inst.Width > outer_w) {
                vb.Width = outer_h * inner_ar;
                vb.Height = vb.Width / inner_ar;
            } else {
                vb.Width = outer_w;
                vb.Height = vb.Width / inner_ar;
            }

            double max_outer_h =
                MainView.Instance.MainContentView.Bounds.Height * (ThemeViewModel.Instance.IsLandscape ? 1 : 0.45d);

            if(vb.Height > max_outer_h) {
                vb.Height = max_outer_h;
                vb.Width = vb.Height * inner_ar;
            }

            if(forceFit) {
                if(vb.Width > outer_w) {
                    vb.Width = outer_w;
                    vb.Height = vb.Width / inner_ar;
                }

                if(vb.Height > outer_h) {
                    double scale = outer_h / vb.Height;
                    vb.Width *= scale;
                    vb.Height *= scale;
                }

                Debug.Assert(vb.Width <= outer_w && vb.Height <= outer_h);
            }

            InvalidateMeasure();
        }
    }
}