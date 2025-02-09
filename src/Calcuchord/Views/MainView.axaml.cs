using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PropertyChanged;
#if SUGAR_WV
using AvaloniaWebView;
#endif

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public static MainView Instance { get; private set; }
        public static string DialogHostName => "MainDialogHost";

        public MainView() {
            if(Instance != null) {
                // singleton error
                Debugger.Break();
            }

            Instance = this;
            InitializeComponent();

            MainContainerGrid.GetObservable(BoundsProperty).Subscribe(value => OnMainContainerSizeChanged());
            EffectiveViewportChanged += (sender,args) => OnMainContainerSizeChanged();

        }

        protected override void OnLoaded(RoutedEventArgs e) {
            if(PlatformWrapper.Services is { } ps &&
               ps.MidiPlayer is { } mp) {
                // only handled by sugarwv
                mp.Init(MainContainerGrid);
            }
        }

        void OnMainContainerSizeChanged() {
            if(MainViewModel.Instance is not { } mvm ||
               ThemeViewModel.Instance is not { } tvm) {
                return;
            }

            tvm.OnPropertyChanged(nameof(tvm.IsLandscape));
            tvm.OnPropertyChanged(nameof(tvm.Orientation));

            mvm.DiscoverMatchColumnCount();
            InstrumentView.MeasureInstrument();
        }
    }
}