using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchesView : UserControl {
        public static MatchesView Instance { get; private set; }

        public MatchesView() {
            if(Instance != null) {
                // singleton error
                Debugger.Break();
            }

            Instance = this;
            InitializeComponent();

            MatchItemsRepeater.Loaded += async (sender,args) => {
                if(MainViewModel.Instance is not { } mvm) {
                    return;
                }

                await mvm.CancelMatchZoomAsync();
                mvm.SetMatchColumnCountAsync(mvm.MatchColCount,mvm.ZoomCts.Token)
                    .FireAndForgetSafeAsync();

            };
        }

        void MatchesOnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            DoBusyCheck();
        }

        void DoBusyCheck() {
            if(MatchesBusyOverlay.IsVisible) {
                return;
            }

            MatchesBusyOverlay.IsVisible = true;
            Dispatcher.UIThread.Post(
                async () => {
                    await Task.Delay(100);
                    while(!MatchItemsRepeater.IsArrangeValid) {
                        await Task.Delay(100);
                    }

                    MatchesBusyOverlay.IsVisible = false;


                });
        }
    }
}