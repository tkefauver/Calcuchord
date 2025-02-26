namespace Calcuchord.Mac {

    public class PlatformServices_mac : PlatformServies_default {
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_mac();
    }
}