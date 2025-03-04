using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class DrawerHostView : UserControl {
        public DrawerHostView() {
            InitializeComponent();
        }

        public void Next() {
            DrawerCarousel.Next();
        }

        public void Previous() {
            DrawerCarousel.Previous();
        }
    }
}