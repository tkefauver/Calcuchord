using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               mvm.Matches is not { } matches) {
            }

            //matches.CollectionChanged += MatchesOnCollectionChanged;
            //MatchItemsRepeater.ElementClearing += (s,e1) => DoBusyCheck();
            //MatchItemsRepeater.ElementPrepared += (s,e2) => DoBusyCheck();
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