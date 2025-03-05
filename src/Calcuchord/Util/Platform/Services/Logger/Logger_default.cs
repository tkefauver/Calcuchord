using System;
using System.Diagnostics;

namespace Calcuchord {
    public class Logger_default : ILog {

        public void WriteLine(string message) {
            string result = $"[{DateTime.Now}] {message}";
#if DEBUG
            Debug.WriteLine(result);
#else 
            Console.WriteLine(result);
            #endif
        }
    }
}