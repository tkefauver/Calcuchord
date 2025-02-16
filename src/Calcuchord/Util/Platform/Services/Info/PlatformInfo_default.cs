using System;

namespace Calcuchord {
    public class PlatformInfo_default : IPlatformInfo {

        public virtual bool IsMobile => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        public virtual bool IsTablet => false;
    }
}