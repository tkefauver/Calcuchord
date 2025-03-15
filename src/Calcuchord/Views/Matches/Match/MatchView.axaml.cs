using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchView : UserControl {
        public MatchView() {
            InitializeComponent();

        }

        void MatchContainerBorder_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(DataContext is not MatchViewModel mtvm ||
               Parent is not ContentPresenter presenter) {
                return;
            }

            mtvm.SelectMatchCommand.Execute(null);
            presenter.BringIntoView();
        }
    }
}