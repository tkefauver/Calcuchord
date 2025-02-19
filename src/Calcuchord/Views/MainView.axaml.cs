using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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


            MainContainerGrid.GetObservable(BoundsProperty).Subscribe(value => OnMainContainerSizeChanged());
            EffectiveViewportChanged += (sender,args) => OnMainContainerSizeChanged();

            ThemeViewModel.Instance.OrientationChanged += (s,e) => RefreshMainGrid();
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

            RefreshMainGrid();
        }

        void OnMainContainerSizeChanged() {
            PlatformWrapper.Services.Logger.WriteLine("size changed");
            if(MainViewModel.Instance is not { } mvm ||
               ThemeViewModel.Instance is not { } tvm) {
                return;
            }

            if(IsLoaded) {
                //RefreshMainGrid();
            }

            mvm.SetMatchColumnCount(mvm.MatchColCount);

            InstrumentView.MeasureInstrument();
        }


        public void RefreshMainGrid() {
            try {
                if(ThemeViewModel.Instance is not { } tvm ||
                   MainViewModel.Instance is not { } mvm) {
                    return;
                }

                double splitter_len = 4;
                MainContainerGrid.ColumnDefinitions.Clear();
                MainContainerGrid.RowDefinitions.Clear();

                double tw = MainContainerGrid.Bounds.Width;
                double th = MainContainerGrid.Bounds.Height;
                if(tvm.IsLandscape) {
                    double inst_len = mvm.IsInstrumentVisible ? tw * 0.5d : 0;
                    double matches_len = tw - inst_len;
                    MainContainerGrid.ColumnDefinitions.Add(
                        new ColumnDefinition(new GridLength(inst_len,GridUnitType.Auto)));
                    MainContainerGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                    MainContainerGrid.ColumnDefinitions.Add(
                        new ColumnDefinition(new GridLength(matches_len,GridUnitType.Star)));

                    Grid.SetRowSpan(MainBgImage,1);
                    Grid.SetColumnSpan(MainBgImage,3);

                    Grid.SetRow(InstrumentView,0);
                    Grid.SetColumn(InstrumentView,0);
                    InstrumentView.Width = inst_len;
                    InstrumentView.Height = double.NaN;

                    Grid.SetRow(MainSplitter,0);
                    Grid.SetColumn(MainSplitter,1);

                    Grid.SetRow(MatchesView,0);
                    Grid.SetColumn(MatchesView,2);
                    MatchesView.Width = matches_len;
                    MatchesView.Height = double.NaN;

                    MainSplitter.ResizeDirection = GridResizeDirection.Columns;
                    MainSplitter.Height = double.NaN;
                    MainSplitter.Width = splitter_len;
                    MainSplitter.Cursor = new Cursor(StandardCursorType.SizeWestEast);
                } else {
                    double inst_len = mvm.IsInstrumentVisible
                        ? Math.Max(210,MainContainerGrid.Bounds.Height * 0.35d)
                        : 0;
                    double matches_len = th - inst_len;
                    MainContainerGrid.RowDefinitions.Add(new RowDefinition(new GridLength(inst_len,GridUnitType.Auto)));
                    MainContainerGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    MainContainerGrid.RowDefinitions.Add(
                        new RowDefinition(new GridLength(matches_len,GridUnitType.Star)));

                    Grid.SetRowSpan(MainBgImage,3);
                    Grid.SetColumnSpan(MainBgImage,1);

                    Grid.SetRow(InstrumentView,0);
                    Grid.SetColumn(InstrumentView,0);
                    InstrumentView.Width = double.NaN;
                    InstrumentView.Height = inst_len;

                    Grid.SetRow(MainSplitter,1);
                    Grid.SetColumn(MainSplitter,0);

                    Grid.SetRow(MatchesView,2);
                    Grid.SetColumn(MatchesView,0);
                    MatchesView.Width = double.NaN;
                    MatchesView.Height = matches_len;

                    MainSplitter.ResizeDirection = GridResizeDirection.Rows;
                    MainSplitter.Height = splitter_len;
                    MainSplitter.Width = double.NaN;
                    MainSplitter.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);

                }

                MainContainerGrid.InvalidateAll();
            } catch(Exception ex) {
                ex.Dump();
            }
        }

        void UpdateGridDefinitions() {
            if(ThemeViewModel.Instance is not { } tvm) {
                return;
            }

            if(tvm.IsLandscape) {

            }
        }
    }
}