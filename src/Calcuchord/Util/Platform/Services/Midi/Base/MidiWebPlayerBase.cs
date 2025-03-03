using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calcuchord {
    public abstract class MidiWebPlayerBase : MidiPlayerBase {
        protected string GetParam(IEnumerable<IEnumerable<int>> noteSets) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach(var ns in noteSets) {
                sb.Append("[");
                sb.Append(string.Join(",",ns));
                sb.Append("]");
                if(ns != noteSets.Last()) {
                    sb.Append(",");
                }
            }

            sb.Append("]");
            return sb.ToString();
        }
    }
}