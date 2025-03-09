using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calcuchord {
    public abstract class MidiWebPlayerBase : MidiPlayerBase {
        protected string GetParam(IEnumerable<IEnumerable<int>> noteSets) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var nsl = noteSets.ToArray();
            for(int i = 0; i < nsl.Length; i++) {
                var ns = nsl[i];
                sb.Append("[");
                sb.Append(string.Join(",",ns));
                sb.Append("]");
                if(i < nsl.Length - 1) {
                    sb.Append(",");
                }
            }

            sb.Append("]");
            return sb.ToString();
        }
    }

}