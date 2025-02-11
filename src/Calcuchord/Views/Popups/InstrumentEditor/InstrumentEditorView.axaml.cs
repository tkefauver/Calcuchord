using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class InstrumentEditorView : UserControl {
        public static string DialogHostName => "InstrumentEditorPopupHost";

        public InstrumentEditorView() {
            InitializeComponent();
        }
    }
}