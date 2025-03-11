using System.IO;

namespace Calcuchord {
    public abstract class MidiSoundFontPlayerBase : MidiPlayerBase {
        protected string MidiFilePath =>
            Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"output.mid");

        public string SoundFontDir =>
            Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"sound");


        protected string GetInstrumentSoundFontPath(Note note) {
            string sounds_dir = SoundFontDir;
            string fn = "guitar.sf2";
            if(MainViewModel.Instance is { } mvm &&
               mvm.SelectedInstrument is { } si &&
               si.IsKeyboard) {
                fn = "piano.sf2";
            }

            return Path.Combine(sounds_dir,fn);
        }
    }
}