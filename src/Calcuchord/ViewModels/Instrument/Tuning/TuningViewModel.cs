using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;
using Material.Styles.Controls;
using MonkeyPaste.Common;

namespace Calcuchord {

    public class TuningViewModel : ViewModelBase<InstrumentViewModel> {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        CancellationTokenSource PatternGenCts { get; set; }

        #endregion

        #region View Models

        public IEnumerable<NoteRowViewModel> PitchSortedRows =>
            NoteRows.OrderBy(x => x.RowNum < 0 ? -1 : x.BaseNote.NoteId);

        public IEnumerable<NoteRowViewModel> SortedRows =>
            NoteRows.OrderBy(x => x.RowNum < 0 ? -1 : x.RowNum);

        public ObservableCollection<NoteRowViewModel> NoteRows { get; } = [];

        public IEnumerable<NoteViewModel> AllNotes =>
            NoteRows.SelectMany(x => x.Notes).OrderBy(x => x.RowNum).ThenBy(x => x.NoteNum);

        public IEnumerable<NoteViewModel> SelectedNotes {
            get => AllNotes.Where(x => x.IsSelected);
            set {
                AllNotes.ForEach(x => x.IsSelected = value == null ? false : value.Contains(x));
                OnPropertyChanged(nameof(SelectedNotes));
            }
        }

        public ObservableCollection<NoteViewModel> OpenNotes { get; } = [];

        public NoteViewModel SelectedOpenNote =>
            OpenNotes.ElementAtOrDefault(SelectedOpenNoteIndex);

        #endregion

        #region Appearance

        public string FullName =>
            $"{Parent.Name} - {Name}";

        public string ProgressTitle =>
            $"Calculating {FullName}...";

        public string GenProgressLabel { get; private set; } = "Preparing...";

        #endregion

        #region Layout

        #endregion

        #region State

        public bool CanDelete =>
            !IsReadOnly; // && Parent.Tunings.Count > 1;

        public bool IsExpanded { get; set; }

        public bool IsCurGenTuning =>
            Parent.CurGenTuning == this;

        public double GenProgress { get; private set; }

        public int BookmarkCount { get; private set; }
        public int ChordsCount { get; private set; }
        public int ScalesCount { get; private set; }
        public int ModesCount { get; private set; }

        bool HasFretNumRow =>
            Parent.InstrumentType != InstrumentType.Piano;

        public int SelectedOpenNoteIndex { get; set; } = 0;

        public bool IsLoaded =>
            Tuning.Chords.Any() &&
            Tuning.Scales.Any() &&
            Tuning.Modes.Any();

        #endregion

        #region Model

