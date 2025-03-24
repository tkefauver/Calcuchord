using System;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class BookmarkGroup {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static BookmarkGroup Create(Tuning tuning,MusicPatternType pt,string name = "",int colorId = -1,
            bool isDefault = false) {
            BookmarkGroup bookmarkGroup = new BookmarkGroup();
            bookmarkGroup.Id = Guid.NewGuid().ToString();
            bookmarkGroup.TuningId = tuning.Id;
            bookmarkGroup.PatternType = pt;
            bookmarkGroup.ColorId = colorId < 0 ? MpRandom.Rand.Next(ThemeViewModel.Instance.BookmarkColors.Length) :
                colorId;
            bookmarkGroup.Name = name;
            bookmarkGroup.IsDefault = isDefault;
            return bookmarkGroup;
        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public int SortOrderIdx { get; set; }

        [JsonProperty]
        public string TuningId { get; set; }

        [JsonProperty]
        public MusicPatternType PatternType { get; set; }

        [JsonProperty]
        public int ColorId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IsDefault { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public string HexColor =>
            ThemeViewModel.Instance.BookmarkColors[ColorId];

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public bool IsTuningBookmark(Tuning tuning) {
            return TuningId == tuning.Id;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}