using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class FretboardView : UserControl {

        public FretboardView() {
            InitializeComponent();

            this.GetObservable(BoundsProperty).Subscribe(x => MeasureFretboard());
            StringsItemsControl.GetObservable(ItemsControl.ItemsSourceProperty).Subscribe(
                _ => {
                    Dispatcher.UIThread.Post(
                        async () => {
                            await Task.Delay(150);
                            MeasureFretboard();
                        });
                });

            FretboardScrollViewer.PointerWheelChanged += (s,e) => {
                FretboardScrollViewer.ScrollToHorizontalOffset(FretboardScrollViewer.Offset.X - (e.Delta.Y * 50));
            };
            //EffectiveViewportChanged += (s,e) => MeasureFretboard();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            MeasureFretboard();
        }


        public void MeasureFretboard() {
            if(DataContext is not TuningViewModel tvm) {
                return;
            }

            FretboardView outer_cntr = this;

            ItemsControl cntr = StringsItemsControl;
            // guitar
            // tw 1600
            // th 188
            double SCALE = 1.0d;

            double def_fret_w = 69d * SCALE;
            double str_h = 27d * SCALE; //th / tvm.Parent.VisualRowCount;
            double label_width = 30 * SCALE; //Math.Max(30,fret_widths.Max() * 0.25);
            double nut_width = 20 * SCALE; //Math.Min(str_h,dot_d);
            double tw = tvm.TotalFretCount * def_fret_w; //Math.Max(1000,tvm.TotalFretCount * (1600 / 23d));
            double th = tvm.Parent.RowCount * str_h; //tw * (0.117521368 * 1);//(tvm.Parent.RowCount / 6d));

            var fvl = StringsItemsControl.GetVisualDescendants<FretView>();
            if(!fvl.Any()) {
                return;
            }

            double GetDistToNut(int fretNum) {
                double d = tw - (tw / Math.Pow(2,fretNum / 12d));
                return d;
            }

            // +2 for label and nut
            double[] fret_widths = new double[tvm.TotalFretCount + 2];
            double last_len_to_nut = 0;
            for(int i = 0; i < fret_widths.Length; i++) {
                if(i == 0) {
                    fret_widths[i] = label_width;
                } else if(i == 1) {
                    fret_widths[i] = nut_width;
                } else {
                    double l = GetDistToNut(i - 1);
                    fret_widths[i] = l - last_len_to_nut;
                    last_len_to_nut = l;
                }
            }

            double min_fret_w = fret_widths.Skip(2).Min();
            double dot_d = min_fret_w / 2d;

            fvl.ForEach(
                fv => {
                    if(fv.GetVisualAncestor<ContentPresenter>() is not { } cp) {
                        return;
                    }

                    cp.Width = fret_widths[fv.BindingContext.NoteNum + 1];
                    cp.Height = str_h;
                    if(fv.GetVisualDescendants<Ellipse>().Where(x => x.IsVisible && x.Classes.Contains("dot")) is
                       { } dots) {
                        dots.ForEach(x => x.Width = dot_d);
                        if(!fv.BindingContext.IsFullDot) {
                            dots.Select(x => x.RenderTransform).OfType<TranslateTransform>().ForEach(
                                x => x.Y = (dot_d / 2d) * (fv.BindingContext.IsTopDotFret ? 1 : -1));
                        }

                    }
                });

            cntr.Width = fret_widths.Sum();
            cntr.Height = th + str_h;
            double inner_ar = Math.Min(6.44,cntr.Width / cntr.Height);
            if(ThemeViewModel.Instance.IsExpandedLayout) {
                FretboardViewbox.HorizontalAlignment = HorizontalAlignment.Center;
                FretboardViewbox.Stretch = Stretch.UniformToFill;
            } else {
                FretboardViewbox.HorizontalAlignment = HorizontalAlignment.Left;
                FretboardViewbox.Stretch = Stretch.Uniform;
            }

            double pad_w = FretboardScrollViewer.Padding.Left + FretboardScrollViewer.Padding.Right;
            double pad_h = FretboardScrollViewer.Padding.Top + FretboardScrollViewer.Padding.Bottom;
            double outer_w = Math.Max(0,outer_cntr.Bounds.Width - pad_w);
            double outer_h = Math.Max(0,outer_cntr.Bounds.Height - pad_h);

            if(cntr.Width > outer_w) {
                FretboardViewbox.Width = outer_h * inner_ar;
                FretboardViewbox.Height = FretboardViewbox.Width / inner_ar;
            } else {
                FretboardViewbox.Width = outer_w;
                FretboardViewbox.Height = FretboardViewbox.Width / inner_ar;
            }

            if(FretboardViewbox.Height > outer_h) {
                FretboardViewbox.Height = outer_h;
                FretboardViewbox.Width = FretboardViewbox.Height * inner_ar;
            }


        }

        void FretView_Loaded(object sender,RoutedEventArgs e) {
            if(sender is not Control c ||
               InstrumentView.Instance is not { } iv) {
                return;
            }

            iv.AttachHandlers(c);
        }

        void FretView_Unloaded(object sender,RoutedEventArgs e) {
            if(sender is not Control c ||
               InstrumentView.Instance is not { } iv) {
                return;
            }

            iv.DetachHandlers(c);
        }
    }
}