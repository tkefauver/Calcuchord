using System;
using System.Collections.Generic;
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

        static Prefs _instance;
        public static Prefs Instance => _instance ??= new();

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        public string SelectedTuningId { get; set; } = Instrument.STANDARD_GUITAR_TUNING_ID;


        public SvgFlags SelectedSvgFlags { get; set; } = SvgBuilderBase.DefaultSvgFlags;


        public bool IsThemeDark { get; set; }


        public List<string> BookmarkIds { get; set; } = [];


        public List<Instrument> Instruments { get; set; } = [];


        public List<OptionViewModel> Options { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        public bool IsPrefsPersistent { get; private set; }

        [JsonIgnore]
        bool RESET_PREFS => false;

        [JsonIgnore]
        string _prefsFilePath;

        [JsonIgnore]
        string PrefsFilePath {
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

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        Prefs() {
            Debug.WriteLine("prefs ctor called");
        }

        #endregion

        #region Public Methods

        public void Init() {
            if(RESET_PREFS) {
                File.Delete(PrefsFilePath);
            }

            bool is_initial_startup = !File.Exists(PrefsFilePath);

            if(is_initial_startup) {
                _ = new Prefs();
                Save();
            } else {
                try {
                    _ = JsonConvert.DeserializeObject<Prefs>(File.ReadAllText(PrefsFilePath));
                    Instruments.ForEach(x => x.RefreshModelTree());
                } catch(Exception e) {
                    e.Dump();
                    _ = new Prefs();
                }
            }

        }

        public void Save() {
            if(MainViewModel.Instance is { } mvm &&
               mvm.EditModeInstrument != null) {
                // should be avoided
            }

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
            try {
                string pref_json = JsonConvert.SerializeObject(this);
                File.WriteAllText(PrefsFilePath,pref_json);
                IsPrefsPersistent = true;
            } catch(Exception e) {
                e.Dump();
                IsPrefsPersistent = false;
            }
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}