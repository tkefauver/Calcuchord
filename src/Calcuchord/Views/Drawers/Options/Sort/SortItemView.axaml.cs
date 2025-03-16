using System;
using System.Linq;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class SortItemView : UserControl {
        public SortItemView() {
            InitializeComponent();
        }

        void FlyoutBase_OnOpened(object sender,EventArgs e) {
            if(sender is not Flyout flyout ||
               flyout.Content is not Control c ||
               c.GetVisualDescendants<Button>() is not { } bl) {
                return;
            }

            // hide host opt from flyout
            bl.Where(x => x.DataContext == DataContext).ForEach(x => x.IsVisible = false);

            if(!MainViewModel.Instance.IsSearchModeSelected || MainViewModel.Instance.IsExactMatchOnly) {
                // hide score 
            }
        }
    }
}