using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public partial class Prefs : ViewModelBase {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        static bool IsParsing { get; set; }

        public static Prefs Parse(string prefsJson) {
            IsParsing = true;
            Prefs result = JsonConvert.DeserializeObject<Prefs>(prefsJson);
            IsParsing = false;
            return result;
        }

        static void DoModelUpToDateCheck() {
            try {
                string backup_prefs_json =
                    MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/appstate_022325.json");
                Prefs ValidationChecker = Parse(backup_prefs_json);
                Instance = null;
            } catch(Exception ex) {
                ex.Dump();
                // Did you refactor part of the model? probably should put it back...or will need to catch 
                // and transform it
                Debugger.Break();
            }
        }

        public static bool IsLoaded { get; private set; }

        public static Prefs Instance { get; private set; }

        public static async Task InitAsync() {
            if(PlatformWrapper.Services is not { } ps ||
               ps.PrefsIo is not { } prefsIo) {
                return;
            }

            if(RESET_PREFS) {
                await prefsIo.WritePrefsAsync(string.Empty);
            }
#if DEBUG
            DoModelUpToDateCheck();
#endif

            string prefs_json = await prefsIo.ReadPrefsAsync();

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
                    await prefsIo.WritePrefsAsync(string.Empty);
                    Instance = null;
                    InitAsync().FireAndForgetSafeAsync();
                    return;
                }
            }

            Instance.IsInitialStartup = is_initial_startup;
            Instance.LastPrefsVersion = Instance.PrefsVersion;
            Instance.WasOptionsOutOfDateOnStartup =
                Instance.PrefsVersion.ToVersion() < Instance.LastOptionsUpdatedPrefsVersion;

            Instance.IsOptionsRequireReset =
                Instance.PrefsVersion.ToVersion() < Instance.ResetRequiredPrefsVersion;
            IsLoaded = true;


        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public bool IsExactMatchOnly { get; set; }

        [JsonProperty]
        public string PrefsVersion { get; set; } = string.Empty;

        [JsonProperty]
        public bool IsThemeDark { get; set; }

        [JsonProperty]
        public int MatchColCount { get; set; } = MainViewModel.DEFAULT_MATCH_COL_COUNT;


        [JsonProperty]
        public List<Instrument> Instruments { get; set; } = [];

        [JsonProperty]
        public List<OptionViewModel> Options { get; set; } = [];

        [JsonProperty]
        public List<BookmarkGroup> BookmarkGroups { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        public bool WasOptionsOutOfDateOnStartup { get; private set; }

        [JsonIgnore]
        public bool IsOptionsRequireReset { get; private set; }


        [JsonIgnore]
        public string LastPrefsVersion { get; private set; } = string.Empty;

        [JsonIgnore]
        BuildInfo RuntimeBuildInfo { get; } = new BuildInfo();

        [JsonIgnore]
        public Version LastOptionsUpdatedPrefsVersion { get; } = new Version("1.0.9214.18195");

        [JsonIgnore]
        Version ResetRequiredPrefsVersion { get; } = new Version("1.0.9195.22711");

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
                if(!IsParsing) {
                    // singleton error
                    Debugger.Break();

                    PlatformWrapper.Services.Logger.WriteLine("singleton error");
                }

                return;
            }

            Instance = this;

        }

        #endregion

        #region Public Methods

        DateTime LastSaveCalledDt { get; set; }

        public void Save() {
            DateTime call_time = DateTime.Now;
            LastSaveCalledDt = call_time;
            Task.Run(
                async () => {
                    if(PlatformWrapper.Services is not { } ps ||
                       ps.PrefsIo is not { } prefsIo) {
                        PlatformWrapper.Services.Logger.WriteLine("prefs io service unavailable");
                        return;
                    }

                    if(IsSaveIgnored) {
                        PlatformWrapper.Services.Logger.WriteLine("prefs save ignored");
                        return;
                    }

                    while(true) {
                        if(LastSaveCalledDt != call_time) {
                            // ignore since called again
                            return;
                        }

                        if(DateTime.Now - call_time > TimeSpan.FromSeconds(10)) {
                            break;
                        }

                        await Task.Delay(1000);
                    }

                    SyncModels();
#if DEBUG
                    Validate();
#endif
                    try {
                        string pref_json = JsonConvert.SerializeObject(this);
                        prefsIo.WritePrefsAsync(pref_json).FireAndForgetSafeAsync();
                        PlatformWrapper.Services.Logger.WriteLine("Prefs SAVED");
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
                Instruments = mvm.Instruments.Select(x => x.Instrument).ToList();
                BookmarkGroups = mvm.Instruments.SelectMany(x => x.Tunings).SelectMany(x => x.BookmarkGroups)
                    .Where(x => !x.IsAddGroupPlaceholder)
                    .Select(x => x.BookmarkGroup).ToList();
                Options = mvm.OptionLookup.Values.SelectMany(x => x).ToList();
                MatchColCount = mvm.MatchColCount;
                IsExactMatchOnly = mvm.IsExactMatchOnly;
                PrefsVersion = RuntimeBuildInfo.Version.ToString();
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

            if(Instruments.SelectMany(x => x.Tunings).SelectMany(x => x.Collections.Values).SelectMany(x => x)
                   .SelectMany(x => x.Patterns) is { } all_ngl &&
               all_ngl.GroupBy(x => x.Id).Where(x => x.Count() > 1) is { } dup_nggl &&
               dup_nggl.Any()) {
                Debugger.Break();

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