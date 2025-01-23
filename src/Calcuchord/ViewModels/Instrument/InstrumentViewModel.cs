using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        public TuningViewModel SelectedTuning {
            get => Tunings.FirstOrDefault(x => x.IsSelected);
            set {
                Tunings.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedTuning));
            }
        }

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsSelected {
            get => Instrument.IsSelected;
            set {
                Instrument.IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        #endregion

        #region Model

        public Instrument Instrument { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentViewModel(MainViewModel parent,Instrument it) : base(parent) {
            PropertyChanged += InstrumentViewModel_OnPropertyChanged;
            Instrument = it;
            Tunings.AddRange(Instrument.Tunings.Select(x => new TuningViewModel(this,x)));
            Instrument.RefreshModelTree();
            Tunings.CollectionChanged += TuningsOnCollectionChanged;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void InstrumentViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedTuning):
                    if(IsSelected) {
                        Parent.OnPropertyChanged(nameof(SelectedTuning));
                    }
                    break;
            }
        }

        void TuningsOnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
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

                Prefs.Instance.SyncAndSave();
                Debug.WriteLine($"'{tuning_vm_to_remove.Tuning.Name}' removed from {Instrument.Name}");
            });

        public ICommand AddTuningCommand => new MpCommand<object>(
            args => {
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
                Tuning new_tuning = new(tuning_name,!Tunings.Any(),false,capo_fret_num);
                new_tuning.OpenNotes.AddRange(actual_open_notes);
                Instrument.Tunings.Add(new_tuning);
                Tunings.Add(new(this,new_tuning));

                Tunings.Last().IsSelected = true;
                Prefs.Instance.SyncAndSave();
            });

        #endregion

    }
}