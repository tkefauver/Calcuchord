using System;
using System.Diagnostics;

namespace Calcuchord {
    public class Logger_default : ILog {

        public void WriteLine(string message) {
            Debug.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}