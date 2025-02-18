using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Calcuchord.JsonChords;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;

namespace Calcuchord {
    public static class DefaultDataBuilder {
        public static async Task BuildAsync() {
            var instl = new List<Instrument>();
            var itl = new[] { InstrumentType.Guitar,InstrumentType.Ukulele,InstrumentType.Piano };
            foreach(InstrumentType it in itl) {
                Instrument inst = Instrument.CreateByType(it);
                InstrumentViewModel inst_vm = new InstrumentViewModel(MainViewModel.Instance);
                await inst_vm.InitAsync(inst);
                if(inst_vm.Tunings.FirstOrDefault() is { } def_tun) {
                    inst_vm.CurGenTuning = def_tun;
                    await def_tun.InitAsync(def_tun.Tuning);

                    if(it != InstrumentType.Piano) {
                        var chords = BuildChordsFromFile(it);
                        def_tun.Tuning.Chords.Clear();
                        def_tun.Tuning.Chords.AddRange(chords);
                    }

                    if(it == InstrumentType.Guitar) {
                        inst.IsSelected = true;
                    }

                    def_tun.IsSelected = true;
                }

                instl.Add(inst);
            }

            await File.WriteAllTextAsync(
                "/home/tkefauver/dev/projects/Calcuchord/src/Calcuchord/Assets/Text/def.json",
                JsonConvert.SerializeObject(instl));

        }

        static List<PatternKeyCollection> BuildChordsFromFile(InstrumentType it) {
            Tuning tuning = Instrument.CreateByType(it).Tunings.FirstOrDefault();

            string json = MpAvFileIo.ReadTextFromResource(
                $"avares://Calcuchord/Assets/Text/{it.ToString().ToLower()}.json");
            ChordsJsonRoot chordsJsonRoot = JsonConvert.DeserializeObject<ChordsJsonRoot>(json);
            var ngcl = new List<PatternKeyCollection>();

            foreach(PropertyInfo pi in typeof(Chords).GetProperties()) {
                object obj = chordsJsonRoot.chords.GetPropertyValue(pi.Name);
                if(obj is not IList keys_obj ||
                   keys_obj.OfType<MusicKey>().FirstOrDefault() is not { } key_obj ||
                   MusicHelpers.ParseNote(key_obj.key) is not { } key_note_tup) {
                    continue;
                }

                NoteType cur_key = key_note_tup.nt;

                foreach(MusicKey chord_group in keys_obj) {
                    string cur_suffix = chord_group.suffix;
                    PatternKeyCollection ngc = new PatternKeyCollection(
                        MusicPatternType.Chords,cur_key,chord_group.suffix);
                    foreach((Position pos,int pos_num) in chord_group.positions.WithIndex()) {
                        NotePattern ng = new NotePattern(ngc,pos_num);
                        ng.CreateId(null);
                        foreach((int f,int str_num) in pos.real_frets.WithIndex()) {
                            InstrumentNote inn = null;
                            if(f < 0) {
                                inn = InstrumentNote.Mute(str_num);
                            } else if(tuning.OpenNotes[str_num].Offset(f) is { } fret_note) {
                                inn = InstrumentNote.Create(f,str_num,fret_note);
                            }

                            Debug.Assert(inn != null,"Parse error");
                            PatternNote pattern_note = PatternNote.Create(pos.fingers[str_num],inn);
                            ng.Notes.Add(pattern_note);
                        }

                        ngc.Patterns.Add(ng);
                    }

                    ngcl.Add(ngc);
                }
            }

            return ngcl;
        }
    }
}