namespace Calcuchord.Browser {
    public class PlatformInfo_browser : IPlatformInfo {

        bool? _isMobile;

        public bool IsMobile {
            get {
                if(_isMobile == null) {
                    _isMobile = JsInterop.IsMobile();
                }

                return _isMobile.Value;
            }
        }

        bool? _isTablet;

        public bool IsTablet {
            get {
                if(_isTablet == null) {
                    _isTablet = JsInterop.IsTablet();
                }

                return _isTablet.Value;
            }
        }
    }
}