        public bool IsSelected {
            get => Tuning.IsSelected;
            set {
                if(IsSelected != value) {
                    Tuning.IsSelected = value;
                    if(IsSelected) {
                        // only trigger save when seleted to avoid a million writes
                        HasModelChanged = true;
                    }

                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public int CapoNum {
            get => Tuning.CapoFretNum;
            set {
                if(CapoNum != value) {
                    Tuning.CapoFretNum = value;
                    OnPropertyChanged(nameof(CapoNum));
                }
            }
        }

        public bool IsPatternEditable =>
            !IsLoaded;

        public bool IsReadOnly => Tuning.IsReadOnly;

        public string Name {
            get => Tuning.Name;
            set {
                if(Name != value) {
                    Tuning.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int WorkingFretCount =>
            Tuning.WorkingColCount;

        public int TotalFretCount =>
            Tuning.Parent.ColCount;

        public int RowCount =>
            Parent.RowCount;

        // +2 for label and nut
        public int LogicalFretCount =>
            TotalFretCount + (Parent.IsKeyboard ? 0 : 2);

        public Tuning Tuning { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public TuningViewModel(InstrumentViewModel parent) : base(parent) {
            PropertyChanged += InstrumentTuningViewModel_OnPropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task<bool> InitAsync(Tuning tuning) {
            bool success = true;

            IsBusy = true;

            Tuning = tuning;
            Tuning.SetParent(Parent.Instrument);

            NoteRows.Clear();

            Tuning.OpenNotes.OrderBy(x => x.RowNum).ForEach(x => NoteRows.Add(new NoteRowViewModel(this,x)));
            if(HasFretNumRow) {
                // add fret num row
                NoteRows.Insert(0,new NoteRowViewModel(this,null));
            }

            OpenNotes.Clear();
            OpenNotes.AddRange(
                NoteRows
                    .Skip(HasFretNumRow ? 1 : 0)
                    .Select(x => x.OpenNote)
                    .OrderBy(x => x.RowNum));

            if(!IsLoaded &&
               (!Parent.IsEditModeEnabled || IsCurGenTuning)) {
                success = await LoadPatternsAsync();
            }

            Parent.Instrument.RefreshModelTree();

            IsBusy = false;
            return success;
        }

        public void ResetSelection() {
            NoteRows.ForEach(x => x.ResetSelection());
        }

        public override string ToString() {
            return Tuning == null ? base.ToString() : Tuning.ToString();
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentTuningViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Name):
                    if(IsSelected) {
                    }

                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        if(Parent.SelectedTuning != this) {
                            Parent.SelectedTuning = this;
                        }

                        ResetSelection();
                        if(!Parent.IsSelected ||
                           Parent.IsEditModeEnabled ||
                           Parent.Parent.LastSelectedTuning == this) {
                            break;
                        }

                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));

                        if(Design.IsDesignMode) {
                            break;
                        }

                        Dispatcher.UIThread.Post(
                            async () => {
                                while(true) {
                                    if(MainView.Instance is { } mv &&
                                       mv.DlgHost is { } mdh &&
                                       mdh.IsLoaded) {
                                        break;
                                    }

                                    // wait until load to prevent showing snackbar on startup
                                    await Task.Delay(100);
                                }

                                string sel_msg = $"{FullName} selected";
                                SnackbarHost.Post(
                                    sel_msg,
                                    MainView.SnackbarHostName,
                                    DispatcherPriority.Background);

                            });
                    }

                    break;
            }
        }

        async Task<bool> LoadPatternsAsync() {
            bool success = true;

            while(true) {
                if(MainView.Instance is not { } mv ||
                   !mv.DlgHost.IsLoaded) {
                    await Task.Delay(100);
                }

                break;
            }

            DialogHost.Show(
                    new TuningGenProgressView { DataContext = this },MainViewModel.Instance.MainDialogHostName)
                .FireAndForgetSafeAsync();

            await Task.Delay(2_000);

            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    PatternGenCts = new CancellationTokenSource();
                    PatternGen pg = new PatternGen(Tuning);
                    pg.ProgressChanged += OnProgressChanged;
                    try {
                        var patterns = await pg.GenerateAsync(PatternGenCts.Token);
                        Tuning.Collections.Keys.ForEach(x => Tuning.Collections[x].AddRange(patterns[x]));

                    } catch(TaskCanceledException) {
                        PlatformWrapper.Services.Logger.WriteLine($"Gen for '{Tuning}' canceled");
                        success = false;

                    }

                    PatternGenCts.Dispose();
                    PatternGenCts = null;
                },DispatcherPriority.Background);

            DialogHost.Close(MainViewModel.Instance.MainDialogHostName);
            return success;


            void OnProgressChanged(object sender,double progress) {
                if(sender is not PatternGen pg) {
                    return;
                }

                // double total_progress = progress / 3d;
                // if(pg.PatternType == MusicPatternType.Scales) {
                //     total_progress += 1 / 3d;
                // } else if(pg.PatternType == MusicPatternType.Chords) {
                //     total_progress += 2 / 3d;
                // }

                Dispatcher.UIThread.Post(
                    () => {
                        GenProgress = progress; //total_progress;
                        GenProgressLabel =
                            $"Generating {pg.PatternType.ToString().ToLower()}...{pg.CurItemCount:n0} found";
                    });
                if(progress >= 1) {
                    pg.ProgressChanged -= OnProgressChanged;
                }
            }
        }


        async Task AdjustCapoAsync(int capoDelta) {
            CapoNum += capoDelta;
            Tuning.OpenNotes.ForEach(x => x.Adjust(capoDelta));
            // var new_open_notes = Tuning.OpenNotes.Select((x,idx) => new InstrumentNote(0,idx,x.Offset(capoDelta)))
            //     .ToList();
            // Tuning.OpenNotes.Clear();
            // Tuning.OpenNotes.AddRange(new_open_notes);
            await InitAsync(Tuning);
        }

        void CloseFlyout(object args) {
            if(args is not Button b ||
               b.Flyout is not { } fo) {
                return;
            }

            // BUG context menu blocks popup and doesn't close 
            // so manually closing
            fo.Hide();
        }

        #endregion

        #region Commands

        public ICommand DeleteThisTuningCommand => new MpCommand<object>(
            async (args) => {
                CloseFlyout(args);

                bool? confirmed = null;
                YesNoDialogView dlg_v = new YesNoDialogView
                {
                    DataContext = new DialogViewModel
                    {
                        Label = $"Are you sure you want to delete '{Name}'?",
                        OkCommand = new MpCommand(
                            () => {
                                confirmed = true;
                            }),
                        CancelCommand = new MpCommand(
                            () => {
                                confirmed = false;
                            })
                    }
                };
                DialogHost.Show(dlg_v,InstrumentEditorView.DialogHostName).FireAndForgetSafeAsync();

                while(!confirmed.HasValue) {
                    await Task.Delay(100);
                }

                DialogHost.Close(InstrumentEditorView.DialogHostName);

                if(!confirmed.Value) {
                    return;
                }

                Parent.RemoveTuningCommand.Execute(this);
            },(args) => {
                return CanDelete;
            });

        public ICommand DuplicateThisTuningCommand => new MpCommand<object>(
            (args) => {
                CloseFlyout(args);
                Tuning dup_tuning = Tuning.Clone();
                dup_tuning.IsReadOnly = false;
                dup_tuning.Name = Parent.GetUniqueTuningName(Name,[]);
                Parent.AddTuningCommand.Execute(dup_tuning);
            });

        public ICommand IncreaseCapoFretCommand => new MpAsyncCommand(
            async () => {
                await AdjustCapoAsync(1);
            },() => {
                return TotalFretCount > Parent.MinEditableFretCount;
            });

        public ICommand DecreaseCapoFretCommand => new MpAsyncCommand(
            async () => {
                await AdjustCapoAsync(-1);
            },() => {
                return CapoNum > 0;
            });


        public ICommand ShowStatsCommand => new MpCommand<object>(
            (args) => {
                CloseFlyout(args);
                ChordsCount = Tuning.Chords.SelectMany(x => x.Patterns).Count();
                ScalesCount = Tuning.Scales.SelectMany(x => x.Patterns).Count();
                ModesCount = Tuning.Modes.SelectMany(x => x.Patterns).Count();

                BookmarkCount =
                    Tuning.Collections.Values.SelectMany(x => x)
                        .SelectMany(x => x.Patterns)
                        .Count(x => x.IsBookmarked);

                TuningStatsView stats_view = new TuningStatsView
                {
                    DataContext = this
                };
                stats_view.OkButton.Command = new MpCommand(
                    () => {
                        DialogHost.Close(InstrumentEditorView.DialogHostName);

                    });

                DialogHost.Show(stats_view,InstrumentEditorView.DialogHostName);

            });

        public ICommand SelectThisTuningCommand => new MpCommand(
            () => {
                Parent.SelectedTuning = this;
            });

        public ICommand CancelPatternGenCommand => new MpCommand(
            () => {
                PatternGenCts?.Cancel();
            });

        #endregion

    }

}