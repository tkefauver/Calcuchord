using Avalonia.Controls;
using Avalonia.Input;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchView : UserControl {
        public MatchView() {
            InitializeComponent();

        }

        void MatchContainerBorder_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(DataContext is not MatchViewModel mtvm) {
                return;
            }

            mtvm.SelectMatchCommand.Execute(null);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            if(DataContext is not MatchViewModel mtvm) {
                return;
            }

            mtvm.SelectMatchCommand.Execute(null);
            this.BringIntoView();
        }
    }
}