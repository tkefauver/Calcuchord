using System.Runtime.Serialization;
using System.Windows.Input;

namespace Calcuchord {
    [DataContract]
    public class OptionViewModel : ViewModelBase {

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

        #endregion

        #region Appearance

        [DataMember]

        public string Label { get; set; }

        [DataMember]
        public string Icon { get; set; }

        #endregion

        #region Layout

        #endregion

        #region State

        [DataMember]

        public bool IsChecked { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        #endregion

        #region Model

        [DataMember]
        public OptionType OptionType { get; set; }

        [IgnoreDataMember]
        public ICommand Command { get; set; }

        [IgnoreDataMember]
        public object CommandParameter { get; set; }

        [DataMember]
        public string OptionValue { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}