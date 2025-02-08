using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class KeyboardView : UserControl {

        public static double WHITE_WIDTH_TO_HEIGHT_RATIO => 5.700677201;
        public static double WHITE_TO_BLACK_WIDTH_RATIO => 0.7;
        public static double WHITE_TO_BLACK_HEIGHT_RATIO => 0.633563;

        public KeyboardView() {
            InitializeComponent();
        }

        public void MeasureKeyboard() {

            double wkw = 100;
            double bkw = wkw * WHITE_TO_BLACK_WIDTH_RATIO;

            double wkh = wkw * WHITE_WIDTH_TO_HEIGHT_RATIO;
            double bkh = wkh * WHITE_TO_BLACK_HEIGHT_RATIO;

            if(DataContext is not TuningViewModel tvm) {
                return;
            }

            double wx = 0;
            for(int i = 0; i < KeyboardItemsControl.ItemCount; i++) {
                if(KeyboardItemsControl.ContainerFromIndex(i) is not { } cp ||
                   cp.DataContext is not NoteViewModel invm) {
                    continue;
                }

                double cur_x = invm.IsAltered ? wx - (bkw / 2d) : wx;

                Canvas.SetLeft(cp,cur_x);
                cp.ZIndex = invm.IsAltered ? 1 : 0;
                cp.Width = invm.IsAltered ? bkw : wkw;
                cp.Height = invm.IsAltered ? bkh : wkh;

                wx = invm.IsAltered ? wx : wx + wkw;
            }

            KeyboardItemsControl.Width = tvm.AllNotes.Count(x => !x.IsAltered) * wkw;
            KeyboardItemsControl.Height = wkh;
            KeyboardItemsControl.InvalidateAll();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            MeasureKeyboard();


        }

        void KeyView_Loaded(object sender,RoutedEventArgs e) {
            if(sender is not Control c ||
               InstrumentView.Instance is not { } iv) {
                return;
            }

            iv.AttachHandlers(c);
        }

        void KeyView_Unloaded(object sender,RoutedEventArgs e) {
            if(sender is not Control c ||
               InstrumentView.Instance is not { } iv) {
                return;
            }

            iv.DetachHandlers(c);
        }
    }
}