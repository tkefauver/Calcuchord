using System.IO;

namespace Calcuchord;

public abstract class MidiSoundFontPlayerBase : MidiPlayerBase {
    protected string MidiFilePath =>
        Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"output.mid");
    public string SoundFontDir =>
        Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"sound");


    protected string GetInstrumentSoundFontPath(Note note) {
        string sounds_dir = SoundFontDir;
        string fn = "guitar.sf2";
        if(note is PatternNote pn &&
           pn.Parent is { } ng &&
           ng.Parent is { } ngc &&
           ngc.Parent is { } tuning &&
           tuning.Parent is { } inst) {
            if(inst.InstrumentType == InstrumentType.Piano) {
                fn = "piano.sf2";
            }
        }

        return Path.Combine(sounds_dir,fn);
    }
}