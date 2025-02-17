using System;
using System.Reflection;

// #if DEBUG
// [assembly: AssemblyVersion("1.0.0.0")]
// #else
// [assembly: AssemblyVersion("1.0.*")]
// #endif

/*
 [assembly: System.Reflection.AssemblyCompanyAttribute("Calcuchord")]
   [assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
   [assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.*")]
   [assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+ac162aa82187800854154a9765fe36d49da338f5")]
   [assembly: System.Reflection.AssemblyProductAttribute("Calcuchord")]
   [assembly: System.Reflection.AssemblyTitleAttribute("Calcuchord")]
   [assembly: System.Reflection.AssemblyVersionAttribute("1.0.*")]
 */

namespace Calcuchord {
    public class BuildInfo {
        // from https://stackoverflow.com/a/826850/105028
        public Version Version { get; }
        public DateTime BuildDate { get; }
        public string DisplayableVersion { get; }

        public BuildInfo() {
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            if(Version != null) {
                BuildDate = new DateTime(2000,1,1)
                    .AddDays(Version.Build)
                    .AddSeconds(Version.Revision * 2);

                DisplayableVersion = $"{Version} ({BuildDate})";
            }

        }
    }
}