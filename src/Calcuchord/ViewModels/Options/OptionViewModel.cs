using System.Windows.Input;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
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

        public string Label { get; set; }


        public string Icon { get; set; }

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsChecked { get; set; }


        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Model

        public OptionType OptionType { get; set; }

        [JsonIgnore]
        public ICommand Command { get; set; }

        [JsonIgnore]
        public object CommandParameter { get; set; }


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