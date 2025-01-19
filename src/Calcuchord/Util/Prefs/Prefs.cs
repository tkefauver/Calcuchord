using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using MonkeyPaste.Common;
using ReactiveUI;

namespace Calcuchord {
    [DataContract]
    public class Prefs : ViewModelBase {
        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        [IgnoreDataMember]
        static string _prefsFilePath;

        [IgnoreDataMember]
        static string PrefsFilePath {
            get {
                var fp = "appstate.json";
                if(_prefsFilePath == null) {
                    if(Application.Current is { } ac &&
                       ac.ApplicationLifetime is ISingleViewApplicationLifetime) {
                        fp = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "appstate.json");
                    }

                    _prefsFilePath = fp;
                }

                try {
                    if(!File.Exists(_prefsFilePath)) {
                        using(File.Create(_prefsFilePath)) {
                            ;
                        }
                    }
                }
                catch(Exception ex) {
                    ex.Dump();
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
            if(Application.Current != null &&
               Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                var suspension = new AutoSuspendHelper(lifetime);
                RxApp.SuspensionHost.CreateNewAppState = () => new Prefs();
                RxApp.SuspensionHost.SetupDefaultSuspendResume(Driver);
                suspension.OnFrameworkInitializationCompleted();
            }

            // Load the saved view model state.
            _ = RxApp.SuspensionHost.GetAppState<Prefs>();
            RxApp.SuspensionHost.CreateNewAppState.Invoke();
        }

        public static Prefs Instance { get; private set; }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [DataMember]
        public string Test { get; set; }

        [DataMember]
        public ObservableCollection<string> ChordBookmarkIds { get; set; } = [];

        [DataMember]
        public ObservableCollection<string> ScaleBookmarkIds { get; set; } = [];

        [DataMember]
        public ObservableCollection<string> ModeBookmarkIds { get; set; } = [];

        [DataMember]
        public ObservableCollection<Instrument> Instruments { get; set; } = [];

        #endregion

        #region Ignored

        [IgnoreDataMember]
        Dictionary<MusicPatternType,ObservableCollection<string>> _bookmarkLookup;

        [IgnoreDataMember]
        public Dictionary<MusicPatternType,ObservableCollection<string>> BookmarkLookup {
            get {
                if(_bookmarkLookup == null) {
                    _bookmarkLookup = new() {
                        { MusicPatternType.Chords,ChordBookmarkIds },
                        { MusicPatternType.Scales,ScaleBookmarkIds },
                        { MusicPatternType.Modes,ModeBookmarkIds }
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
            Instruments.CollectionChanged += Coll_OnCollectionChanged;
            ChordBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
            ScaleBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;
            ModeBookmarkIds.CollectionChanged += Coll_OnCollectionChanged;

        }

        #endregion

        #region Public Methods

        public void Save() {
            Driver?.SaveState(this);
            Debug.WriteLine($"{DateTime.Now} prefs saved");
        }

        #endregion

        #region Protected Variables

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