using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Calcuchord {
    public class InstrumentTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string,IDataTemplate> AvailableTemplates { get; } = new Dictionary<string,IDataTemplate>();

        public Control Build(object param) {
            if(param is not InstrumentContentFlags icf) {
                return null;
            }

            string key = null;
            if(icf.HasFlag(InstrumentContentFlags.Translate)) {
                key = "TranslateTemplate";
            } else if(icf.HasFlag(InstrumentContentFlags.Keyboard)) {
                key = "KeyboardTemplate";
            } else {
                key = "FretboardTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return true;
        }
    }
}