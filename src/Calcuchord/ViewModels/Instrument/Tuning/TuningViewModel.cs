using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

    public partial class TuningViewModel : ViewModelBase<InstrumentViewModel> {

        #region Private Variables

        #endregion

        #region Constants

        const string DEFAULT_BOOKMARK_GROUP_NAME = "Group1";

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
            NoteRows.OrderBy(
                x => x.RowNum < 0 ?
                    -1 :
                    x.BaseNote.NoteId);

        public IEnumerable<NoteRowViewModel> SortedRows =>
            NoteRows.OrderBy(
                x => x.RowNum < 0 ?
                    -1 :
                    x.RowNum);

        public ObservableCollection<BookmarkGroupViewModel> SelectedBookmarkGroups { get; set; } = [];
        public ObservableCollection<BookmarkGroupViewModel> BookmarkGroups { get; } = [];
        public ObservableCollection<BookmarkGroupViewModel> BoundBookmarkGroups { get; private set; } = [];

        public IReadOnlyList<BookmarkGroupViewModel> AvailableBookmarkGroups { get; private set; } = [];

        public BookmarkGroupViewModel DefaultBookmarkGroup =>
            AvailableBookmarkGroups.FirstOrDefault(x => x.IsDefault);

        public BookmarkGroupViewModel AddNewPlaceholderGroup { get; private set; }


        public ObservableCollection<NoteRowViewModel> NoteRows { get; } = [];

        public IEnumerable<NoteViewModel> AllNotes =>
            NoteRows.SelectMany(x => x.Notes).OrderBy(x => x.RowNum).ThenBy(x => x.NoteNum);

        public IEnumerable<NoteViewModel> SelectedNotes {
            get => AllNotes.Where(x => x.IsSelected);
            set {
                AllNotes.ForEach(
                    x => x.IsSelected = value == null ?
                        false :
                        value.Contains(x));
                OnPropertyChanged();
            }
        }

        public ObservableCollection<NoteViewModel> OpenNotes { get; } = [];

        public NoteViewModel SelectedOpenNote =>
            OpenNotes.ElementAtOrDefault(SelectedOpenNoteIndex);

        #endregion

        #region Appearance

        public string FullName =>
            Tuning.FullName;

        #endregion

        #region Layout

        public int BookmarkColumnCount =>
            BoundBookmarkGroups.Count <= 3 ?
                2 : 3;

        #endregion

        #region State

        public bool IsFretless =>
            Parent.IsFretless;

        public MusicPatternType LastNotePatternType { get; set; }

        public bool CanDelete =>
            !IsReadOnly; // && Parent.Tunings.Count > 1;

        public bool IsExpanded { get; set; }

        public bool IsCurGenTuning =>
            Parent.CurGenTuning == this;


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
                        //HasModelChanged = true;
                        Prefs.Instance.Save();
                    }

                    OnPropertyChanged();
                }
            }
        }

        public int CapoNum {
            get => Tuning.CapoFretNum;
            set {
                if(CapoNum != value) {
                    Tuning.CapoFretNum = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
            TotalFretCount +
            (Parent.IsKeyboard ?
                0 :
                2);

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
            Tuning = tuning;
            Tuning.SetParent(Parent.Instrument);

            if(string.IsNullOrEmpty(Tuning.Id)) {
                Tuning.CreateId();
            }

            NoteRows.Clear();

            Tuning.OpenNotes.OrderBy(x => x.RowNum).ForEach(x => NoteRows.Add(new NoteRowViewModel(this,x)));
            if(HasFretNumRow) {
                // add fret num row
                NoteRows.Insert(0,new NoteRowViewModel(this,null));
            }

            OpenNotes.Clear();
            OpenNotes.AddRange(
                NoteRows
                    .Skip(
                        HasFretNumRow ?
                            1 :
                            0)
                    .Select(x => x.OpenNote)
                    .OrderBy(x => x.RowNum));

            BookmarkGroups.Clear();
            BookmarkGroups.AddRange(
                Prefs.Instance.BookmarkGroups
                    .Where(x => x.IsTuningBookmark(Tuning))
                    .Select(x => new BookmarkGroupViewModel(this,x)));
            if(BookmarkGroups.None()) {
                // initial tuning init, add default bookmark groups
                BookmarkGroups.AddRange(
                    Enumerable.Range(0,Enum.GetNames(typeof(MusicPatternType)).Length)
                        .Select(
                            x => new BookmarkGroupViewModel(
                                this,
                                BookmarkGroup.Create(
                                    Tuning,(MusicPatternType)x,DEFAULT_BOOKMARK_GROUP_NAME,isDefault: true))));
                if(Tuning.Collections
                           .SelectMany(x => x.Value).SelectMany(x => x.Patterns).Where(x => x.IsBookmarked) is
                       { } legacy_bookmarks &&
                   legacy_bookmarks.Any()) {
                    // add legacy bookmarks to default groups
                    foreach(NotePattern lb in legacy_bookmarks) {
                        if(BookmarkGroups.FirstOrDefault(x => x.PatternType == lb.PatternType) is { } def_pt_bmgvm) {
                            def_pt_bmgvm.ToggleLinkedWithPatternCommand.Execute(lb);
                        }
                    }

                    Prefs.Instance.Save();
                }
            }

            AddNewPlaceholderGroup = new BookmarkGroupViewModel(this,null);
            BookmarkGroups.Add(AddNewPlaceholderGroup);
            BookmarkGroups.CollectionChanged += BookmarkGroups_CollectionChanged;
            SelectedBookmarkGroups.CollectionChanged += SelectedBookmarkGroups_CollectionChanged;


            if(!IsLoaded &&
               (!Parent.IsEditModeEnabled || IsCurGenTuning)) {
                success = await LoadPatternsAsync();
            }

            Parent.Instrument.RefreshModelTree();

            return success;
        }

        public void ResetSelection() {
            NoteRows.ForEach(x => x.ResetSelection());
        }

        public IEnumerable<MatchViewModel> GetMatchResults(
            InstrumentNote[] sel_notes,
            MatchScoreMethodType score_method,
            DisplayModeType mode_type,
            MusicPatternType pattern_type,
            NoteType? target_key,
            IEnumerable<string> target_suffixes,
            out string[] keyl,
            out string[] suffl) {

            List<NoteType> avail_keys = [];
            List<string> avail_suffixes = [];
            var mrl = new List<MatchViewModel>();
            foreach(PatternKeyCollection pkc in Tuning.Collections[pattern_type]) {
                bool omitted_key = target_key is { } tk2 && tk2 != pkc.Key;

                foreach(NotePattern np in pkc.Patterns) {
                    bool omitted_suff = target_suffixes.Any() && !target_suffixes.Contains(np.SuffixKey);
                    bool valid = false;
                    double score = 1;
                    if(mode_type == DisplayModeType.Search) {
                        score = GetScore(np,sel_notes,score_method);
                        if(score > 0 || sel_notes.Length == 0) {
                            valid = true;
                        }
                    } else if(mode_type == DisplayModeType.Bookmarks) {
                        if(SelectedBookmarkGroups.Any(x => np.IsInBookmarkGroup(x.BookmarkGroup))) {
                            valid = true;
                        }
                    } else {
                        valid = true;
                    }

                    if(!valid) {
                        continue;
                    }

                    if(!avail_keys.Contains(np.Key)) {
                        avail_keys.Add(np.Key);
                    }

                    if(!avail_suffixes.Contains(np.SuffixKey)) {
                        avail_suffixes.Add(np.SuffixKey);
                    }

                    if(!omitted_key &&
                       !omitted_suff &&
                       (mode_type != DisplayModeType.Search || score > 0)) {
                        //yield return mvm;
                        mrl.Add(new MatchViewModel(pattern_type,np,score));
                    }
                }
            }

            keyl = avail_keys.Select(x => x.ToString()).ToArray();
            suffl = avail_suffixes.ToArray();
            return mrl;
        }

        public void UpdateAvailableSortOrder(bool byModel) {
            if(byModel) {
                BoundBookmarkGroups = new ObservableCollection<BookmarkGroupViewModel>(
                    BookmarkGroups
                        .Where(x => x.PatternType == MainViewModel.Instance.SelectedPatternType)
                        .OrderBy(x => x.SortOrderIdx));
            } else {
                BoundBookmarkGroups.ForEach(
                    (x,idx) => x.SortOrderIdx = x.IsAddGroupPlaceholder ? int.MaxValue : idx);
                OnPropertyChanged(nameof(BoundBookmarkGroups));
            }

            AvailableBookmarkGroups = BoundBookmarkGroups.SkipLast(1).ToList();
        }

        public int GetNewBookmarkColorId() {
            return Enumerable.Range(0,ThemeViewModel.Instance.BookmarkColors.Length)
                .Select(
                    x =>
                        (x,AvailableBookmarkGroups.Count(y => y.BookmarkGroup.ColorId == x)))
                .OrderBy(x => x.Item2)
                .Select(x => x.Item1)
                .FirstOrDefault();
        }

        public override string ToString() {
            return Tuning == null ?
                base.ToString() :
                Tuning.ToString();
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentTuningViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Name):
                    if(IsSelected) {
                        Parent.RaisePropertyChanged(nameof(Parent.Subtitle));
                    }

                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        Dispatcher.UIThread.Invoke(
                            () => {
                                if(Parent.SelectedTuning != this) {
                                    Parent.SelectedTuning = this;
                                }

                                //ResetSelection();
                                if(!Parent.IsSelected ||
                                   Parent.IsEditModeEnabled ||
                                   Parent.Parent.LastSelectedTuning == this) {
                                    return;
                                }

                                MainViewModel.Instance.RaisePropertyChanged(
                                    nameof(MainViewModel.Instance.SelectedTuning));

                                UpdateAvailableSortOrder(true);

                                if(Design.IsDesignMode ||
                                   !MainViewModel.Instance.IsLoaded ||
                                   MainView.Instance is not { } mv ||
                                   mv.DlgHost is not { } mdh ||
                                   !mdh.IsLoaded) {
                                    return;
                                }

                                string sel_msg = $"{FullName} selected";
                                SnackbarHost.Post(
                                    sel_msg,
                                    MainView.SnackbarHostName,
                                    DispatcherPriority.Background);
                            },DispatcherPriority.Background);
                    }

                    break;
            }
        }

        async Task<bool> LoadPatternsAsync() {
            bool success = false;

            while(true) {
                if(MainView.Instance is not { } mv ||
                   !mv.DlgHost.IsLoaded) {
                    await Task.Delay(100);
                }

                break;
            }

            PatternGen pg = new PatternGen(this);
            Dispatcher.UIThread.Post(
                () => {
                    if(!MainViewModel.Instance.IsLoaded) {
                        // initial startup gen
                        DialogHost.Close(MainViewModel.Instance.MainDialogHostName);
                    }

                    TuningGenProgressView pv = new TuningGenProgressView { DataContext = pg };
                    DialogHost.Show(
                            pv,MainViewModel.Instance.MainDialogHostName)
                        .FireAndForgetSafeAsync();
                });


            PatternGenCts = new CancellationTokenSource();
            try {
                await Task.Run(
                    async () => {
                        var patterns = await pg.GenerateAsync(PatternGenCts.Token);
                        Tuning.Collections.Keys.ForEach(x => Tuning.Collections[x].AddRange(patterns[x]));
                        success = true;
                    },PatternGenCts.Token);
            } catch {
                // canceled
            }

            DialogHost.Close(MainViewModel.Instance.MainDialogHostName);

            return success;
        }

        void BookmarkGroups_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(BookmarkColumnCount));
        }

        void SelectedBookmarkGroups_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            Debug.Assert(
                SelectedBookmarkGroups.None(x => x.IsAddGroupPlaceholder),"Placeholder group shouldn't be selected");

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

        double GetScore(NotePattern pattern,InstrumentNote[] matchNotes,MatchScoreMethodType scoring) {
            double score = 0;
            if(pattern.ToString() == "C minor_1_1" ||
               pattern.ToString() == "C major_1_4") {
                //bls
            }

            double[,] score_matrix = new double[pattern.Parent.Parent.Parent.RowCount,matchNotes.Length];
            for(int p = 0; p < pattern.Parent.Parent.Parent.RowCount; p++) {
                if(scoring == MatchScoreMethodType.Exact) {
                    if(matchNotes.FirstOrDefault(x => x.RowNum == p) is not { } match_row_note) {
                        continue;
                    }

                    double exact_score = pattern.Notes[p].SimilarityScore(match_row_note,scoring);
                    if(exact_score == 0) {
                        return 0;
                    }

                    score += exact_score;
                    continue;
                }

                for(int m = 0; m < matchNotes.Length; m++) {
                    score_matrix[p,m] = pattern.Notes[p].SimilarityScore(matchNotes[m],scoring);
                }
            }

            if(scoring != MatchScoreMethodType.Exact) {
                var used_pattern_note_idxl = new List<int>();
                for(int m = 0; m < matchNotes.Length; m++) {
                    double max_match_score = 0;
                    int max_match_pattern_idx = -1;
                    for(int p = 0; p < pattern.Notes.Count; p++) {
                        if(used_pattern_note_idxl.Contains(p)) {
                            continue;
                        }

                        if(score_matrix[p,m] > max_match_score) {
                            max_match_score = score_matrix[p,m];
                            max_match_pattern_idx = p;
                        }
                    }

                    if(max_match_pattern_idx < 0) {
                        continue;
                    }

                    score += max_match_score;
                    used_pattern_note_idxl.Add(max_match_pattern_idx);
                }
            }

            return score / matchNotes.Length;

            // foreach(InstrumentNote mn in matchNotes.Where(x=>!x.IsMute)) {
            //     double max_score = pattern.Notes.Where(x=>!x.IsMute).Max(x => x.SimilarityScore(mn,scoring));
            //     if(scoring == MatchScoreMethodType.Exact && max_score == 0) {
            //         return 0;
            //     }
            //
            //     score += max_score;
            // }
            //
            // int pattern_len = matchNotes.Length;
            // //int pattern_len = matchNotes.Count(x => !x.IsMute);
            // // int pattern_len = pattern.Notes.Count(x => !x.IsMute) +
            // //                   Math.Min(pattern.Notes.Count(x => x.IsMute),matchNotes.Count(x => x.IsMute));
            //
            // return score / pattern_len;
        }

        #endregion

        #region Commands

        public ICommand DeleteThisTuningCommand => new MpCommand<object>(
            async (args) => {
                Extensions.CloseFlyout(args);

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
                            }),
                    },
                };
                DialogHost.Show(dlg_v,MainViewModel.Instance.InstEditDialogHostName).FireAndForgetSafeAsync();

                while(!confirmed.HasValue) {
                    await Task.Delay(100);
                }

                DialogHost.Close(MainViewModel.Instance.InstEditDialogHostName);

                if(!confirmed.Value) {
                    return;
                }

                Parent.RemoveTuningCommand.Execute(this);
            },(args) => {
                return CanDelete;
            });

        public ICommand DuplicateThisTuningCommand => new MpCommand<object>(
            (args) => {
                Extensions.CloseFlyout(args);
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
                Extensions.CloseFlyout(args);
                ChordsCount = Tuning.Chords.SelectMany(x => x.Patterns).Count();
                ScalesCount = Tuning.Scales.SelectMany(x => x.Patterns).Count();
                ModesCount = Tuning.Modes.SelectMany(x => x.Patterns).Count();

                BookmarkCount =
                    Tuning.Collections.Values.SelectMany(x => x)
                        .SelectMany(x => x.Patterns)
                        .Count(x => x.IsBookmarked);

                TuningStatsView stats_view = new TuningStatsView
                {
                    DataContext = this,
                };
                stats_view.OkButton.Command = new MpCommand(
                    () => {
                        DialogHost.Close(MainViewModel.Instance.InstEditDialogHostName);

                    });

                DialogHost.Show(stats_view,MainViewModel.Instance.InstEditDialogHostName);

            });

        public ICommand SelectThisTuningCommand => new MpCommand(
            () => {
                Parent.SelectedTuning = this;
            });

        public ICommand CancelPatternGenCommand => new MpCommand(
            () => {
                PatternGenCts?.Cancel();
            });


        public ICommand AddNewBookmarkGroupCommand => new MpCommand<object>(
            (args) => {
                BookmarkGroup new_bmg = BookmarkGroup.Create(
                    Tuning,MainViewModel.Instance.SelectedPatternType,
                    colorId: GetNewBookmarkColorId());
                BookmarkGroupViewModel new_bmg_vm = new BookmarkGroupViewModel(this,new_bmg);
                // NOTE args may be a note pattern when from match menu
                new_bmg_vm.BeginEditCommand.Execute(args);
            });

        #endregion

    }

}