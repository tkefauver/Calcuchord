using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Calcuchord.JsonChords;
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

        public IEnumerable<StringRowViewModel> SortedStringRows =>
            StringRows.OrderBy(
                x => x.StringNum < 0 ? -1 : IsStringsDescending ? StringCount - x.StringNum : x.StringNum);

        public ObservableCollection<StringRowViewModel> StringRows { get; } = [];

        public IEnumerable<FretViewModel> AllFrets =>
            StringRows.SelectMany(x => x.Frets);

        public IEnumerable<FretViewModel> SelectedFrets =>
            AllFrets.Where(x => x.IsSelected);

        #endregion

        #region Appearance

        public string StringSortIcon =>
            IsStringsDescending ? "SortDescending" : "SortAscending";

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsStringsDescending {
            get => Prefs.Instance.IsStringsDescending;
            set {
                Prefs.Instance.IsStringsDescending = value;
                OnPropertyChanged(nameof(IsStringsDescending));
            }
        }

        public bool IsLoaded =>
            Tuning.Chords.Any() && Tuning.Scales.Any();

        public bool IsSelected {
            get => Tuning.IsSelected;
            set {
                Tuning.IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        #endregion

        #region Model

        public int FretCount =>
            Tuning.FretCount;

        int StringCount =>
            Parent.StringCount;

        // +2 for label and nut
        public int LogicalFretCount =>
            FretCount + (Parent.IsKeyboard ? 0 : 2);

        public Tuning Tuning { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public TuningViewModel() {
        }

        public TuningViewModel(InstrumentViewModel parent,Tuning tuning) : base(parent) {
            PropertyChanged += InstrumentTuningViewModel_OnPropertyChanged;
            Init(tuning);
        }

        #endregion

        #region Public Methods

        public async Task InitCollectionsAsync() {
            bool needs_save = false;
            if(!Tuning.Chords.Any()) {
                await CreateChordsAsync();
                needs_save = true;
            }

            if(!Tuning.Scales.Any()) {
                await CreateScalesAsync();
                needs_save = true;
            }

            if(needs_save) {
                Prefs.Instance.SyncAndSave();
            }

            Parent.Instrument.RefreshModelTree();
        }

        #endregion

        #region Protected Methods

        public void Init(Tuning tuning) {
            Tuning = tuning;
            Tuning.SetParent(Parent.Instrument);


            Tuning.OpenNotes.OrderBy(x => x.StringNum).ForEach(x => StringRows.Add(new(this,x)));
            if(!Parent.IsKeyboard) {
                StringRows.Insert(0,new(this,null));
            }
            OnPropertyChanged(nameof(IsStringsDescending));
        }

        #endregion

        #region Private Methods

        void InstrumentTuningViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));
                    }

                    break;
                case nameof(IsStringsDescending):
                    OnPropertyChanged(nameof(SortedStringRows));
                    OnPropertyChanged(nameof(StringSortIcon));
                    AllFrets.ForEach(x => x.OnPropertyChanged(nameof(x.IsTopDotFret)));
                    AllFrets.ForEach(x => x.OnPropertyChanged(nameof(x.IsBottomDotFret)));

                    break;
            }
        }


        async Task CreateChordsAsync() {
            bool from_file = Tuning.IsDefault && (Tuning.Parent.InstrumentType == InstrumentType.Guitar ||
                                                  Tuning.Parent.InstrumentType == InstrumentType.Ukulele);
            from_file = false;
            IEnumerable<NoteGroupCollection> chords = null;
            if(from_file) {
                chords = GenFromFile(Tuning);
            } else {
                chords = await GenFromPatternsAsync(Tuning);
            }

            Tuning.Chords.AddRange(chords);
            return;

            async Task<IEnumerable<NoteGroupCollection>> GenFromPatternsAsync(Tuning tuning) {
                PatternGen pg = new(MusicPatternType.Chords,tuning);
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
                        NoteGroupCollection ngc = new(MusicPatternType.Chords,cur_key,chord_group.suffix);
                        foreach((Position pos,int pos_num) in chord_group.positions.WithIndex()) {
                            NoteGroup ng = new(ngc,pos_num);
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

        async Task CreateScalesAsync() {
            PatternGen pg = new(MusicPatternType.Scales,Tuning);
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

        #endregion

        #region Commands

        #endregion

    }
}