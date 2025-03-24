using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DialogHostAvalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace Calcuchord {
    public class BookmarkGroupViewModel : ViewModelBase<TuningViewModel> {

        #region Private Variables

        string _preEditBookmarkGroupJson;

        NotePattern _patternToAdd;

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models

        #endregion

        #region Appearance

        public string HexColor =>
            ColorId < 0 ? "#00000000" : ThemeViewModel.Instance.BookmarkColors[ColorId];

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsEditing { get; set; }

        public bool IsAddGroupPlaceholder => BookmarkGroup == null;

        #endregion

        #region Model

        public int SortOrderIdx {
            get => BookmarkGroup == null ? int.MaxValue : BookmarkGroup.SortOrderIdx;
            set {
                if(SortOrderIdx != value) {
                    SortOrderIdx = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDefault =>
            BookmarkGroup == null ? false : BookmarkGroup.IsDefault;

        int ColorId {
            get => BookmarkGroup == null ? -1 : BookmarkGroup.ColorId;
            set {
                if(ColorId != value && BookmarkGroup != null) {
                    BookmarkGroup.ColorId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HexColor));
                }
            }
        }

        public string GroupId =>
            BookmarkGroup == null ? string.Empty : BookmarkGroup.Id;

        public string GroupName {
            get => BookmarkGroup == null ? "Add" : BookmarkGroup.Name;
            set {
                if(GroupName != value && BookmarkGroup != null) {
                    BookmarkGroup.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public MusicPatternType PatternType =>
            BookmarkGroup == null ?
                MainViewModel.Instance.SelectedPatternType : BookmarkGroup.PatternType;

        public BookmarkGroup BookmarkGroup { get; private set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public BookmarkGroupViewModel(TuningViewModel parent,BookmarkGroup bookmarkGroup) : base(parent) {
            BookmarkGroup = bookmarkGroup;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand FinishEditCommand => new MpCommand<object>(
            async (args) => {
                if(args is not string finish_type) {
                    return;
                }

                if(IsAddGroupPlaceholder) {
                    // shouldn't happen
                }

                if(finish_type == "FINISH") {
                    if(!Parent.BookmarkGroups.Contains(this)) {
                        Parent.BookmarkGroups.Add(this);
                    }

                    if(_patternToAdd != null) {
                        // only occurs for a new group
                        TogglePatternCommand.Execute(_patternToAdd);
                    }

                    Prefs.Instance.Save();
                } else if(finish_type == "DELETE") {
                    bool? confirmed = null;
                    YesNoDialogView confirm_view = new YesNoDialogView();
                    confirm_view.Title.Text = $"Are you sure you want to delete '{GroupName}'?";
                    confirm_view.NoButton.Command = new MpCommand(
                        () => {
                            confirmed = false;
                            DialogHost.Close(MainViewModel.Instance.BookmarkGroupEditDialogHostName);
                        });
                    confirm_view.YesButton.Command = new MpCommand(
                        () => {
                            confirmed = true;
                            DialogHost.Close(MainViewModel.Instance.BookmarkGroupEditDialogHostName);
                        });
                    DialogHost.Show(confirm_view,MainViewModel.Instance.BookmarkGroupEditDialogHostName)
                        .FireAndForgetSafeAsync();
                    while(confirmed == null) {
                        await Task.Delay(100);
                    }

                    if(!confirmed.Value) {
                        _patternToAdd = null;
                        _preEditBookmarkGroupJson = null;
                        return;
                    }

                    Parent.BookmarkGroups.Remove(this);
                    Parent.SelectedBookmarkGroups.Remove(this);
                    foreach(NotePattern assoc_pattern in Parent.Tuning.Collections.SelectMany(x => x.Value)
                                .SelectMany(x => x.Patterns).Where(x => x.IsInBookmarkGroup(BookmarkGroup))) {
                        assoc_pattern.RemoveFromBookmarkGroup(BookmarkGroup);
                    }

                    Parent.AvailableBookmarkGroups.ForEach(
                        (x,idx) => x.SortOrderIdx = x.IsAddGroupPlaceholder ? int.MaxValue : idx);

                    Prefs.Instance.Save();
                } else {
                    if(_preEditBookmarkGroupJson != BookmarkGroup.SerializeObject()) {
                        // restore group
                        BookmarkGroup = _preEditBookmarkGroupJson.DeserializeObject<BookmarkGroup>();
                    }
                }

                _patternToAdd = null;
                _preEditBookmarkGroupJson = null;
                IsEditing = false;


                DialogHost.Close(MainViewModel.Instance.MainDialogHostName);
            });


        public ICommand DoubleTapCommand => new MpCommand<object>(
            (args) => {
                BookmarkGroupViewModel to_edit_bmgvm = this;
                if(IsAddGroupPlaceholder) {
                    if(args is not NotePattern &&
                       args is not string) {
                        // actual double tap, ignore for add group (handled from initial click when selected
                        return;
                    }

                    BookmarkGroup new_bmg = BookmarkGroup.Create(
                        Parent.Tuning,MainViewModel.Instance.SelectedPatternType);
                    new_bmg.SortOrderIdx = Parent.AvailableBookmarkGroups.Count() - 1;
                    to_edit_bmgvm = new BookmarkGroupViewModel(Parent,new_bmg);
                }

                to_edit_bmgvm._preEditBookmarkGroupJson = to_edit_bmgvm.BookmarkGroup.SerializeObject();
                if(args is NotePattern np) {
                    // add new from match bookmark flyout
                    to_edit_bmgvm._patternToAdd = np;
                }

                to_edit_bmgvm.IsEditing = true;

                EditBookmarkGroupView edit_bmv = new EditBookmarkGroupView
                {
                    DataContext = to_edit_bmgvm,
                };
                DialogHost.Show(edit_bmv,MainViewModel.Instance.MainDialogHostName);


            });

        public ICommand ChangeColorCommand => new MpCommand<object>(
            (args) => {
                if(args is not string newColorIdStr ||
                   !int.TryParse(newColorIdStr,out int newColorId) ||
                   ColorId == newColorId) {
                    return;
                }

                ColorId = newColorId;
                OnPropertyChanged(nameof(HexColor));
            });

        public ICommand TogglePatternCommand => new MpCommand<object>(
            (args) => {
                if(args is not NotePattern np ||
                   np.PatternType != PatternType) {
                    return;
                }

                if(np.IsInBookmarkGroup(BookmarkGroup)) {
                    // remove
                    np.RemoveFromBookmarkGroup(BookmarkGroup);
                } else {
                    np.AddToBookmarkGroup(BookmarkGroup);
                }

                if(MainViewModel.Instance is { } mvm &&
                   mvm.Matches.FirstOrDefault(x => x.NotePattern == np) is { } mtvm) {
                    mtvm.RaisePropertyChanged(nameof(mtvm.IsBookmarked));
                }

                if(MainViewModel.Instance.SelectedDisplayMode == DisplayModeType.Bookmarks &&
                   Parent.SelectedBookmarkGroups.Contains(this)) {
                    MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.BookmarkToggle)
                        .FireAndForgetSafeAsync();
                }

                Prefs.Instance.Save();
            });

        #endregion

    }
}