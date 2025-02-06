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

        public static Prefs Instance { get; private set; }

        public static void Init() {
            if(RESET_PREFS) {
                File.Delete(PrefsFilePath);
            }

            bool is_initial_startup = !File.Exists(PrefsFilePath);

            if(is_initial_startup) {
                _ = new Prefs();
                //Instance.Save();
            } else {
                try {
                    string prefs_json = File.ReadAllText(PrefsFilePath);
                    _ = JsonConvert.DeserializeObject<Prefs>(prefs_json);
                    Instance.Instruments.ForEach(x => x.RefreshModelTree());
                } catch(Exception e) {
                    e.Dump();
                    _ = new Prefs();
                }
            }

            Instance.IsInitialStartup = is_initial_startup;
        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public string SelectedTuningId { get; set; } = Instrument.STANDARD_GUITAR_TUNING_ID;

        [JsonProperty]
        public SvgFlags SelectedSvgFlags { get; set; } = SvgBuilderBase.DefaultSvgFlags;

        [JsonProperty]
        public bool IsThemeDark { get; set; }

        [JsonProperty]
        public List<string> BookmarkIds { get; set; } = [];

        [JsonProperty]
        public List<Instrument> Instruments { get; set; } = [];

        [JsonProperty]
        public List<OptionViewModel> Options { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        public bool IsSaveIgnored { get; set; }

        [JsonIgnore]
        public bool IsInitialStartup { get; private set; }

        [JsonIgnore]
        public bool IsPrefsPersistent => File.Exists(PrefsFilePath);

        [JsonIgnore]
        static bool RESET_PREFS => false;

        [JsonIgnore]
        static string _prefsFilePath;

        [JsonIgnore]
        static string PrefsFilePath {
            get {
                if(_prefsFilePath == null &&
                   PlatformWrapper.Services is { } ps &&
                   ps.StorageHelper is { } sh &&
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

        public Prefs() {
            Debug.WriteLine("prefs ctor called");
            if(Instance != null) {
                // singleton erro
                Debugger.Break();
            }

            Instance = this;

        }

        #endregion

        #region Public Methods

        public void Save() {
            if(IsSaveIgnored) {
                Debug.WriteLine("prefs save ignored");
                return;
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
            if(MainViewModel.Instance is { } mvm) {
                Instruments = mvm.Instruments.Select(x => x.Instrument).ToList();
                Options = mvm.OptionLookup.Values.SelectMany(x => x).ToList();
                if(mvm.SelectedTuning is { } sel_tun) {
                    SelectedTuningId = sel_tun.Id;
                }

                IsThemeDark = ThemeViewModel.Instance.IsDark;

            }


            Validate();
            try {
                bool is_new = !File.Exists(PrefsFilePath);
                string pref_json = JsonConvert.SerializeObject(this);
                File.WriteAllText(PrefsFilePath,pref_json);
                if(is_new) {
                    Debug.WriteLine("Prefs CREATED");
                } else {
                    Debug.WriteLine("Prefs SAVED");
                }
            } catch(Exception e) {
                e.Dump();
            }
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void Validate() {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            if(mvm.SelectedTuning == null) {
                Debug.Assert(string.IsNullOrEmpty(SelectedTuningId),"Save error");
            } else {
                Debug.Assert(SelectedTuningId == mvm.SelectedTuning.Id,"Save errror");
            }

            if(Instruments.Difference(mvm.Instruments.Select(x => x.Instrument)) is { } inst_diffs &&
               inst_diffs.Any()) {
                Debugger.Break();
            }

            if(Options.Difference(mvm.OptionLookup.Values.SelectMany(x => x)) is { } opts_diffs && opts_diffs.Any()) {
                Debugger.Break();
            }

            Debug.Assert(IsThemeDark == ThemeViewModel.Instance.IsDark,"save error");

        }

        #endregion

        #region Commands

        #endregion

    }
}