using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Calcuchord {
    public class DrawerPageTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string,IDataTemplate> AvailableTemplates { get; } = new Dictionary<string,IDataTemplate>();

        public Control Build(object param) {
            string key = null;
            DrawerPageType dpt = (DrawerPageType)param;
            if(dpt == DrawerPageType.Main) {
                key = "MainPageTemplate";
            } else if(dpt == DrawerPageType.Options) {
                key = "OptionsPageTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return true;
        }
    }
}