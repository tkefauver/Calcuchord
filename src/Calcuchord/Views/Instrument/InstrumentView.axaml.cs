using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class InstrumentView : UserControl {
        public static InstrumentView Instance { get; private set; }

        public InstrumentView() {
            Instance = this;
            InitializeComponent();
        }
    }
}