using System;

namespace Calcuchord {
    public static class PlatformWrapper {
        public static IStorageHelper StorageHelper { get; set; }
        public static IWebViewHelper WebViewHelper { get; set; }
        static Action LoadAction { get; set; }

        public static void Init(Action loadAction) {
            LoadAction = loadAction;
        }

        public static void Load() {
            LoadAction?.Invoke();
        }

    }
}