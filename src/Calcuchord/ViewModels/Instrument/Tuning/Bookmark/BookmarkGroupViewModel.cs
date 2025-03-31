using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace Calcuchord;

public class BookmarkGroupViewModel : ViewModelBase<TuningViewModel> {
    #region Constructors

    public BookmarkGroupViewModel(TuningViewModel parent, BookmarkGroup bookmarkGroup) : base(parent) {
        BookmarkGroup = bookmarkGroup;
    }

    #endregion

    #region Private Variables

    private string _preEditBookmarkGroupJson;

    private NotePattern _patternToAdd;

    #endregion

    #region Properties

    #region Appearance

    public string HexColor =>
        ColorId < 0 ? "#00000000" : ThemeViewModel.Instance.BookmarkColors[ColorId];

    #endregion

    #region State

    public bool IsSelected =>
        Parent.SelectedBookmarkGroups.Contains(this);

    public bool CanEdit => !IsAddGroupPlaceholder;
    public bool CanDrag => !IsAddGroupPlaceholder;
    public bool IsDropOverLeft { get; set; }
    public bool IsDropOverRight { get; set; }
    public bool IsDragging { get; set; }
    public bool IsDragValid { get; set; }
    public bool IsDragCopy { get; set; }

    public bool IsEditing { get; set; }

    public bool IsAddGroupPlaceholder => BookmarkGroup == null;

    public bool IsNew =>
        !Parent.BookmarkGroups.Contains(this);

    public int MaxNameLength => 10;

    #endregion

    #region Model

