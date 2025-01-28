using Avalonia.Controls;
using Avalonia.Input;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchView : UserControl {
        public MatchView() {
            InitializeComponent();

        }

        void InitSvg() {
            // SKSvg Svg = new SKSvg();
            // CustomTypefaceProvider test = new CustomTypefaceProvider(@"C:\Fonts\Pacifico.ttf");
            // Svg.Settings.TypefaceProviders.Insert(0,test);
            // Svg.Load(@"c:\TEMP\sample.svg");
            // canvas.DrawPicture(Svg.Picture,ref matrix);
        }

        void MatchContainerBorder_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(DataContext is not MatchViewModelBase mtvm) {
                return;
            }

            mtvm.SelectMatchCommand.Execute(null);
        }
    }
}