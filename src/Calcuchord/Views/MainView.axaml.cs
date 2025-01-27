using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public static MainView Instance { get; private set; }

        public MainView() {
            Instance = this;
            InitializeComponent();
            //MidiPlayer.Instance.Init(HiddenWebview);
        }
    }
}