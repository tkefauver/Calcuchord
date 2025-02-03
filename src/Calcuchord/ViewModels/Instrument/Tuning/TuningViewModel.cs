using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Calcuchord.JsonChords;
using DialogHostAvalonia;
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

        #endregion

        #region Layout

        #endregion

        #region State

        public int BookmarkCount { get; private set; }

        bool HasFretNumRow =>
            Parent.InstrumentType != InstrumentType.Piano;

        public int SelectedOpenNoteIndex { get; set; } = 0;

        public bool IsLoaded =>
            Tuning.Chords.Any() &&
            Tuning.Scales.Any() &&
            Tuning.Modes.Any();

        public bool IsSelected { get; set; }

        #endregion

        #region Model

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

        public int FretCount =>
            Tuning.FretCount;

        int StringCount =>
            Parent.RowCount;

        // +2 for label and nut
        public int LogicalFretCount =>
            FretCount + (Parent.IsKeyboard ? 0 : 2);

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

        public async Task InitAsync(Tuning tuning) {
            IsBusy = true;

            Tuning = tuning;
            Tuning.SetParent(Parent.Model);

            NoteRows.Clear();
            Tuning.OpenNotes.OrderBy(x => x.RowNum).ForEach(x => NoteRows.Add(new(this,x)));
            if(HasFretNumRow) {
                // add fret num row
                NoteRows.Insert(0,new(this,null));
            }

            OpenNotes.Clear();
            OpenNotes.AddRange(NoteRows
                .Skip(HasFretNumRow ? 1 : 0)
                .Select(x => x.OpenNote)
                .OrderBy(x => x.RowNum));

            if(!IsLoaded) {
                if(Prefs.Instance.Instruments.SelectMany(x => x.Tunings).FirstOrDefault(x => x.Id == Id) is not
                   { } tuning_model) {
                    if(!Parent.IsEditModeEnabled) {
                        // error
                        Debugger.Break();
                    }

                    return;
                }

                await CreateChordsAsync();
                tuning_model.Chords = Tuning.Chords;
                await CreateScalesAndModesAsync();
                tuning_model.Scales = Tuning.Scales;
                tuning_model.Modes = Tuning.Modes;

            }

            Parent.Model.RefreshModelTree();

            IsBusy = false;
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
                case nameof(IsSelected):
                    if(IsSelected && !Parent.IsEditModeEnabled) {
                        ResetSelection();
                        Prefs.Instance.SelectedTuningId = Id;
                        Prefs.Instance.Save();
                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));
                    }

                    break;
            }
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
                var result = await pg.GenerateAsync();
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
                       keys_obj.OfType<Key>().FirstOrDefault() is not { } key_obj ||
                       MusicHelpers.ParseNote(key_obj.key) is not { } key_note_tup) {
                        continue;
                    }

                    NoteType cur_key = key_note_tup.nt;

                    foreach(Key chord_group in keys_obj) {
                        string cur_suffix = chord_group.suffix;
                        NoteGroupCollection ngc = new NoteGroupCollection(
                            MusicPatternType.Chords,cur_key,chord_group.suffix);
                        foreach((Position pos,int pos_num) in chord_group.positions.WithIndex()) {
                            NoteGroup ng = new NoteGroup(ngc,pos_num);
                            ng.CreateId(null);
                            foreach((int f,int str_num) in pos.real_frets.WithIndex()) {
                                ng.Notes.Add(
                                    new(
                                        pos.fingers[str_num],f,str_num,tuning.OpenNotes[str_num].Offset(f).Key,
                                        tuning.OpenNotes[str_num].Register));
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
            PatternGen pg = new PatternGen(MusicPatternType.Scales,Tuning);
            var scales = await pg.GenerateAsync();
            Tuning.Scales.AddRange(scales);
            Debug.WriteLine(
                $"{scales.SelectMany(x => x.Groups).Count()} scales generated for {Tuning}");

            pg = new(MusicPatternType.Modes,Tuning);
            var modes = await pg.GenerateAsync();
            Tuning.Modes.AddRange(modes);
            Debug.WriteLine(
                $"{modes.SelectMany(x => x.Groups).Count()} modes generated for {Tuning}");
        }

        async Task AdjustCapoAsync(int capoDelta) {
            CapoNum += capoDelta;
            var new_open_notes = Tuning.OpenNotes.Select((x,idx) => new InstrumentNote(0,idx,x.Offset(capoDelta)))
                .ToList();
            Tuning.OpenNotes.Clear();
            Tuning.OpenNotes.AddRange(new_open_notes);
            await InitAsync(Tuning);
        }

        #endregion

        #region Commands

        public bool CanDelete =>
            !IsReadOnly && Parent.Tunings.Count > 1;

        public ICommand DeleteThisTuningCommand => new MpCommand(() => {
            Parent.RemoveTuningCommand.Execute(Id);
        },() => {
            return CanDelete;
        });

        public ICommand DuplicateThisTuningCommand => new MpCommand(() => {
            Tuning dup_tuning = Tuning.Clone();
            dup_tuning.IsReadOnly = false;
            dup_tuning.Name = Parent.GetUniqueTuningName(Name,[]);
            Parent.AddTuningCommand.Execute(dup_tuning);
        });

        public ICommand IncreaseCapoFretCommand => new MpAsyncCommand(async () => {
            await AdjustCapoAsync(1);
        },() => {
            return FretCount > Parent.MinEditableFretCount;
        });

        public ICommand DecreaseCapoFretCommand => new MpAsyncCommand(async () => {
            await AdjustCapoAsync(-1);
        },() => {
            return CapoNum > 0;
        });


        public ICommand ShowStatsCommand => new MpCommand(() => {
            BookmarkCount =
                Tuning.Collections.Values.SelectMany(x => x)
                    .SelectMany(x => x.Groups)
                    .Count(x => Prefs.Instance.BookmarkIds.Contains(x.Id));

            DialogHost.Show(this,"MainDialogHost");
        });

        public ICommand SelectThisTuningCommand => new MpCommand(() => {
            Parent.SelectedTuning = this;
        });

        #endregion

    }
}