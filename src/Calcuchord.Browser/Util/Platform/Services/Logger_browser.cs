using System;

namespace Calcuchord.Browser {
    public class Logger_browser : ILog {
        public string LogPath { get; set; }

        public void WriteLine(string message) {
            JsInterop.ConsoleLog($"[{DateTime.Now}] {message}");
        }
    }

}