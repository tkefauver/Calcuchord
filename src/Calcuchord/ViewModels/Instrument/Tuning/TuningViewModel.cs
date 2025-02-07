using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Calcuchord.JsonChords;
using DialogHostAvalonia;
using Material.Styles.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;

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

        public string GenProgressLabel { get; private set; } = "Test label text";

        #endregion

        #region Layout

        #endregion

        #region State

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

        public bool IsSelected {
            get => Parent.SelectedTuning == this;
            set {
                Parent.SelectedTuning = value ? this : null;
                OnPropertyChanged(nameof(IsSelected));
            }

        }

        #endregion

        #region Instrument

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

        public string Id =>
            Tuning.Id;

        public int WorkingFretCount =>
            Tuning.WorkingFretCount;

        public int TotalFretCount =>
            Tuning.Parent.FretCount;

        int StringCount =>
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
                        ResetSelection();
                        Prefs.Instance.SelectedTuningId = Id;
                        Prefs.Instance.Save();
                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));

                        Dispatcher.UIThread.Post(
                            async () => {
                                while(!MainView.Instance.IsLoaded) {
                                    // wait until load to prevent showing snackbar on startup
                                    await Task.Delay(100);
                                }

                                string sel_msg = $"{FullName} selected";
                                SnackbarHost.Post(
                                    sel_msg,
                                    null,
                                    DispatcherPriority.Background);

                            });
                    }

                    break;
            }
        }

        async Task<bool> LoadPatternsAsync() {
            if(Prefs.Instance.Instruments.SelectMany(x => x.Tunings).FirstOrDefault(x => x.Id == Id) is not
               { } tuning_model) {
                if(!Parent.IsEditModeEnabled && Parent.IsActivated) {
                    // error
                    Debugger.Break();
                }

                return false;
            }

            PatternGenCts = new CancellationTokenSource();

            var progress_lookup = new Dictionary<MusicPatternType,double>
            {
                { MusicPatternType.Chords,0 },
                { MusicPatternType.Scales,0 },
                { MusicPatternType.Modes,0 }
            };
            var tasks = new List<Func<Task>>
            {
                CreateScalesAndModesAsync,
                CreateChordsAsync
            };

            bool is_done = false;

            _ = Task.Run(
                async () => {
                    await Task.WhenAll(tasks.Select(task => task()));
                    tuning_model.Chords = Tuning.Chords;
                    tuning_model.Scales = Tuning.Scales;
                    tuning_model.Modes = Tuning.Modes;
                    is_done = true;
                },PatternGenCts.Token);
            while(true) {
                if(is_done) {
                    break;
                }

                if(PatternGenCts.Token.IsCancellationRequested) {
                    break;
                }

                await Task.Delay(100);
            }

            return is_done;


            void OnProgressChanged(object sender,EventArgs e) {
                if(sender is not PatternGen pg) {
                    return;
                }

                progress_lookup[pg.PatternType] = pg.PercentDone;

                Dispatcher.UIThread.Post(
                    () => {
                        GenProgress =
                            progress_lookup
                                [MusicPatternType.Chords]; //progress_lookup.Values.Sum() / progress_lookup.Count;
                        if(pg.PatternType == MusicPatternType.Chords) {
                            GenProgressLabel = $"{pg.TotalChordCount:n0} chords found"; //pg.ProgressLabel;    
                        }


                        Debug.WriteLine($"Total Progress: {GenProgress} Label: '{pg.ProgressLabel}'");
                    });
            }

            async Task CreateChordsAsync() {
                bool from_file =
                    Tuning.Id is
                        Instrument.STANDARD_GUITAR_TUNING_ID or
                        Instrument.STANDARD_UKULELE_TUNING_ID;
                //from_file = false;
                IEnumerable<NoteGroupCollection> chords = null;
                if(from_file) {
                    chords = GenFromFile(Tuning);
                } else {
                    chords = await GenFromPatternsAsync(Tuning);
                }

                Tuning.Chords.AddRange(chords);
                return;

                async Task<IEnumerable<NoteGroupCollection>> GenFromPatternsAsync(Tuning tuning) {
                    PatternGen pg = new PatternGen(MusicPatternType.Chords,tuning);
                    pg.ProgressChanged += OnProgressChanged;
                    var result = await pg.GenerateAsync(PatternGenCts.Token);
                    return result;
                }

                IEnumerable<NoteGroupCollection> GenFromFile(Tuning tuning) {
                    string json = MpAvFileIo.ReadTextFromResource(
                        $"avares://Calcuchord/Assets/Text/{tuning.Parent.InstrumentType.ToString().ToLower()}.json");
                    ChordsJsonRoot chordsJsonRoot = JsonConvert.DeserializeObject<ChordsJsonRoot>(json);
                    var ngcl = new List<NoteGroupCollection>();

                    foreach(PropertyInfo pi in typeof(Chords).GetProperties()) {
                        object obj = chordsJsonRoot.chords.GetPropertyValue(pi.Name);
                        if(obj is not IList keys_obj ||
                           keys_obj.OfType<MusicKey>().FirstOrDefault() is not { } key_obj ||
                           MusicHelpers.ParseNote(key_obj.key) is not { } key_note_tup) {
                            continue;
                        }

                        NoteType cur_key = key_note_tup.nt;

                        foreach(MusicKey chord_group in keys_obj) {
                            string cur_suffix = chord_group.suffix;
                            NoteGroupCollection ngc = new NoteGroupCollection(
                                MusicPatternType.Chords,cur_key,chord_group.suffix);
                            foreach((Position pos,int pos_num) in chord_group.positions.WithIndex()) {
                                NoteGroup ng = new NoteGroup(ngc,pos_num);
                                ng.CreateId(null);
                                foreach((int f,int str_num) in pos.real_frets.WithIndex()) {
                                    InstrumentNote inn = null;
                                    if(f < 0) {
                                        inn = InstrumentNote.Mute(str_num);
                                    } else if(tuning.OpenNotes[str_num].Offset(f) is { } fret_note) {
                                        inn = new InstrumentNote(f,str_num,fret_note);
                                    }

                                    Debug.Assert(inn != null,"Parse error");
                                    PatternNote pattern_note = new PatternNote(pos.fingers[str_num],inn);
                                    ng.Notes.Add(pattern_note);
                                }

                                ngc.Groups.Add(ng);
                            }

                            ngcl.Add(ngc);
                        }
                    }

                    return ngcl;
                }
            }

            async Task CreateScalesAndModesAsync() {
                PatternGen pg1 = new PatternGen(MusicPatternType.Scales,Tuning);
                pg1.ProgressChanged += OnProgressChanged;
                var scales = await pg1.GenerateAsync(PatternGenCts.Token);
                Tuning.Scales.AddRange(scales);
                Debug.WriteLine(
                    $"{scales.SelectMany(x => x.Groups).Count()} scales generated for {Tuning}");

                PatternGen pg2 = new PatternGen(MusicPatternType.Modes,Tuning);
                pg2.ProgressChanged += OnProgressChanged;
                var modes = await pg2.GenerateAsync(PatternGenCts.Token);
                Tuning.Modes.AddRange(modes);
                Debug.WriteLine(
                    $"{modes.SelectMany(x => x.Groups).Count()} modes generated for {Tuning}");
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

        public bool CanDelete =>
            !IsReadOnly && Parent.Tunings.Count > 1;

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
                DialogHost.Show(dlg_v,MainView.DialogHostName).FireAndForgetSafeAsync();

                while(!confirmed.HasValue) {
                    await Task.Delay(100);
                }

                DialogHost.Close(MainView.DialogHostName);

                if(!confirmed.Value) {
                    return;
                }

                Parent.RemoveTuningCommand.Execute(Id);
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
                ChordsCount = Tuning.Chords.SelectMany(x => x.Groups).Count();
                ScalesCount = Tuning.Scales.SelectMany(x => x.Groups).Count();
                ModesCount = Tuning.Modes.SelectMany(x => x.Groups).Count();

                BookmarkCount =
                    Tuning.Collections.Values.SelectMany(x => x)
                        .SelectMany(x => x.Groups)
                        .Count(x => Prefs.Instance.BookmarkIds.Contains(x.Id));

                TuningStatsView stats_view = new TuningStatsView
                {
                    DataContext = this
                };

                DialogHost.Show(stats_view,MainView.DialogHostName);

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