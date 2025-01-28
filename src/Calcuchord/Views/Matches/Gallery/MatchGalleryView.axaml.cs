using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchGalleryView : UserControl {

        public MatchGalleryView() {
            InitializeComponent();
            MatchItemsRepeater.ElementPrepared += MatchItemsRepeaterOnElementPrepared;
        }

        void MatchItemsRepeaterOnElementPrepared(object sender,ItemsRepeaterElementPreparedEventArgs e) {
        }
    }
}