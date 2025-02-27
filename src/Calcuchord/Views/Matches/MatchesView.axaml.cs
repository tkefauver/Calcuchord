using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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

        public void ScrollItemIntoView(MatchViewModel mtvm) {
            if(this.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == mtvm) is not { } mv) {
                return;
            }

            mv.BringIntoView();
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

        void EmptyTextCntrContentControl_OnLoaded(object sender,RoutedEventArgs e) {
            if(sender is not Control c) {
                return;
            }

            void OnVisChanged() {
                if(c.IsVisible && c.Classes.Contains("index-mode")) {

                }
            }

            c.GetObservable(IsVisibleProperty).Subscribe(value => OnVisChanged());
        }
    }
}