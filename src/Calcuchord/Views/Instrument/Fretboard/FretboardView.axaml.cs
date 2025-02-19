using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
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

            ItemsControl cntr = StringsItemsControl;
// guitar
// tw 1600
// th 188
            double def_fret_w = 69d;
            double def_fret_h = 31d;
            double tw = tvm.TotalFretCount * def_fret_w; //Math.Max(1000,tvm.TotalFretCount * (1600 / 23d));
            double th = tvm.Parent.RowCount * def_fret_h; //tw * (0.117521368 * 1);//(tvm.Parent.RowCount / 6d));

            var fvl = StringsItemsControl.GetVisualDescendants<FretView>();
            if(!fvl.Any()) {
                return;
            }

            double GetDistToNut(int fretNum) {
                double d = tw - (tw / Math.Pow(2,fretNum / 12d));
                return d;
            }

            double[] fret_widths = new double[tvm.TotalFretCount];
            double ll = 0;
            for(int i = 1; i <= fret_widths.Length; i++) {
                double l = GetDistToNut(i);
                fret_widths[i - 1] = l - ll;
                ll = l;
            }

            double label_width = 30; //Math.Max(30,fret_widths.Max() * 0.25);
            double str_h = th / tvm.Parent.VisualRowCount;
            double dot_d = Math.Min((tvm.Parent.RowCount * 3) + 2,fret_widths.Min());
            double nut_width = Math.Min(str_h,dot_d);

            fvl.ForEach(
                fv => {
                    if(fv.GetVisualAncestor<ContentPresenter>() is not { } cp) {
                        return;
                    }

                    int fn = fv.BindingContext.NoteNum;
                    cp.Width =
                        fn < 0 ?
                            label_width :
                            fn == 0 ?
                                nut_width :
                                fn <= fret_widths.Length ?
                                    fret_widths[fn - 1] :
                                    0;
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
            double frets_width = fret_widths.Sum();

            cntr.Width = frets_width + nut_width + label_width;
            cntr.Height = th;

            double lt = label_width + nut_width;
            double tt = str_h;
            FretboardBgImage.Width = frets_width;
            FretboardBgImage.Height = th - str_h;
            FretboardBgImage.Margin = new(lt,tt,0,0);
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