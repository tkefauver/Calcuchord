using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class NotePattern : PrimaryModelBase {

        #region Properties

        #region Members

        [JsonProperty]
        public int SubPosition { get; set; }

        [JsonProperty]
        public int Position { get; set; }

        [JsonProperty]
        public List<PatternNote> Notes { get; set; } = [];

        [JsonProperty]
        public bool IsBookmarked { get; set; }

        [JsonProperty]
        public string BookmarkGroupIdsCsv { get; set; } = string.Empty;

        #endregion

        #region Ignored

        [JsonIgnore]
        List<string> _bookmarkGroupIds;

        [JsonIgnore]
        List<string> BookmarkGroupIds {
            get {
                if(_bookmarkGroupIds == null) {
                    _bookmarkGroupIds = BookmarkGroupIdsCsv.Split(',').ToList();
                }

                return _bookmarkGroupIds;
            }
        }

        [JsonIgnore]
        public MusicPatternType PatternType =>
            Parent.PatternType;

        [JsonIgnore]
        public string SuffixKey =>
            Parent.SuffixKey;

        [JsonIgnore]
        string _suffixDisplayValue;

        [JsonIgnore]
        public string SuffixDisplayValue {
            get {
                if(string.IsNullOrEmpty(_suffixDisplayValue)) {
                    _suffixDisplayValue = PatternType.ToDisplayValue(SuffixKey);
                }

                return _suffixDisplayValue;
            }
        }

        [JsonIgnore]
        public PatternKeyCollection Parent { get; private set; }

        [JsonIgnore]
        public NoteType Key =>
            Parent.Key;

        [JsonIgnore]
        public string Name =>
            $"{Key} {SuffixDisplayValue}";

        [JsonIgnore]
        public string FullName =>
            $"{Name}_{Position}_{SubPosition}";

        #endregion

        #endregion

        #region Constructors

        public NotePattern() {
        }

        public NotePattern(PatternKeyCollection ngc) : this(ngc,0,[]) {
        }

        public NotePattern(PatternKeyCollection ngc,int position) : this(ngc,position,[]) {
        }

        public NotePattern(PatternKeyCollection ngc,int position,IEnumerable<PatternNote> notes) : this() {
            Position = position;
            Notes.AddRange(notes);
            SetParent(ngc);
        }

        #endregion

        #region Public Methods

        public void SetParent(PatternKeyCollection parent) {
            Parent = parent;
            foreach(PatternNote pn in Notes) {
                pn.SetParent(this);
            }
        }

        public string GetSignature() {
            return string.Join(" ",Notes.OrderBy(x => x.RowNum).Select(x => x.ColNum.ToString()));
        }

        public override string ToString() {
            return FullName;
        }

        public bool IsInBookmarkGroup(BookmarkGroup bmg) {
            if(bmg == null) {
                return false;
            }

            return BookmarkGroupIds.Contains(bmg.Id);
        }

        public void AddToBookmarkGroup(BookmarkGroup bmg) {
            if(bmg is null ||
               IsInBookmarkGroup(bmg)) {
                return;
            }

            BookmarkGroupIds.Add(bmg.Id);
            BookmarkGroupIdsCsv = string.Join(",",BookmarkGroupIds);
        }

        public void RemoveFromBookmarkGroup(BookmarkGroup bmg) {
            if(bmg is null ||
               !IsInBookmarkGroup(bmg)) {
                return;
            }

            BookmarkGroupIds.Remove(bmg.Id);
            BookmarkGroupIdsCsv = string.Join(",",BookmarkGroupIds);
        }

        public IEnumerable<IEnumerable<int>> GetToneGroups() {
            if(Parent is not { } ngc ||
               ngc.Parent is not { } tuning ||
               tuning.Parent is not { } inst) {
                return null;
            }

            var note_sets = new List<List<Note>>();
            InstrumentType it = inst.InstrumentType;
            note_sets = Notes.Where(x => !x.IsMute).Select(x => new List<Note> { x }).ToList();
            if(it.IsDoubledStrings()) {
                foreach(var note_set in note_sets) {
                    if(note_set.FirstOrDefault() is not InstrumentNote inn) {
                        continue;
                    }

                    note_set.Add(it.GetDoubledString(inn.Key,inn.Register,inn.RowNum).ToNote());
                }
            }

            return note_sets.Select(x => x.Select(y => y.MidiTone));
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}