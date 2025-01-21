using System;
using System.Collections;
using System.Collections.Generic;
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

        public InstrumentTuning Tuning { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public TuningViewModel(InstrumentViewModel parent,InstrumentTuning tuning) : base(parent) {
            PropertyChanged += InstrumentTuningViewModel_OnPropertyChanged;
            Tuning = tuning;
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
                Prefs.Instance.Save();
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
                    break;
            }
        }


        async Task CreateChordsAsync() {
            bool from_file = Tuning.IsDefault && (Tuning.Parent.InstrumentType == InstrumentType.Guitar ||
                                                  Tuning.Parent.InstrumentType == InstrumentType.Ukulele);
            IEnumerable<NoteGroupCollection> chords = null;
            if(from_file) {
                chords = GenFromFile(Tuning);
            } else {
                chords = await GenFromPatternsAsync(Tuning);
            }

            Tuning.Chords.AddRange(chords);

            Debug.WriteLine(
                $"{chords.SelectMany(x => x.Groups).Count()} chords {(from_file ? " loaded from file" : " generated")} for {Tuning}");

            NoteGroup test_c = Tuning.Chords.FirstOrDefault(x => x.Key == NoteType.C && x.Suffix.ToLower() == "major")
                .Groups
                .FirstOrDefault(x => x.Position == 0);

            NoteGroup test_fmaj = Tuning.Chords
                .FirstOrDefault(x => x.Key == NoteType.F && x.Suffix.ToLower() == "major")
                .Groups
                .FirstOrDefault(x => x.Position == 0);

            NoteGroup test_emaj4 = Tuning.Chords
                .FirstOrDefault(x => x.Key == NoteType.E && x.Suffix.ToLower() == "major")
                .Groups
                .FirstOrDefault(x => x.Position == 3);
            return;

            async Task<IEnumerable<NoteGroupCollection>> GenFromPatternsAsync(InstrumentTuning tuning) {
                await Task.Delay(1);
                return [];
            }

            IEnumerable<NoteGroupCollection> GenFromFile(InstrumentTuning tuning) {
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
            await Task.Delay(1);
            PatternGen pg = new(MusicPatternType.Scales,Tuning);
            var scales = pg.Generate();
            Tuning.Scales.AddRange(scales);
            Debug.WriteLine(
                $"{scales.SelectMany(x => x.Groups).Count()} scales generated for {Tuning}");

            pg = new(MusicPatternType.Modes,Tuning);
            var modes = pg.Generate();
            Tuning.Modes.AddRange(modes);
            Debug.WriteLine(
                $"{modes.SelectMany(x => x.Groups).Count()} modes generated for {Tuning}");

        }

        #endregion

        #region Commands

        #endregion
    }
}