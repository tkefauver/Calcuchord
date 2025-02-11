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
                    // json error, about to delete are you sure?
                    Debugger.Break();

                    // TODO should maybe say there was an error here instead of just reseting data
                    File.Delete(PrefsFilePath);
                    Instance = null;
                    Init();
                    return;
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
        public bool IsThemeDark { get; set; }

        [JsonProperty]
        public int MatchColCount { get; set; } = 1;


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


            if(MainViewModel.Instance is { } mvm) {
                Instruments = mvm.Instruments.Select(x => x.Instrument).ToList();
                Options = mvm.OptionLookup.Values.SelectMany(x => x).ToList();
                IsThemeDark = ThemeViewModel.Instance.IsDark;
                MatchColCount = mvm.MatchColCount;
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

        void LogPrefs() {
            Debug.WriteLine("");
            string tuning_str = MainViewModel.Instance == null || MainViewModel.Instance.SelectedTuning == null
                ? string.Empty
                : MainViewModel.Instance.SelectedTuning.ToString();
            string sel_tuning_full_name = "NONE";
            if(Instruments.FirstOrDefault(x => x.IsSelected) is { } sel_ivm &&
               sel_ivm.Tunings.FirstOrDefault(x => x.IsSelected) is { } sel_tvm) {
                sel_tuning_full_name = sel_tvm.ToString();
            }

            Debug.WriteLine($"{DateTime.Now} prefs saved. SelectedTuningId: {sel_tuning_full_name} {tuning_str}");
            foreach(Instrument inst in Instruments) {
                foreach(Tuning tuning in inst.Tunings) {
                    Debug.WriteLine(
                        $"{inst} Chords: {tuning.Chords.SelectMany(x => x.Groups).Count()} Scales: {tuning.Scales.SelectMany(x => x.Groups).Count()} Modes: {tuning.Modes.SelectMany(x => x.Groups).Count()}");
                }
            }

            Debug.WriteLine("");
        }

        void Validate() {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            if(mvm.SelectedTuning == null) {
                // should always have a selected tuning
                Debugger.Break();
            }

            if(Instruments.SelectMany(x => x.Tunings).SelectMany(x => x.Collections.Values).SelectMany(x => x)
                   .SelectMany(x => x.Groups) is { } all_ngl &&
               all_ngl.GroupBy(x => x.Id).Where(x => x.Count() > 1) is { } dup_nggl &&
               dup_nggl.Any()) {

                // BUG randomly bookmarking duplicates the noteGroup
                // i think its a virtualization thing maybe w/ the items repeater maybe 
                // foreach(Tuning tuning in all_tunings) {
                //     bool needs_update = false;
                //     foreach(var coll in tuning.Collections.Values) {
                //         if(coll.SelectMany(x => x.Groups).GroupBy(x => x.FullName).Where(x => x.Count() > 1) is
                //                { } dup_ngl &&
                //            dup_ngl.Any()) {
                //             foreach(var dup_group_to_remove in dup_ngl) {
                //                 foreach(NoteGroup dup_to_remove in dup_group_to_remove.Skip(1)) {
                //                     dup_to_remove.Parent.Groups.Remove(dup_to_remove);
                //                     needs_update = true;
                //                 }
                //             }
                //         }
                //     }
                //
                //     if(needs_update &&
                //        mvm.Instruments.SelectMany(x => x.Tunings).FirstOrDefault(x => x.Tuning == tuning) is
                //            { } tuning_vm) {
                //         tuning_vm.InitAsync(tuning).FireAndForgetSafeAsync();
                //     }
                // }
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