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
            if(PlatformWrapper.WebViewHelper is not { } wvh ||
               !wvh.IsSupported) {
                return;
            }

#if SUGAR_WV
           WebView hwv = new WebView
                {
                    Width = 0,
                    Height = 0,
                    IsVisible = false
                };
                hwv.Loaded += (sender,args) => {
                    wvh.ConfigureWebView(hwv);
                };
                MainContainerGrid.Children.Insert(0,hwv);
#else
            wvh.ConfigureWebView(MainContainerGrid);
#endif
        }

        void OnMainContainerSizeChanged() {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            mvm.DiscoverMatchColumnCount();
        }
    }
}