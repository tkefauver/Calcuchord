using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
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


        void MeasureFretboard() {
            if(DataContext is not TuningViewModel tvm) {
                return;
            }

            ItemsControl cntr = StringsItemsControl;

            double min_width = tvm.TotalFretCount * (1600 / 23d);
            double tw = min_width;
            double th = tw * (0.117521368 * 1);

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

            double nut_width = fret_widths.Min();
            double label_width = fret_widths.Max() * 0.25;
            double str_h = th / tvm.Parent.LogicalStringCount;

            fvl.ForEach(
                x => {
                    if(x.GetVisualAncestor<ContentPresenter>() is not { } cp) {
                        return;
                    }

                    int fn = x.BindingContext.NoteNum;
                    cp.Width =
                        fn < 0 ?
                            label_width :
                            fn == 0 ?
                                nut_width :
                                fn <= fret_widths.Length ?
                                    fret_widths[fn - 1] :
                                    0;
                    cp.Height = str_h;
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