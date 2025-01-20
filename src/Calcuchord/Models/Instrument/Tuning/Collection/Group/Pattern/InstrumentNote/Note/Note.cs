using System;
using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class Note {
        #region Private Variables

        #endregion

        #region Constants

        const int MAX_NOTE_TYPE = 12;

        #endregion

        #region Statics

        public static Note Parse(string text) {
            if(MusicHelpers.ParseNote(text) is not { } note_tup ||
               note_tup.register is not { } reg) {
                return null;
            }

            return new(note_tup.nt,reg);
        }

        static int GetId(Note nt) {
            return (int)nt.Key + (nt.Register * MAX_NOTE_TYPE);
        }

        static Note FromId(int id) {
            var register = (int)Math.Floor(id / (double)MAX_NOTE_TYPE);
            var nt = (NoteType)(id - (register * MAX_NOTE_TYPE));
            return new(nt,register);
        }

        #endregion

        #region Properties

        #region Members

        [DataMember]
        public NoteType Key { get; set; }

        [DataMember]
        public int Register { get; set; }

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public virtual string Name =>
            Key.ToDisplayValue();

        [IgnoreDataMember]
        public virtual string FullName =>
            Key.ToDisplayValue(Register);

        [IgnoreDataMember]
        int _id = -1;

        [IgnoreDataMember]
        public int NoteId {
            get {
                if(_id < 0) {
                    _id = GetId(this);
                }

                return _id;
            }
        }

        [IgnoreDataMember]
        int _midiTone = -1;

        [IgnoreDataMember]
        public int MidiTone {
            get {
                if(_midiTone < 0) {

                    //   Tone | id
                    // E2 40 | 28
                    //
                    // C3 48 | 36
                    // E3 52
                    // C0 24
                    _midiTone = NoteId + 12;

                }

                return _midiTone;
            }
        }

        [IgnoreDataMember]
        public Note Next =>
            FromId(NoteId + 1);

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
        }

        #endregion

        #region Public Methods

        public override string ToString() {
            return FullName;
        }

        public Note Clone() {
            return new(Key,Register);
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion
    }
}