using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Calcuchord {
    [JsonObject]
    public class Note : ModelBase {

        #region Private Variables

        #endregion

        #region Constants

        const int MAX_NOTE_TYPE = 12;
        public const int MAX_REGISTER = 8;

        public const int MIN_NOTE_ID = 0;
        public const int MAX_NOTE_ID = MAX_NOTE_TYPE * MAX_REGISTER;

        #endregion

        #region Statics

        public static Note Parse(string text) {
            if(MusicHelpers.ParseNote(text) is not { } note_tup ||
               note_tup.register is not { } reg) {
                return null;
            }

            return new(note_tup.nt,reg);
        }

        public static int GetId(Note nt) {
            return nt.IsMute ? int.MaxValue : (int)nt.Key + (nt.Register * MAX_NOTE_TYPE);
        }

        static Note GetNote(int id) {
            if(id == int.MaxValue) {
                return new(null,null);
            }

            int register = (int)Math.Floor(id / (double)MAX_NOTE_TYPE);
            NoteType nt = (NoteType)(id - (register * MAX_NOTE_TYPE));
            return new(nt,register);
        }

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public NoteType Key { get; set; }

        [JsonProperty]
        public int Register { get; set; }

        [JsonProperty]
        public int NoteId { get; set; }

        [JsonProperty]
        public bool IsMute { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public bool IsAltered =>
            Key == NoteType.Db ||
            Key == NoteType.Eb ||
            Key == NoteType.Gb ||
            Key == NoteType.Ab ||
            Key == NoteType.Bb;

        [JsonIgnore]
        public virtual string Name =>
            Key.ToDisplayValue();

        [JsonIgnore]
        public virtual string FullName =>
            Key.ToDisplayValue(Register);

        // [JsonIgnore]
        // int? _id = null;
        //
        // [JsonIgnore]
        // public int NoteId {
        //     get {
        //         if(_id is null) {
        //             _id = GetId(this);
        //         }
        //
        //         return _id.Value;
        //     }
        // }

        [JsonIgnore]
        int _midiTone = -1;

        [JsonIgnore]
        public int MidiTone {
            get {
                if(_midiTone < 0) {
                    //   Tone | id
                    // E2 40 | 28
                    //
                    // C3 48 | 36
                    // E3 52
                    // C0 24
                    // C C# D D# E F F# G G# A A# B
                    //  D: D3 A3 D3 F#4
                    // Id: 37 44 37 
                    _midiTone = IsMute ? 0 : NoteId + 12;
                }

                return _midiTone;
            }
        }

        [JsonIgnore]
        public Note Next =>
            IsMute ? null : GetNote(NoteId + 1);

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public Note() {
        }

        public Note(NoteType nt,int register) {
            Key = nt;
            Register = register;
            NoteId = GetId(this);
        }

        public Note(NoteType? nt,int? register) {
            if(nt.HasValue && register.HasValue) {
                Key = nt.Value;
                Register = register.Value;
            } else {
                IsMute = true;
            }

            NoteId = GetId(this);
        }

        #endregion

        #region Public Methods

        public virtual void Adjust(int offset) {
            if(Offset(offset) is not { } adj_note) {
                return;
            }

            NoteId = adj_note.NoteId;
            _midiTone = adj_note.MidiTone;
            Key = adj_note.Key;
            Register = adj_note.Register;
            IsMute = adj_note.IsMute;
        }

        public Note Offset(int offset) {
            if(IsMute) {
                return null;
            }

            return GetNote(Math.Max(0,NoteId + offset));
        }

        public override string ToString() {
            return FullName;
        }

        public Note Clone() {
            return new(Key,Register);
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}