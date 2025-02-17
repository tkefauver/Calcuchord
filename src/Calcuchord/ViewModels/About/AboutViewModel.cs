using System.Windows.Input;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class AboutViewModel : ViewModelBase {

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

        public string AppVersion =>
            AppBuildInfo.Version.ToString();

        #endregion

        #region Layout

        #endregion

        #region State

        #endregion

        #region Model

        BuildInfo AppBuildInfo { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public AboutViewModel() {
            AppBuildInfo = new BuildInfo();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand OpenLinkCommand => new MpCommand<object>(
            (args) => {
                if(args is not string arg_url) {
                    return;
                }

                PlatformWrapper.Services.UriNavigator.NavigateTo(arg_url);
            });

        #endregion


    }
}