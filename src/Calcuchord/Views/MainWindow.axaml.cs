using System.Threading.Tasks;
using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        async void WindowBase_OnResized(object sender,WindowResizedEventArgs e) {
            if(MainView.Instance is not { } mv) {
                return;
            }

            await Task.Delay(1_000);
            mv.RefreshMainGrid();
        }
    }
}