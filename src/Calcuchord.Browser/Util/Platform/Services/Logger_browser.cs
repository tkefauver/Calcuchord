using System;

namespace Calcuchord.Browser {
    public class Logger_browser : ILog {
        public void WriteLine(string message) {
            JsInterop.ConsoleLog($"[{DateTime.Now}] {message}");
        }
    }

}