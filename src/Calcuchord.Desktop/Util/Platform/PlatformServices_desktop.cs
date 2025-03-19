using System;

namespace Calcuchord.Desktop {

    public class PlatformServices_desktop : PlatformServies_default {
        public PlatformServices_desktop()
        {
            if(OperatingSystem.IsWindows())
            {
                MidiPlayer = new MidiPlayer_win_desktop();
            } else
            {
                MidiPlayer = new MidiPlayer_fluid_lib_desktop();
            }
        }
        //public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_fluid_proc_desktop();
        public override IMidiPlayer MidiPlayer { get; }
    }
}