    public int SortOrderIdx
    {
        get => BookmarkGroup == null ? int.MaxValue : BookmarkGroup.SortOrderIdx;
        set
        {
            if (SortOrderIdx != value) {
                BookmarkGroup.SortOrderIdx = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsDefault =>
        BookmarkGroup == null ? false : BookmarkGroup.IsDefault;

    private int ColorId
    {
        get => BookmarkGroup == null ? -1 : BookmarkGroup.ColorId;
        set
        {
            if (ColorId != value && BookmarkGroup != null) {
                BookmarkGroup.ColorId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HexColor));
            }
        }
    }

    public string GroupId =>
        BookmarkGroup == null ? string.Empty : BookmarkGroup.Id;

    public string GroupName
    {
        get => BookmarkGroup == null ? "Add" : BookmarkGroup.Name;
        set
        {
            if (value != null && GroupName != value && BookmarkGroup != null) {
                BookmarkGroup.Name = value.Substring(0, Math.Min(value.Length, MaxNameLength));
                OnPropertyChanged();
            }
        }
    }

    public MusicPatternType PatternType =>
        BookmarkGroup == null ? MainViewModel.Instance.SelectedPatternType : BookmarkGroup.PatternType;

    public BookmarkGroup BookmarkGroup { get; private set; }

    #endregion

    #endregion

    #region Commands

    public ICommand ToggleSelectedCommand => new MpCommand<object>(
        args =>
        {
            if (IsAddGroupPlaceholder) {
                Parent.AddNewBookmarkGroupCommand.Execute(args);
                return;
            }

            if (IsSelected)
                Parent.SelectedBookmarkGroups.Remove(this);
            else
                Parent.SelectedBookmarkGroups.Add(this);

            OnPropertyChanged(nameof(IsSelected));

            if (MainViewModel.Instance is not { } mvm) return;

            mvm.RaisePropertyChanged(nameof(mvm.IsAnyBookmarksSelected));

            if (mvm.IsBookmarkModeSelected)
                mvm.UpdateMatchesAsync(MatchUpdateSource.BookmarkToggle)
                    .FireAndForgetSafeAsync(DispatcherPriority.Background);


        });


    public ICommand FinishEditCommand => new MpCommand<object>(
        async args =>
        {
            Debug.Assert(CanEdit, "Cannot edit placeholder group");

            if (args is not string finish_type) return;

            if (finish_type == "FINISH") {
                if (!Parent.BookmarkGroups.Contains(this)) {
                    SortOrderIdx = Parent.AvailableBookmarkGroups.Count;
                    Parent.BookmarkGroups.Add(this);
                }

                if (_patternToAdd != null)
                    // only occurs for a new group
                    ToggleLinkedWithPatternCommand.Execute(_patternToAdd);

                Parent.UpdateAvailableSortOrder(true);

                Prefs.Instance.Save();
            }
            else if (finish_type == "DELETE") {
                bool? confirmed = null;
                var confirm_view = new YesNoDialogView();
                confirm_view.Title.Text = $"Are you sure you want to delete '{GroupName}'?";
                confirm_view.NoButton.Command = new MpCommand(
                    () =>
                    {
                        confirmed = false;
                        DialogManager.Close(MainViewModel.Instance.BookmarkGroupEditDialogHostName);
                    });
                confirm_view.YesButton.Command = new MpCommand(
                    () =>
                    {
                        confirmed = true;
                        DialogManager.Close(MainViewModel.Instance.BookmarkGroupEditDialogHostName);
                    });
                DialogManager.ShowAsync(confirm_view, MainViewModel.Instance.BookmarkGroupEditDialogHostName)
                    .FireAndForgetSafeAsync();
                while (confirmed == null) await Task.Delay(100);

                if (!confirmed.Value) {
                    _patternToAdd = null;
                    _preEditBookmarkGroupJson = null;
                    IsEditing = false;
                    return;
                }

                Parent.BookmarkGroups.Remove(this);
                Parent.SelectedBookmarkGroups.Remove(this);
                Parent.BoundBookmarkGroups.Remove(this);
                // Parent.BoundBookmarkGroups.Where(x => !x.IsAddGroupPlaceholder && x.SortOrderIdx > SortOrderIdx)
                //     .ForEach(x => x.SortOrderIdx--);
                foreach (var assoc_pattern in Parent.Tuning.Collections.SelectMany(x => x.Value)
                             .SelectMany(x => x.Patterns).Where(x => x.IsInBookmarkGroup(BookmarkGroup)))
                    assoc_pattern.RemoveFromBookmarkGroup(BookmarkGroup);

                Parent.UpdateAvailableSortOrder(false);

                Prefs.Instance.Save();
            }
            else {
                if (_preEditBookmarkGroupJson != BookmarkGroup.SerializeObject())
                    // restore group
                    BookmarkGroup = _preEditBookmarkGroupJson.DeserializeObject<BookmarkGroup>();
            }

            _patternToAdd = null;
            _preEditBookmarkGroupJson = null;
            IsEditing = false;

            DialogManager.Close(MainViewModel.Instance.MainDialogHostName);
        });

    public ICommand BeginEditCommand => new MpCommand<object>(
        args =>
        {
            Debug.Assert(CanEdit, "Cannot edit placeholder group");

            _preEditBookmarkGroupJson = BookmarkGroup.SerializeObject();
            if (args is NotePattern np)
                // add new from match bookmark flyout
                _patternToAdd = np;

            IsEditing = true;

            var edit_bmv = new EditBookmarkGroupView
            {
                DataContext = this
            };
            DialogManager.ShowAsync(edit_bmv, MainViewModel.Instance.MainDialogHostName);


        });

    public ICommand ChangeColorCommand => new MpCommand<object>(
        args =>
        {
            if (args is not string newColorIdStr ||
                !int.TryParse(newColorIdStr, out var newColorId) ||
                ColorId == newColorId)
                return;

            ColorId = newColorId;
            OnPropertyChanged(nameof(HexColor));
        });

    public ICommand ToggleLinkedWithPatternCommand => new MpCommand<object>(
        args =>
        {
            if (args is not NotePattern np ||
                np.PatternType != PatternType)
                return;

            if (np.IsInBookmarkGroup(BookmarkGroup))
                // remove
                np.RemoveFromBookmarkGroup(BookmarkGroup);
            else
                np.AddToBookmarkGroup(BookmarkGroup);

            if (MainViewModel.Instance is { } mvm &&
                mvm.Matches.FirstOrDefault(x => x.NotePattern == np) is { } mtvm)
                mtvm.RaisePropertyChanged(nameof(mtvm.IsBookmarked));

            if (MainViewModel.Instance.SelectedDisplayMode == DisplayModeType.Bookmarks &&
                Parent.SelectedBookmarkGroups.Contains(this))
                MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.BookmarkToggle)
                    .FireAndForgetSafeAsync();

            Prefs.Instance.Save();
        });

    #endregion
}