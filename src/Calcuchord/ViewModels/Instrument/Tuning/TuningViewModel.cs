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

        public ObservableCollection<StringRowViewModel> StringRows { get; } = [];

        public IEnumerable<FretViewModel> AllFrets =>
            StringRows.SelectMany(x => x.Frets);

        public IEnumerable<FretViewModel> SelectedFrets =>
            AllFrets.Where(x => x.IsSelected);

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsLoaded =>
            Tuning.Chords.Any() && Tuning.Scales.Any();

        public bool IsSelected {
            get => Parent.SelectedTuning == this;
            set {
                if(value) {
                    Tuning.LastSelectedDt = DateTime.Now;
                    HasModelChanged = true;
                }

                OnPropertyChanged(nameof(IsSelected));
            }
        }

        #endregion

        #region Model

        public Tuning Tuning { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public TuningViewModel(InstrumentViewModel parent,Tuning tuning) : base(parent) {
            PropertyChanged += InstrumentTuningViewModel_OnPropertyChanged;
            Tuning = tuning;
            Tuning.SetParent(parent.Instrument);

            Tuning.OpenNotes.ForEach(x => StringRows.Add(new(this,x)));
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

        #endregion

        #region Private Methods

        void InstrumentTuningViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.SelectedTuning));
                    }

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