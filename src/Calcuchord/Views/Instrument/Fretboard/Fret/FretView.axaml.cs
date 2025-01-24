using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class FretView : UserControl {
        public FretViewModel BindingContext =>
            DataContext as FretViewModel;

        public FretView() {
            InitializeComponent();
        }
    }
}