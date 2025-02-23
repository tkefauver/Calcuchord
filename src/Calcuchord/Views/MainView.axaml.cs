using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public static MainView Instance { get; private set; }
        public static string SnackbarHostName => "Root";

        public MainView() {
            if(Instance != null) {
                // singleton error
                Debugger.Break();
            }

            Instance = this;
            InitializeComponent();

            EffectiveViewportChanged += (sender,args) => OnMainContainerSizeChanged();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            if(MainViewModel.Instance is { } mvm &&
               PlatformWrapper.Services is { } ps &&
               ps.MidiPlayer is { } mp) {
                Dispatcher.UIThread.Post(
                    async () => {
                        // wait for load
                        while(!mvm.IsLoaded) {
                            await Task.Delay(100);
                        }

                        // wait for asset move
                        await Task.Delay(500);

                        // only handled by sugarwv
                        mp.Init(MainContainerGrid);
                    });
            }
        }

        void OnMainContainerSizeChanged() {
            PlatformWrapper.Services.Logger.WriteLine("size changed");
            if(MainViewModel.Instance is not { } mvm ||
               ThemeViewModel.Instance is not { } tvm) {
                return;
            }

            tvm.DoOrientationCheck();

            mvm.SetMatchColumnCount(mvm.MatchColCount);

            InstrumentView.MeasureInstrument();
        }


    }
}