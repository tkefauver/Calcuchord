using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
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

            EffectiveViewportChanged += (_,_) => OnMainContainerSizeChanged();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            // BUG right drawer opens automatically when threshold width == 0
            mvm.IsRightDrawerOpen = false;

            if(PlatformWrapper.Services is { } ps &&
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

            //mvm.SetMatchColumnCount(mvm.MatchColCount);

            InstrumentView.MeasureInstrument();
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               mvm.EditModeInstrument is not null) {
                return;
            }

            if(e.Key == Key.OemMinus && e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                mvm.IncreaseMatchColumnsCommand.Execute(null);
                return;
            }

            if(e.Key == Key.OemPlus && e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                mvm.DecreaseMatchColumnsCommand.Execute(null);
                return;
            }

            if(MatchesView.Instance is not { } mv ||
               mv.MatchItemsRepeater.GetVisualDescendants<MatchView>()
                   .FirstOrDefault(x => x.IsKeyboardFocusWithin) is null) {
                return;
            }

            if(CheckAndDoArrowNav(e)) {
                return;
            }

            if(mvm.SelectedMatch is not { } sel_mtvm) {
                return;
            }

            if(e.Key == Key.Space || e.Key == Key.Enter) {
                sel_mtvm.ToggleMatchPlaybackCommand.Execute(sel_mtvm);
                return;
            }

            if(e.Key == Key.D && e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                sel_mtvm.ToggleBookmarkCommand.Execute(null);
                return;
            }

            if(e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                sel_mtvm.SetMatchToInstrumentCommand.Execute(null);
            }

        }

        bool CheckAndDoArrowNav(KeyEventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               mvm.Matches.None()) {
                return false;
            }

            int dx = 0;
            int dy = 0;
            if(e.Key == Key.Left) {
                dx = -1;
            } else if(e.Key == Key.Right) {
                dx = 1;
            } else if(e.Key == Key.Up) {
                dy = -1;
            } else if(e.Key == Key.Down) {
                dy = 1;
            }

            if(dx == 0 && dy == 0) {
                return false;
            }

            if(mvm.SelectedMatch is not { } sel_mtvm) {
                // sel first by default regardless of arrow
                if(mvm.Matches.FirstOrDefault() is { } first_mtvm) {
                    first_mtvm.SelectMatchCommand.Execute(null);
                    return true;
                }

                return false;
            }

            int cc = MatchesView.Instance.GetVisualColCount();

            int sel_idx = mvm.Matches.IndexOf(sel_mtvm);
            int sel_row = sel_idx / cc;
            int sel_col = sel_idx % cc;
            //PlatformWrapper.Services.Logger.WriteLine($"'{sel_mtvm.NotePattern}' idx: {sel_idx} r: {sel_row} c: {sel_col}");

            int new_sel_col = sel_col + dx;
            int new_sel_row = sel_row + dy;
            if(new_sel_col < 0 || new_sel_row < 0) {
                return false;
            }

            int new_sel_idx = (cc * new_sel_row) + new_sel_col;
            if(new_sel_idx >= mvm.Matches.Count ||
               mvm.Matches.ElementAt(new_sel_idx) is not { } to_sel_mtvm) {
                return false;
            }

            to_sel_mtvm.SelectMatchCommand.Execute(null);


            return true;
        }


    }
}