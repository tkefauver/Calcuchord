using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            if(PlatformWrapper.Services is not { } ps ||
               ps.PrefsIo is not { } prefsIo) {
                return;
            }

            if(RESET_PREFS) {
                prefsIo.WritePrefs(string.Empty);
            }

            string prefs_json = prefsIo.ReadPrefs();

            bool is_initial_startup = string.IsNullOrEmpty(prefs_json);

            PlatformWrapper.Services.Logger.WriteLine($"Initial Startup: {is_initial_startup}");

            if(is_initial_startup) {
                _ = new Prefs();
            } else {
                try {
                    _ = JsonConvert.DeserializeObject<Prefs>(prefs_json);
                    Instance.Instruments.ForEach(x => x.RefreshModelTree());
                } catch(Exception e) {
                    e.Dump();
                    // json error, about to delete are you sure?
                    Debugger.Break();

                    // TODO should maybe say there was an error here instead of just reseting data
                    prefsIo.WritePrefs(string.Empty);
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
        public int MatchColCount { get; set; } = 3;


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
        static bool RESET_PREFS => false;

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public Prefs() {
            PlatformWrapper.Services.Logger.WriteLine("prefs ctor called");
            if(Instance != null) {
                // singleton erro
                Debugger.Break();
            }

            Instance = this;

        }

        #endregion

        #region Public Methods

        public void Save() {
            Task.Run(
                () => {
                    if(PlatformWrapper.Services is not { } ps ||
                       ps.PrefsIo is not { } prefsIo) {
                        PlatformWrapper.Services.Logger.WriteLine("prefs io service unavailable");
                        return;
                    }

                    if(IsSaveIgnored) {
                        PlatformWrapper.Services.Logger.WriteLine("prefs save ignored");
                        return;
                    }

                    SyncModels();


                    // Validate();
                    try {
                        string pref_json = JsonConvert.SerializeObject(this);
                        prefsIo.WritePrefs(pref_json);

                        if(IsInitialStartup) {
                            PlatformWrapper.Services.Logger.WriteLine("Prefs CREATED");
                        } else {
                            PlatformWrapper.Services.Logger.WriteLine("Prefs SAVED");
                        }
                    } catch(Exception e) {
                        e.Dump();
                    }
                });
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void SyncModels() {
            if(MainViewModel.Instance is { } mvm) {
                if(OperatingSystem.IsBrowser()) {
                    Instruments = mvm.Instruments.Select(x => x.Instrument.Clone()).ToList();
                    Instruments.SelectMany(x => x.Tunings).ForEach(x => x.ClearPatterns());
                } else {
                    Instruments = mvm.Instruments.Select(x => x.Instrument).ToList();
                }

                Options = mvm.OptionLookup.Values.SelectMany(x => x).ToList();
                MatchColCount = mvm.MatchColCount;
            }

            if(ThemeViewModel.Instance is { } tvm) {
                IsThemeDark = tvm.IsDark;
            }
        }

        void LogPrefs() {
            PlatformWrapper.Services.Logger.WriteLine("");
            string tuning_str = MainViewModel.Instance == null || MainViewModel.Instance.SelectedTuning == null
                ? string.Empty
                : MainViewModel.Instance.SelectedTuning.ToString();
            string sel_tuning_full_name = "NONE";
            if(Instruments.FirstOrDefault(x => x.IsSelected) is { } sel_ivm &&
               sel_ivm.Tunings.FirstOrDefault(x => x.IsSelected) is { } sel_tvm) {
                sel_tuning_full_name = sel_tvm.ToString();
            }

            PlatformWrapper.Services.Logger.WriteLine(
                $"{DateTime.Now} prefs saved. SelectedTuningId: {sel_tuning_full_name} {tuning_str}");
            foreach(Instrument inst in Instruments) {
                foreach(Tuning tuning in inst.Tunings) {
                    PlatformWrapper.Services.Logger.WriteLine(
                        $"{inst} Chords: {tuning.Chords.SelectMany(x => x.Patterns).Count()} Scales: {tuning.Scales.SelectMany(x => x.Patterns).Count()} Modes: {tuning.Modes.SelectMany(x => x.Patterns).Count()}");
                }
            }

            PlatformWrapper.Services.Logger.WriteLine("");
        }

        void Validate() {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            // if(mvm.SelectedTuning == null) {
            //     // should always have a selected tuning
            //     Debugger.Break();
            // }

            if(Instruments.SelectMany(x => x.Tunings).SelectMany(x => x.Collections.Values).SelectMany(x => x)
                   .SelectMany(x => x.Patterns) is { } all_ngl &&
               all_ngl.GroupBy(x => x.Id).Where(x => x.Count() > 1) is { } dup_nggl &&
               dup_nggl.Any()) {

                // BUG randomly bookmarking duplicates the notePattern
                // i think its a virtualization thing maybe w/ the items repeater maybe 
                // foreach(Tuning tuning in all_tunings) {
                //     bool needs_update = false;
                //     foreach(var coll in tuning.Collections.Values) {
                //         if(coll.SelectMany(x => x.Patterns).GroupBy(x => x.FullName).Where(x => x.Count() > 1) is
                //                { } dup_ngl &&
                //            dup_ngl.Any()) {
                //             foreach(var dup_group_to_remove in dup_ngl) {
                //                 foreach(NotePattern dup_to_remove in dup_group_to_remove.Skip(1)) {
                //                     dup_to_remove.Parent.Patterns.Remove(dup_to_remove);
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