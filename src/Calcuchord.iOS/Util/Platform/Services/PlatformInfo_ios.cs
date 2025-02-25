namespace Calcuchord.iOS {
    public class PlatformInfo_ios : IPlatformInfo {
        public PlatformInfo_ios() {

            IsTablet = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
        }

        public bool IsMobile => true;
        public bool IsTablet { get; }
    }
}