using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class FretView : UserControl {
        public NoteViewModel BindingContext =>
            DataContext as NoteViewModel;

        public FretView() {
            InitializeComponent();
        }
    }
}