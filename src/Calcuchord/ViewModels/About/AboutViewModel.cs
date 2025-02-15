using System;
using System.Reflection;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class AboutViewModel : ViewModelBase {
        public string AppName => "Calcuchord";

        public string AppVersion {
            get {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                if(version != null) {
                    return version.ToString();
                }

                return string.Empty;
            }
        }


        public ICommand OpenLinkCommand => new MpCommand<object>(
            (args) => {
                if(args is not string arg_url ||
                   !Uri.TryCreate(arg_url,UriKind.Absolute,out Uri arg_uri)) {
                    return;
                }

                arg_uri.OpenInBrowser();
            });
    }
}