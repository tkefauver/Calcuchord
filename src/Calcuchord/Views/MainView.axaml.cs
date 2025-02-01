using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using AvaloniaWebView;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public static MainView Instance { get; private set; }

        public MainView() {
            if(Instance != null) {
                // singleton error
                Debugger.Break();
            }

            Instance = this;
            InitializeComponent();

            MainContainerGrid.GetObservable(BoundsProperty).Subscribe(value => OnMainContainerSizeChanged());
            EffectiveViewportChanged += (sender,args) => OnMainContainerSizeChanged();

            if(PlatformWrapper.WebViewHelper is { } wvh &&
               wvh.IsSupported) {
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
            }
        }

        void OnMainContainerSizeChanged() {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            mvm.DiscoverMatchColumnCount();
        }
    }
}