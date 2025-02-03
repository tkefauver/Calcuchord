using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

        public ObservableCollection<TuningViewModel> Tunings { get; } = [];


        public TuningViewModel DefaultTuning =>
            Tunings.FirstOrDefault();

        public TuningViewModel SelectedTuning {
            get => Tunings.FirstOrDefault(x => x.IsSelected);
            set {
                Tunings.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedTuning));
            }
        }

        #endregion

        #region Appearance

        public string SelectedTuningInfo =>
            SelectedTuning == null ? string.Empty : SelectedTuning.Name;

        public string Icon =>
            InstrumentType.ToIconName();


        public string[] FretCounts =>
            Enumerable.Range(MinEditableFretCount,(MaxEditableFretCount - MinEditableFretCount) + 1)
                .Select(x => x.ToString()).ToArray();

        public string[] StringCounts =>
            Enumerable.Range(MinEditableStringCount,(MaxEditableStringCount - MinEditableStringCount) + 1)
                .Select(x => x.ToString()).ToArray();

        #endregion

        #region Layout

        #endregion

        #region State

        public string[] InstrumentTypeNames =>
            new[] { string.Empty }
                .Union(Enum.GetNames(typeof(InstrumentType)).Where(x => x != InstrumentType.Piano.ToString()))
                .ToArray();

        public bool IsCustomSelected => SelectedInstrumentTypeIndex == InstrumentTypeNames.Length - 1;


        public int SelectedStringCountIndex { get; set; }
        public int SelectedFretCountIndex { get; set; }
        public int SelectedInstrumentTypeIndex { get; set; }

        InstrumentType? SelectedInstrumentType =>
            SelectedInstrumentTypeIndex == 0
                ? null
                : InstrumentTypeNames[SelectedInstrumentTypeIndex].ToEnum<InstrumentType>();

        public bool HasInstrumentType => SelectedInstrumentTypeIndex > 0;

        public bool IsEditModeEnabled =>
            Parent.EditModeInstrument == this;

        public bool IsActivated =>
            MainViewModel.Instance.Instruments.Contains(this);

        public bool IsSelected { get; set; }


        public bool CanEditTunings =>
            Tunings.Any();

        public bool IsKeyboard =>
            InstrumentType == InstrumentType.Piano;

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

        public string Name {
            get => Model.Name;
            set {
                if(Name != value) {
                    Model.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int RowCount =>
            Model.StringCount;

        public int LogicalStringCount =>
            IsKeyboard ? RowCount : RowCount + 1;


        public InstrumentType InstrumentType {
            get => Model.InstrumentType;
            set {
                if(InstrumentType != value) {
                    Model.InstrumentType = value;
                    OnPropertyChanged(nameof(InstrumentType));
                }
            }
        }


        public Instrument Model { get; set; }

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
            Model = instrument;
            Tunings.AddRange(await Task.WhenAll(Model.Tunings.Select(x => CreateTuningViewModelAsync(x))));
            Model.RefreshModelTree();
            IsBusy = false;
        }

        public override string ToString() {
            return Model == null ? base.ToString() : Model.ToString();
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
                case nameof(SelectedInstrumentTypeIndex):
                    if(IsEditModeEnabled) {
                        ChangeInstrumentTypeAsync(SelectedInstrumentType).FireAndForgetSafeAsync();
                    }

                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        if(SelectedTuning == null) {
                            SelectedTuning = DefaultTuning;
                        }

                        Prefs.Instance.SelectedTuningId = SelectedTuning.Id;
                        Prefs.Instance.Save();
                    } else {
                        Tunings.ForEach(x => x.IsSelected = false);
                    }

                    break;
                case nameof(SelectedTuning):
                    if(IsSelected) {
                        Parent.OnPropertyChanged(nameof(SelectedTuning));
                    }

                    OnPropertyChanged(nameof(SelectedTuningInfo));

                    break;
            }
        }

        void Tunings_OnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Tunings));
            OnPropertyChanged(nameof(CanEditTunings));

            MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.CanFinishEdit));
        }

        async Task<TuningViewModel> CreateTuningViewModelAsync(Tuning tuning) {
            TuningViewModel tvm = new TuningViewModel(this);
            await tvm.InitAsync(tuning);
            return tvm;
        }

        async Task ChangeInstrumentTypeAsync(InstrumentType? instrumentType) {
            Debug.Assert(IsEditModeEnabled,"Error, only change inst type in edit mode");
            if(instrumentType is not { } new_inst_type) {
                OnPropertyChanged(nameof(HasInstrumentType));
                return;
            }

            string new_inst_name = new_inst_type.ToString();

            bool needs_default_name =
                Model == null ||
                string.IsNullOrWhiteSpace(Name) ||
                Enum.GetNames(typeof(InstrumentType)).Any(x => Name.ToLower().StartsWith(x.ToLower()));
            if(needs_default_name) {
                // user hasn't changed name
                new_inst_name = MainViewModel.Instance.GetUniqueInstrumentName(new_inst_name,[this]);
            } else {
                // use current name
                new_inst_name = Name;
            }

            await InitAsync(Instrument.CreateByType(new_inst_type,name: new_inst_name));

            OnPropertyChanged(nameof(FretCounts));
            OnPropertyChanged(nameof(StringCounts));
            OnPropertyChanged(nameof(Name));

            SelectedFretCountIndex = FretCounts.IndexOf(Model.FretCount.ToString());
            SelectedStringCountIndex = StringCounts.IndexOf(Model.StringCount.ToString());
            OnPropertyChanged(nameof(HasInstrumentType));
            //Tunings.ForEach(x => x.OpenNotes.ForEach(y => y.OnPropertyChanged(nameof(y.MarkerDetail))));

        }

        #endregion

        #region Commands

        public ICommand RemoveTuningCommand => new MpCommand<object>(
            args => {
                if(args is not string tuning_guid ||
                   Tunings.FirstOrDefault(x => x.Tuning.Id == tuning_guid) is not { } tuning_vm_to_remove ||
                   !Model.Tunings.Remove(tuning_vm_to_remove.Tuning) ||
                   !Tunings.Remove(tuning_vm_to_remove)) {
                    return;
                }

                Prefs.Instance.Save();
                Tunings.ForEach(x => x.OnPropertyChanged(nameof(x.CanDelete)));
                Debug.WriteLine($"'{tuning_vm_to_remove.Tuning.Name}' removed from {Model.Name}");
            });

        public MpIAsyncCommand<object> AddTuningCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not Tuning new_tuning) {
                    Instrument temp_inst = Instrument.CreateByType(InstrumentType);
                    new_tuning = temp_inst.Tunings.First();
                    new_tuning.Name = GetUniqueTuningName(new_tuning.Name,[]);
                }

                new_tuning.SetParent(Model);
                Model.Tunings.Add(new_tuning);

                TuningViewModel tvm = await CreateTuningViewModelAsync(new_tuning);
                Tunings.Add(tvm);

                Tunings.Last().IsSelected = true;
            });

        #endregion

    }
}