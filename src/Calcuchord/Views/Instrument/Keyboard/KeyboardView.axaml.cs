using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class KeyboardView : UserControl {
        public KeyboardView() {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if(KeyboardItemsControl.GetVisualDescendants<ContentPresenter>() is not { } cpl) {
                return;
            }

            foreach(ContentPresenter cp in cpl) {
                if(cp.DataContext is not KeyViewModel keyViewModel) {
                    continue;
                }

                Canvas.SetLeft(cp,keyViewModel.KeyX);
                cp.ZIndex = keyViewModel.IsAltered ? 10 : 0;
            }
        }
    }
}