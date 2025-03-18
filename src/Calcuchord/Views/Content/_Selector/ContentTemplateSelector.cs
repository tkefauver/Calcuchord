using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Calcuchord {
    public class ContentTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string,IDataTemplate> AvailableTemplates { get; } = new Dictionary<string,IDataTemplate>();

        public Control Build(object param) {
            string key = "DefaultTemplate";
            if(param is MainContentFlags mcf) {
                if(mcf.HasFlag(MainContentFlags.Search) || mcf.HasFlag(MainContentFlags.Translate)) {
                    if(mcf.HasFlag(MainContentFlags.Landscape)) {
                        key = "SearchLandscapeTemplate";
                    } else {
                        key = "SearchTemplate";
                    }
                }
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return true;
        }
    }
}