using _Microsoft.Android.Resource.Designer;
using Android.Content;

namespace Calcuchord.Android {
    public class PlatformInfo_ad : IPlatformInfo {
        public PlatformInfo_ad(Context context) {
            if(context is not MainActivity activity ||
               activity.Resources is not { } res) {
                return;
            }

            IsTablet = res.GetBoolean(ResourceConstant.Boolean.isTablet);
        }

        public bool IsMobile => true;
        public bool IsTablet { get; }
    }
}