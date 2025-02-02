using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Calcuchord {
    public class InstrumentPopupTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string,IDataTemplate> AvailableTemplates { get; } = new Dictionary<string,IDataTemplate>();

        public Control Build(object param) {
            string key = null;
            if(param is TuningViewModel ivm) {
                key = "TuningEditorTemplate";
            } else if(param is InstrumentViewModel invm) {
                key = "InstrumentEditorTemplate";
            } else {
                return null;
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return true;
        }
    }
}