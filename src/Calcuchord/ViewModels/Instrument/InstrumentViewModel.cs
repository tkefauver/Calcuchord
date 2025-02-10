using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class InstrumentViewModel : ViewModelBase<MainViewModel> {

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

        public TuningViewModel CurGenTuning { get; set; }

        public ObservableCollection<TuningViewModel> Tunings { get; } = [];


        public TuningViewModel DefaultTuning =>
            Tunings.FirstOrDefault();

        public TuningViewModel SelectedTuning {
            get => Tunings.FirstOrDefault(x => x.IsSelected);
            set {
                if(SelectedTuning != value) {
                    Tunings.ForEach(x => x.IsSelected = x == value);
                    OnPropertyChanged(nameof(SelectedTuning));
                }
            }
        }

        #endregion

        #region Appearance

        public string Icon =>
            InstrumentNameToSvgConverter.Instance.Convert(
                InstrumentType.ToString(),typeof(string),null,CultureInfo.CurrentCulture) as string;


        public string[] FretCounts =>
            Enumerable.Range(
                    MinEditableFretCount,
                    (MaxEditableFretCount - MinEditableFretCount) + 1)
                .Select(x => x.ToString()).ToArray();

        public string[] StringCounts =>
            Enumerable.Range(
                    MinEditableStringCount,
                    (MaxEditableStringCount - MinEditableStringCount) + 1)
                .Select(x => x.ToString()).ToArray();

        #endregion

        #region Layout

        #endregion

        #region State

        public bool CanEdit => true; //!IsKeyboard;

        public bool IsInstrumentTabSelected { get; set; }
        public bool IsTuningTabSelected { get; set; }

        public string[] InstrumentTypeNames =>
            Enum.GetNames(typeof(InstrumentType))
                //.Where(x => x != InstrumentType.Piano.ToString())
                .ToArray();

        public int SelectedStringCountIndex { get; set; }
        public int SelectedFretCountIndex { get; set; }

        int LastSelectedInstrumentTypeIndex { get; set; } = -1;

        public int SelectedInstrumentTypeIndex {
            get => InstrumentTypeNames.IndexOf(InstrumentType.ToString());
            set {
                if(SelectedInstrumentTypeIndex != value) {
                    InstrumentType = InstrumentTypeNames[value].ToEnum<InstrumentType>();
                    OnPropertyChanged(nameof(SelectedInstrumentTypeIndex));

                }
            }
        }

        public bool CanChangeStringCount =>
            !IsActivated &&
            Tunings.Count <= 1;

        public bool IsEditModeEnabled =>
            Parent.EditModeInstrument == this;

        public bool IsActivated =>
            MainViewModel.Instance.Instruments.Contains(this);


        public bool CanEditTunings =>
            Tunings.Any();

        public bool IsKeyboard =>
            InstrumentType == InstrumentType.Piano;

        bool IsInstrumentTypeChanging { get; set; }

        #endregion

        #region Model

        #region Validation

        public string InvalidText { get; set; }

        public bool IsValid =>
            string.IsNullOrEmpty(InvalidText);

        public int MinEditableStringCount => 3;
        public int MaxEditableStringCount => 7;
        public int MinEditableFretCount => 5;
        public int MaxEditableFretCount => 36;

        #endregion

        public bool IsSelected {
            get => Instrument.IsSelected;
            set {
                if(IsSelected != value) {
                    Instrument.IsSelected = value;

                    if(IsSelected) {
                        HasModelChanged = true;
                    }

                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string Name {
            get => Instrument.Name;
            set {
                if(Name != value) {
                    Instrument.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int RowCount =>
            Instrument.StringCount;

        public int VisualRowCount =>
            IsKeyboard ? RowCount : RowCount + 1;


        public InstrumentType InstrumentType {
            get => Instrument.InstrumentType;
            set {
                if(InstrumentType != value) {
                    Instrument.InstrumentType = value;
                    OnPropertyChanged(nameof(InstrumentType));
                }
            }
        }


        public Instrument Instrument { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentViewModel(MainViewModel parent) : base(parent) {
            PropertyChanged += InstrumentViewModel_OnPropertyChanged;
            Tunings.CollectionChanged += Tunings_OnCollectionChanged;
        }


        protected InstrumentViewModel() {
        }

        #endregion

        #region Public Methods

        public async Task InitAsync(Instrument instrument) {
            IsBusy = true;
            Instrument = instrument;
            Tunings.Clear();
            Tunings.AddRange(await Task.WhenAll(Instrument.Tunings.Select(x => CreateTuningViewModelAsync(x))));
            Instrument.RefreshModelTree();

            IsBusy = false;
        }

        public override string ToString() {
            return Instrument == null ? base.ToString() : Instrument.ToString();
        }

        public string GetUniqueTuningName(string desiredName,TuningViewModel[] ignored) {
            string unique_name = desiredName;
            var other_tl = Tunings.Where(x => !ignored.Contains(x));

            int suffix = 1;
            while(other_tl.Any(x => x.Name.ToLower() == unique_name.ToLower())) {
                unique_name = $"{desiredName}{suffix++}";
            }

            return unique_name;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsEditModeEnabled):
                    if(IsEditModeEnabled) {
                        UpdateEditorSelectionToType();
                        // BUG sel tuning checkbox not always working
                        Tunings.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    }

                    break;
                case nameof(SelectedInstrumentTypeIndex):
                    if(SelectedInstrumentTypeIndex == LastSelectedInstrumentTypeIndex) {
                        break;
                    }

                    LastSelectedInstrumentTypeIndex = SelectedInstrumentTypeIndex;
                    if(IsEditModeEnabled) {
                        ChangeInstrumentTypeAsync().FireAndForgetSafeAsync();
                    }

                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        if(SelectedTuning == null) {
                            SelectedTuning = DefaultTuning;
                        }

                        if(Parent.SelectedInstrument != this) {
                            Parent.SelectedInstrument = this;
                        }
                    }

                    break;
                case nameof(SelectedTuning):
                    Tunings.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

                    if(IsSelected) {
                        Parent.OnPropertyChanged(nameof(SelectedTuning));
                    }

                    break;
                case nameof(SelectedStringCountIndex):
                    if(SelectedStringCountIndex < 0 ||
                       IsInstrumentTypeChanging) {
                        break;
                    }

                    if(int.TryParse(
                           StringCounts[SelectedStringCountIndex],
                           out int new_str_count) &&
                       Instrument.StringCount != new_str_count) {
                        ChangeStringCount(new_str_count);
                    }

                    break;
                case nameof(SelectedFretCountIndex):
                    if(SelectedFretCountIndex < 0 ||
                       IsInstrumentTypeChanging) {
                        break;
                    }

                    if(int.TryParse(
                           FretCounts[SelectedFretCountIndex],
                           out int new_fret_count) &&
                       Instrument.FretCount != new_fret_count) {
                        Instrument.FretCount = new_fret_count;
                        InitAsync(Instrument).FireAndForgetSafeAsync();
                    }

                    break;
            }
        }

        void Tunings_OnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Tunings));
            OnPropertyChanged(nameof(CanEditTunings));

            MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.CanFinishEdit));
        }

        void UpdateEditorSelectionToType() {
            SelectedFretCountIndex = FretCounts.IndexOf(Instrument.FretCount.ToString());
            SelectedStringCountIndex = StringCounts.IndexOf(Instrument.StringCount.ToString());
        }

        async Task<TuningViewModel> CreateTuningViewModelAsync(Tuning tuning) {
            TuningViewModel tvm = new TuningViewModel(this);
            await tvm.InitAsync(tuning);
            return tvm;
        }

        async Task ChangeInstrumentTypeAsync() {
            IsInstrumentTypeChanging = true;

            Debug.Assert(
                IsEditModeEnabled,
                "Error, only change inst type in edit mode");

            string new_inst_name = InstrumentType.ToString();

            bool needs_default_name =
                Instrument == null ||
                string.IsNullOrWhiteSpace(Name) ||
                Enum.GetNames(typeof(InstrumentType)).Any(x => Name.ToLower().StartsWith(x.ToLower()));
            if(needs_default_name) {
                // user hasn't changed name
                new_inst_name = MainViewModel.Instance.GetUniqueInstrumentName(
                    new_inst_name,
                    [this]);
            } else {
                // use current name
                new_inst_name = Name;
            }

            await InitAsync(
                Instrument.CreateByType(
                    InstrumentType,
                    name: new_inst_name));

            SelectedTuning = Tunings.FirstOrDefault();

            OnPropertyChanged(nameof(FretCounts));
            OnPropertyChanged(nameof(StringCounts));
            OnPropertyChanged(nameof(Name));

            // BUG init will end up using previous string counts
            // so block changes until complete 
            IsInstrumentTypeChanging = false;
            UpdateEditorSelectionToType();
        }

        void ChangeStringCount(int newStrCount) {
            Debug.Assert(
                Instrument.Tunings.Count == 1,
                "Should have only ONE tuning");
            Debug.Assert(
                !IsActivated,
                "No string count changes for existing instruments");

            int diff = newStrCount - Instrument.StringCount;

            var open_notes = Instrument.Tunings.FirstOrDefault()?.OpenNotes;
            if(diff > 0) {
                // completely reset strings
                open_notes =
                    "D2 E2 A2 D3 G3 B3 E4"
                        .Split(" ")
                        .Take(newStrCount)
                        .Select(
                            (x,idx) => new InstrumentNote(
                                0,
                                idx,
                                Note.Parse(x))).ToList();
            } else {
                // trim highest
                open_notes = open_notes.Take(newStrCount).ToList();
            }

            Instrument.StringCount = newStrCount;
            Instrument.Tunings.ForEach(x => x.OpenNotes = open_notes.ToList());
            InitAsync(Instrument).FireAndForgetSafeAsync();
        }

        #endregion

        #region Commands

        public ICommand RemoveTuningCommand => new MpCommand<object>(
            args => {
                if(args is not TuningViewModel tuning_vm_to_remove) {
                    return;
                }

                int to_remove_idx = Tunings.IndexOf(tuning_vm_to_remove);
                Instrument.Tunings.Remove(tuning_vm_to_remove.Tuning);
                Tunings.Remove(tuning_vm_to_remove);

                int to_sel_idx = to_remove_idx >= Tunings.Count ? to_remove_idx - 1 : to_remove_idx;
                SelectedTuning = Tunings[to_sel_idx];
                Tunings.ForEach(x => x.OnPropertyChanged(nameof(x.CanDelete)));

                Prefs.Instance.Save();

                Debug.WriteLine($"'{tuning_vm_to_remove.Tuning.Name}' removed from {Instrument.Name}");
            });

        public MpIAsyncCommand<object> AddTuningCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not Tuning new_tuning) {
                    Instrument temp_inst = Instrument.CreateByType(InstrumentType);
                    new_tuning = temp_inst.Tunings.First();
                    new_tuning.Name = GetUniqueTuningName(
                        new_tuning.Name,
                        []);
                }

                new_tuning.SetParent(Instrument);
                Instrument.Tunings.Add(new_tuning);

                TuningViewModel tvm = await CreateTuningViewModelAsync(new_tuning);
                Tunings.Add(tvm);

                SelectedTuning = Tunings.Last();
                if(IsEditModeEnabled) {
                    SelectedTuning.IsExpanded = true;
                }
            });

        #endregion

    }
}