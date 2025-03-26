using Avalonia.Controls;
using Avalonia.Interactivity;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class EditBookmarkGroupView : UserControl {
        public EditBookmarkGroupView() {
            InitializeComponent();
        }

        void Button_OnClick(object sender,RoutedEventArgs e) {
            if(sender is Button b &&
               b.Command is { } cmd) {
                cmd.Execute(b.CommandParameter);
            }

            Extensions.CloseFlyout(this.ColorButton);
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            GroupNameTextBox.Focus();
            GroupNameTextBox.SelectAll();
        }
    }
}