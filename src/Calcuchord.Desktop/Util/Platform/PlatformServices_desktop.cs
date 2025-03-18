namespace Calcuchord.Desktop {

    public class PlatformServices_desktop : PlatformServies_default {
        //public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_fluid_proc_desktop();
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_fluid_lib_desktop();
    }
}