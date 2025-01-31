using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class Prefs : ViewModelBase {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        static bool RESET_PREFS => false;

        [JsonIgnore]
        static string _prefsFilePath;

        [JsonIgnore]
        static string PrefsFilePath {
            get {
                if(_prefsFilePath == null &&
                   PlatformWrapper.StorageHelper is { } sh &&
                   sh.StorageDir is { } sd) {
                    string fn = "appstate.json";
                    _prefsFilePath = Path.Combine(sd,fn);
                }

                return _prefsFilePath;
            }
        }

        public static void Init() {
            if(RESET_PREFS) {
                File.Delete(PrefsFilePath);
            }

            //
            bool is_initial_startup = !File.Exists(PrefsFilePath);

            if(is_initial_startup) {
                _ = new Prefs();
                Instance.Save();
            } else {
                try {
                    _ = JsonConvert.DeserializeObject<Prefs>(File.ReadAllText(PrefsFilePath));
                    Instance.Instruments.ForEach(x => x.RefreshModelTree());
                } catch(Exception e) {
                    e.Dump();
                    _ = new Prefs();
                }
            }


        }

        public static Prefs Instance { get; private set; }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        public string SelectedTuningId { get; set; }


        public SvgFlags SelectedSvgFlags { get; set; } = SvgBuilderBase.DefaultSvgFlags;


        public bool IsThemeDark { get; set; }


        public List<string> BookmarkIds { get; set; } = [];


        public List<Instrument> Instruments { get; set; } = [];


        public List<OptionViewModel> Options { get; set; } = [];

        #endregion

        #region Ignored

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public Prefs() {
            Debug.WriteLine("prefs ctor called");
            if(Instance != null) {
                return;
            }

            Instance = this;

            //PropertyChanged += PropertyChanged_OnPropertyChanged;
            // Instruments.CollectionChanged += Coll_OnCollectionChanged;
            // ChordBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
            // ScaleBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
        }

        #endregion

        #region Public Methods

        public void Save() {
            Debug.WriteLine("");
            string tuning_str = MainViewModel.Instance == null || MainViewModel.Instance.SelectedTuning == null
                ? string.Empty
                : MainViewModel.Instance.SelectedTuning.ToString();
            Debug.WriteLine($"{DateTime.Now} prefs saved. SelectedTuningId: {SelectedTuningId} {tuning_str}");
            foreach(Instrument inst in Instruments) {
                foreach(Tuning tuning in inst.Tunings) {
                    Debug.WriteLine(
                        $"{inst} Chords: {tuning.Chords.SelectMany(x => x.Groups).Count()} Scales: {tuning.Scales.SelectMany(x => x.Groups).Count()} Modes: {tuning.Modes.SelectMany(x => x.Groups).Count()}");
                }
            }

            Debug.WriteLine("");
            string pref_json = JsonConvert.SerializeObject(this);
            MpFileIo.WriteTextToFile(PrefsFilePath,pref_json);

            try {
                Prefs test = JsonConvert.DeserializeObject<Prefs>(pref_json);
            } catch(Exception e) {
                e.Dump();
            }
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void Coll_OnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            Save();
        }

        void PropertyChanged_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            Save();
        }

        #endregion

        #region Commands

        #endregion

    }
}