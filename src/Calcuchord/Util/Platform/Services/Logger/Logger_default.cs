using System;
using System.Diagnostics;
using System.IO;

namespace Calcuchord {
    public class Logger_default : ILog {
        public string LogPath { get; set; }

        public void WriteLine(string message) {
            string result = $"[{DateTime.Now}] {message}";
#if DEBUG
            Debug.WriteLine(result);
#else
            Console.WriteLine(result);
#endif
            if(LogPath != null) {
                File.AppendAllText(LogPath,Environment.NewLine + result);
            }
        }
    }
}