using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Calcuchord {
    public class InstrumentTuningTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string,IDataTemplate> AvailableTemplates { get; } = new Dictionary<string,IDataTemplate>();

        public Control Build(object param) {
            string key = null;
            if(param is TuningViewModel ivm) {
                key = ivm.Parent.IsKeyboard ? "KeyboardViewTemplate" : "FretboardViewTemplate";
            } else {
                key = "EmptyTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return true; //data is TuningViewModel;
        }
    }
}