using System;
using System.Collections.Generic;
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
            Tunings.FirstOrDefault(x => x.IsDefault) ?? Tunings.FirstOrDefault();

        public TuningViewModel SelectedTuning {
            get => Tunings.FirstOrDefault(x => x.IsSelected);
            set {
                Tunings.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedTuning));
            }
        }

        #endregion

        #region Appearance

        public string Icon =>
            InstrumentType == InstrumentType.Guitar ? "GuitarElectric" :
            InstrumentType == InstrumentType.Ukulele ? "GuitarAcoustic" :
            InstrumentType == InstrumentType.Piano ? "Piano" :
            "Music";

        public string Name =>
            Instrument.Name;

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsKeyboard =>
            InstrumentType == InstrumentType.Piano;

        #endregion

        #region Model

        public int StringCount =>
            Instrument.StringCount;

        public int LogicalStringCount =>
            IsKeyboard ? StringCount : StringCount + 1;


        public InstrumentType InstrumentType =>
            Instrument.InstrumentType;

        public Instrument Instrument { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentViewModel(MainViewModel parent) : base(parent) {
            PropertyChanged += InstrumentViewModel_OnPropertyChanged;
            Tunings.CollectionChanged += TuningsOnCollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitAsync(Instrument instrument) {
            IsBusy = true;
            Instrument = instrument;
            Tunings.AddRange(await Task.WhenAll(Instrument.Tunings.Select(x => CreateTuningViewModelAsync(x))));
            Instrument.RefreshModelTree();
            IsBusy = false;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):

                    if(IsSelected) {
                        if(SelectedTuning == null) {
                            SelectedTuning = DefaultTuning;
                        }

                        Prefs.Instance.SelectedTuningId = SelectedTuning.Id;
                    } else {
                        Tunings.ForEach(x => x.IsSelected = false);
                    }

                    break;
                case nameof(SelectedTuning):
                    if(IsSelected) {
                        Parent.OnPropertyChanged(nameof(SelectedTuning));
                    }

                    break;
            }
        }

        void TuningsOnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
        }

        async Task<TuningViewModel> CreateTuningViewModelAsync(Tuning tuning) {
            TuningViewModel tvm = new TuningViewModel(this);
            await tvm.InitAsync(tuning);
            return tvm;
        }

        string GetDefaultTuningName() {
            string prefix = $"{Instrument.Name} Tuning #";
            int idx = 1;
            while(true) {
                string def_tuning_name = $"{prefix}{idx}";
                if(Tunings.Any(x => x.Tuning.Name == def_tuning_name)) {
                    idx++;
                    continue;
                }

                return def_tuning_name;
            }
        }

        #endregion

        #region Commands

        public ICommand RemoveTuningCommand => new MpCommand<object>(
            args => {
                if(args is not string tuning_guid ||
                   Tunings.FirstOrDefault(x => x.Tuning.Id == tuning_guid) is not { } tuning_vm_to_remove ||
                   !Instrument.Tunings.Remove(tuning_vm_to_remove.Tuning) ||
                   !Tunings.Remove(tuning_vm_to_remove)) {
                    return;
                }

                Prefs.Instance.Save();
                Debug.WriteLine($"'{tuning_vm_to_remove.Tuning.Name}' removed from {Instrument.Name}");
            });

        public ICommand AddTuningCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not object[] arg_parts ||
                   arg_parts.Length < 2 ||
                   arg_parts[0] is not IEnumerable<Note> notes ||
                   arg_parts[1] is not int capo_fret_num) {
                    return;
                }

                if(arg_parts.Length == 2 ||
                   arg_parts[2] is not string tuning_name) {
                    tuning_name = GetDefaultTuningName();
                }

                var actual_open_notes =
                    notes.Select((x,idx) => new InstrumentNote(0,idx,x.Offset(capo_fret_num)));
                Tuning new_tuning = new Tuning(tuning_name,!Tunings.Any(),false,capo_fret_num);
                new_tuning.Id = Guid.NewGuid().ToString();
                new_tuning.OpenNotes.AddRange(actual_open_notes);
                Instrument.Tunings.Add(new_tuning);

                TuningViewModel tvm = await CreateTuningViewModelAsync(new_tuning);
                Tunings.Add(tvm);

                Tunings.Last().IsSelected = true;
                Prefs.Instance.Save();
            });

        #endregion

    }
}