using System;
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

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsLoaded =>
            Tuning.Chords.Any() && Tuning.Scales.Any();

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public string Id =>
            Tuning.Id;

        public bool IsDefault =>
            Tuning.IsDefault;

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
            Tuning.SetParent(Parent.Instrument);

            Tuning.OpenNotes.OrderBy(x => x.RowNum).ForEach(x => NoteRows.Add(new(this,x)));
            if(!Parent.IsKeyboard) {
                NoteRows.Insert(0,new(this,null));
            }

            if(!Tuning.Chords.Any()) {
                await CreateChordsAsync();
                if(Prefs.Instance.Instruments.SelectMany(x => x.Tunings).FirstOrDefault(x => x.Id == Id) is
                   { } tuning_model) {
                    tuning_model.Chords = Tuning.Chords;
                }
                // error
            }

            if(!Tuning.Scales.Any() || !Tuning.Modes.Any()) {
                await CreateScalesAndModesAsync();

                if(Prefs.Instance.Instruments.SelectMany(x => x.Tunings).FirstOrDefault(x => x.Id == Id) is
                   { } tuning_model) {
                    tuning_model.Scales = Tuning.Scales;
                    tuning_model.Modes = Tuning.Modes;
                }
                // error
            }

            Parent.Instrument.RefreshModelTree();

            IsBusy = false;
        }

        public void ResetSelection() {
            NoteRows.ForEach(x => x.ResetSelection());
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentTuningViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        ResetSelection();
                        Prefs.Instance.SelectedTuningId = Id;
                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));
                        // if(Parent.IsKeyboard &&
                        //    AllNotes.OfType<KeyViewModel>() is { } kvml) {
                        //     kvml.ForEach(x => x.OnPropertyChanged(nameof(x.KeyX)));
                        //     kvml.ForEach(x => x.OnPropertyChanged(nameof(x.KeyWidth)));
                        //     kvml.ForEach(x => x.OnPropertyChanged(nameof(x.KeyHeight)));
                        //     Debug.WriteLine(string.Join(",",kvml.Select(x => x.KeyX)));
                        // }
                    }

                    break;
            }
        }


        async Task CreateChordsAsync() {
            bool from_file = Tuning.IsDefault &&
                             (Tuning.Parent.InstrumentType == InstrumentType.Guitar ||
                              Tuning.Parent.InstrumentType == InstrumentType.Ukulele);
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
                            ng.Id = Guid.NewGuid().ToString();
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

        #endregion

        #region Commands

        #endregion

    }
}