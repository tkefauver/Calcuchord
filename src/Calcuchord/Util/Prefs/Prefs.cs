using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Calcuchord {
    [DataContract]
    public class Prefs : ViewModelBase {

        #region Private Variables

        [IgnoreDataMember]
        bool _isSavingIgnored;

        #endregion

        #region Constants

        #endregion

        #region Statics

        [IgnoreDataMember]
        static string _prefsFilePath;

        [IgnoreDataMember]
        static string PrefsFilePath {
            get {
                string fp = "appstate.json";
                if(_prefsFilePath == null) {
                    if(Application.Current is { } ac &&
                       ac.ApplicationLifetime is ISingleViewApplicationLifetime) {
                        fp = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "appstate.json");
                    }

                    _prefsFilePath = fp;
                }

                return _prefsFilePath;
            }
        }

        [IgnoreDataMember]
        static ISuspensionDriver _driver;

        [IgnoreDataMember]
        public static ISuspensionDriver Driver {
            get {
                if(_driver == null) {
                    _driver = new NewtonsoftJsonSuspensionDriver(PrefsFilePath);
                }

                return _driver;
            }
        }

        public static void Init() {
            //File.Delete(PrefsFilePath);
            bool is_initial_startup = !File.Exists(PrefsFilePath);

            if(Application.Current != null &&
               Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                AutoSuspendHelper suspension = new AutoSuspendHelper(lifetime);
                RxApp.SuspensionHost.CreateNewAppState = () => new Prefs();
                RxApp.SuspensionHost.SetupDefaultSuspendResume(Driver);
                suspension.OnFrameworkInitializationCompleted();
            }

            // Load the saved view model state.
            if(Design.IsDesignMode) {
                _ = new Prefs();
            } else {
                _ = RxApp.SuspensionHost.GetAppState<Prefs>();
                if(is_initial_startup) {
                    //RxApp.SuspensionHost.CreateNewAppState.Invoke();
                    _ = new Prefs();
                }
                //
            }
        }

        public static Prefs Instance { get; private set; }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [DataMember]
        string _selectedTuningId;

        public string SelectedTuningId {
            get => _selectedTuningId;
            set {
                if(SelectedTuningId != value) {
                    _selectedTuningId = value;
                    OnPropertyChanged(nameof(SelectedTuningId));
                }
            }
        }

        [DataMember]
        public SvgFlags SelectedSvgFlags { get; set; } = SvgBuilderBase.DefaultSvgFlags;

        [DataMember]
        public bool IsThemeDark { get; set; }

        [DataMember]
        public ObservableCollection<string> ChordBookmarkIds { get; set; } = [];

        [DataMember]
        public ObservableCollection<string> ScaleBookmarkIds { get; set; } = [];

        // [DataMember]
        // public ObservableCollection<string> ModeBookmarkIds { get; set; } = [];

        [DataMember]
        public ObservableCollection<Instrument> Instruments { get; set; } = [];

        [DataMember]
        public ObservableCollection<OptionViewModel> Options { get; set; } = [];

        #endregion

        #region Ignored

        [IgnoreDataMember]
        Dictionary<MusicPatternType,ObservableCollection<string>> _bookmarkLookup;

        [IgnoreDataMember]
        public Dictionary<MusicPatternType,ObservableCollection<string>> BookmarkLookup {
            get {
                if(_bookmarkLookup == null) {
                    _bookmarkLookup = new()
                    {
                        { MusicPatternType.Chords,ChordBookmarkIds },
                        { MusicPatternType.Scales,ScaleBookmarkIds }
                        //{ MusicPatternType.Modes,ModeBookmarkIds }
                    };
                }

                return _bookmarkLookup;
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
                return;
            }

            Instance = this;

            PropertyChanged += PropertyChanged_OnPropertyChanged;
            // Instruments.CollectionChanged += Coll_OnCollectionChanged;
            // ChordBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
            // ScaleBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
        }

        #endregion

        #region Public Methods

        public void Save() {
            if(_isSavingIgnored) {
                Debug.WriteLine("Save ignored");
                return;
            }

            if(Driver == null) {
            }

            Driver?.SaveState(this);
            Debug.WriteLine("");
            Debug.WriteLine($"{DateTime.Now} prefs saved. SelectedTuningId: {SelectedTuningId}");
            foreach(Instrument inst in Instruments) {
                foreach(Tuning tuning in inst.Tunings) {
                    Debug.WriteLine(
                        $"{inst} Chords: {tuning.Chords.SelectMany(x => x.Groups).Count()} Scales: {tuning.Scales.SelectMany(x => x.Groups).Count()} Modes: {tuning.Modes.SelectMany(x => x.Groups).Count()}");
                }
            }

            Debug.WriteLine("");
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