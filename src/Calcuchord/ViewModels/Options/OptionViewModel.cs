using System.Windows.Input;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        [JsonIgnore]
        public bool IsVisible { get; set; } = true;

        [JsonIgnore]
        public bool IsSecondaryVisible =>
            IsVisible &&
            OptionType is
                OptionType.ChordSort or
                OptionType.ScaleSort or
                OptionType.ModeSort;

        [JsonProperty]
        public bool IsChecked { get; set; }

        [JsonProperty]
        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Model

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public OptionType OptionType { get; set; }

        [JsonIgnore]
        public ICommand Command { get; set; }

        [JsonIgnore]
        public object CommandParameter { get; set; }


        [JsonProperty]
        public string OptionValue { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public override string ToString() {
            return Label;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand SelectThisOptionCommand => new MpCommand(
            () => {
                MainViewModel.Instance.SelectOptionCommand.Execute(this);

            });

        #endregion

    }
}