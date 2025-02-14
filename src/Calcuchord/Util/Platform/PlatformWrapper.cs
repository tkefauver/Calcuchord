namespace Calcuchord {
    public static class PlatformWrapper {
        public static IPlatformServices Services { get; private set; }

        public static void Init(IPlatformServices platformServices) {
            Services = platformServices;
        }

    }
}