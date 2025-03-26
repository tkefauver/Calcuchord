using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class OptionsDrawerView : UserControl {
        public OptionsDrawerView() {
            InitializeComponent();
        }

        void InputElement_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               sender is not TextBlock tb) {
                return;
            }

            if(tb.Text == "Suffix" &&
               mvm.SuffixOptions.Where(x => x.IsChecked) is { } sel_suff &&
               sel_suff.Any()) {
                sel_suff.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Degree" &&
                      mvm.DegreeOptions.Where(x => x.IsChecked) is { } sel_deg &&
                      sel_deg.Any()) {
                sel_deg.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Key" &&
                      mvm.KeyOptions.Where(x => x.IsChecked) is { } sel_key &&
                      sel_key.Any()) {
                sel_key.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Sort") {
                OptionViewModel so1 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 0);
                OptionViewModel so2 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 1);
                OptionViewModel so3 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 2);

                OptionViewModel so4 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 3);
                so1.IsChecked = false;
                so2.IsChecked = false;
                so3.IsChecked = false;
                so4.IsChecked = false;

                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so1),0);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so2),1);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so3),2);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so4),3);

                mvm.RaisePropertyChanged(nameof(mvm.SortOption1));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption2));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption3));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption4));

                mvm.UpdateMatchesAsync(MatchUpdateSource.SortToggle).FireAndForgetSafeAsync();
            }
        }

        #region Bookmark Dnd

        void BookmarkGroupContainerButton_OnLoaded(object sender,RoutedEventArgs e) {
            if(sender is not Control ctrl) {
                return;
            }

            InitDnd(ctrl);

        }

        void InitDnd(Control anchorControl) {
            if(anchorControl.DataContext is not BookmarkGroupViewModel anchor_bmgvm ||
               anchor_bmgvm.Parent is not { } tvm) {
                return;
            }

            DateTime? last_press_dt = null;
            string BOOKMARK_DND_ID = "bookmarkGroupItemView";
            double MIN_DRAG_DIST = 10;
            TimeSpan DOUBLE_TAP_SPAN = TimeSpan.FromMilliseconds(500);
            TimeSpan HOLD_SPAN = DOUBLE_TAP_SPAN * 2;

            DragDrop.SetAllowDrop(anchorControl,true);

            anchorControl.Unloaded += Anchor_Unloaded;
            anchorControl.AddHandler(PointerPressedEvent,Anchor_PointerPressed,RoutingStrategies.Tunnel,true);

            anchorControl.AddHandler(DragDrop.DragOverEvent,Anchor_DragOver);
            anchorControl.AddHandler(DragDrop.DragLeaveEvent,Anchor_DragLeave);
            anchorControl.AddHandler(DragDrop.DropEvent,Anchor_Drop);


            void Anchor_Unloaded(object sender,RoutedEventArgs e) {
                anchorControl.Unloaded -= Anchor_Unloaded;
                anchorControl.RemoveHandler(PointerPressedEvent,Anchor_PointerPressed);
                anchorControl.RemoveHandler(DragDrop.DragOverEvent,Anchor_DragOver);
                anchorControl.RemoveHandler(DragDrop.DragLeaveEvent,Anchor_DragLeave);
                anchorControl.RemoveHandler(DragDrop.DropEvent,Anchor_Drop);
            }

            async void Anchor_PointerPressed(object sender_press,PointerPressedEventArgs e_press) {
                bool can_drag = anchor_bmgvm.CanDrag && e_press.Pointer.Type == PointerType.Mouse;
                bool can_edit = anchor_bmgvm.CanEdit;

                bool is_ready_to_handle_release = true;
                bool handle_release = true;

                DateTime press_dt = DateTime.Now;

                bool is_dbl_tap_press =
                    last_press_dt.HasValue &&
                    press_dt - last_press_dt.Value <= TimeSpan.FromMilliseconds(500);
                last_press_dt = press_dt;

                bool is_still_down = true;
                Point down_loc = e_press.GetPosition(anchorControl);
                double drag_dist = 0;

                anchorControl.PointerReleased += AnchorControl_PointerReleased;
                anchorControl.PointerMoved += AnchorControl_PointerMoved;
                anchorControl.PointerExited += AnchorControl_PointerExited;

                void AnchorControl_PointerExited(object sender,PointerEventArgs e) {
                    anchorControl.PointerReleased -= AnchorControl_PointerReleased;
                    anchorControl.PointerMoved -= AnchorControl_PointerMoved;
                    anchorControl.PointerExited -= AnchorControl_PointerExited;
                }

                void AnchorControl_PointerMoved(object sender_move,PointerEventArgs e_move) {
                    drag_dist = down_loc.Distance(e_move.GetPosition(anchorControl));
                }

                async void AnchorControl_PointerReleased(object sender_release,PointerReleasedEventArgs e_release) {
                    is_still_down = false;
                    anchor_bmgvm.IsDragging = false;
                    anchor_bmgvm.IsDragCopy = false;
                    anchorControl.PointerReleased -= AnchorControl_PointerReleased;
                    anchorControl.PointerMoved -= AnchorControl_PointerMoved;
                    anchorControl.PointerExited -= AnchorControl_PointerExited;

                    bool is_over_anchor = anchorControl.Bounds.Contains(e_release.GetPosition(anchorControl));
                    Stopwatch sw = Stopwatch.StartNew();
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                    while(!is_ready_to_handle_release) {
                        await Task.Delay(10);
                        if(sw.Elapsed >= TimeSpan.FromSeconds(30)) {
                            // timeout (who knows what could happen)
                            break;
                        }
                    }

                    if(handle_release &&
                       is_over_anchor) {
                        // only handle release when over anchor and not part of a gesture
                        anchor_bmgvm.ToggleSelectedCommand.Execute(null);
                    }

                }

                if(can_edit) {
                    if(is_dbl_tap_press) {
                        handle_release = false;
                        anchor_bmgvm.BeginEditCommand.Execute(null);
                        return;
                    }

                    is_ready_to_handle_release = false;
                    await Task.Delay(DOUBLE_TAP_SPAN);
                    if(last_press_dt != press_dt) {
                        // new press occured ie dbl tap, ignore
                        handle_release = false;
                        is_ready_to_handle_release = true;
                        return;
                    }
                }

                if(is_still_down) {
                    // check for hold
                    is_ready_to_handle_release = false;
                    bool is_hold = false;
                    while(true) {
                        if(!is_still_down) {
                            break;
                        }

                        if(drag_dist >= MIN_DRAG_DIST) {
                            break;
                        }

                        if(DateTime.Now - press_dt >= HOLD_SPAN) {
                            is_hold = true;
                            break;
                        }

                        await Task.Delay(5);
                    }

                    if(is_hold) {
                        handle_release = false;
                        is_ready_to_handle_release = true;
                        anchor_bmgvm.BeginEditCommand.Execute(null);
                        return;
                    }

                    is_ready_to_handle_release = true;

                    if(can_drag && is_still_down) {
                        DataObject drag_data = new DataObject();
                        drag_data.Set(BOOKMARK_DND_ID,anchor_bmgvm);

                        anchor_bmgvm.IsDragging = true;
                        DragDropEffects result = await DragDrop.DoDragDrop(
                            e_press,drag_data,DragDropEffects.Move | DragDropEffects.Copy);
                        handle_release = result == DragDropEffects.None;
                    }
                } else {
                    is_ready_to_handle_release = true;
                }
            }

            void Anchor_DragLeave(object sender_leave,DragEventArgs e_leave) {
                anchor_bmgvm.IsDropOverLeft = false;
                anchor_bmgvm.IsDropOverRight = false;
                PlatformWrapper.Services.Logger.WriteLine($"Drag Leave: '{anchor_bmgvm.GroupName}'");
            }

            void Anchor_DragOver(object sender_over,DragEventArgs e_over) {
                if(e_over.Data.Get(BOOKMARK_DND_ID) is not BookmarkGroupViewModel drop_bmgvm) {
                    return;
                }

                if(drop_bmgvm == anchor_bmgvm) {
                    e_over.DragEffects = DragDropEffects.None;
                    return;
                }

                drop_bmgvm.IsDragCopy = e_over.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                                        e_over.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                                        e_over.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if(anchor_bmgvm.IsAddGroupPlaceholder ||
                   (!drop_bmgvm.IsDragCopy && drop_bmgvm == anchor_bmgvm)) {
                    e_over.DragEffects = DragDropEffects.None;
                    drop_bmgvm.IsDragValid = false;
                    PlatformWrapper.Services.Logger.WriteLine("Drag Invalid");
                    return;
                }

                drop_bmgvm.IsDragValid = true;
                anchor_bmgvm.IsDropOverLeft = e_over.GetPosition(anchorControl).X / anchorControl.Bounds.Width <= 0.5;
                anchor_bmgvm.IsDropOverRight = !anchor_bmgvm.IsDropOverLeft;

                if(drop_bmgvm.IsDragCopy) {
                    e_over.DragEffects = DragDropEffects.Copy;
                } else {
                    int drop_idx = anchor_bmgvm.SortOrderIdx + (anchor_bmgvm.IsDropOverRight ? 1 : 0);
                    if(drop_bmgvm.SortOrderIdx == drop_idx) {
                        // invalidate self drop
                        anchor_bmgvm.IsDropOverLeft = false;
                        anchor_bmgvm.IsDropOverRight = false;
                        drop_bmgvm.IsDragValid = false;
                        e_over.DragEffects = DragDropEffects.None;
                    } else {
                        e_over.DragEffects = DragDropEffects.Move;
                    }
                }

                PlatformWrapper.Services.Logger.WriteLine(
                    $"Drag Over: '{anchor_bmgvm.GroupName}' Flags: {e_over.DragEffects}");
            }

            void Anchor_Drop(object sender_drop,DragEventArgs e_drop) {
                if(e_drop.Data.Get(BOOKMARK_DND_ID) is not BookmarkGroupViewModel drop_bmgvm) {
                    return;
                }


                if(e_drop.DragEffects == DragDropEffects.None) {
                    return;
                }

                bool is_copy = e_drop.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                               e_drop.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                               e_drop.KeyModifiers.HasFlag(KeyModifiers.Shift);

                e_drop.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;

                Dispatcher.UIThread.Post(
                    () => {

                        PlatformWrapper.Services.Logger.WriteLine($"Drop: '{anchor_bmgvm.GroupName}' ");

                        int drop_idx = tvm.BoundBookmarkGroups.IndexOf(anchor_bmgvm) +
                                       (anchor_bmgvm.IsDropOverRight ? 1 : 0);
                        var avail_group_vml = tvm.BoundBookmarkGroups.ToList();
                        if(is_copy) {
                            // clone drag group and link it w/ all assoc drops linked patterns
                            BookmarkGroup copied_bmg = drop_bmgvm.BookmarkGroup.Clone();
                            copied_bmg.Name += " copy";
                            var linked_patterns = tvm.Tuning.Collections.SelectMany(x => x.Value)
                                .SelectMany(x => x.Patterns).Where(x => x.IsInBookmarkGroup(drop_bmgvm.BookmarkGroup));
                            linked_patterns.ForEach(x => x.AddToBookmarkGroup(copied_bmg));

                            // add clone and prepare it for sort
                            drop_bmgvm = new BookmarkGroupViewModel(tvm,copied_bmg);
                            drop_bmgvm.SortOrderIdx = tvm.BoundBookmarkGroups.Count - 2;
                            tvm.BookmarkGroups.Add(drop_bmgvm);
                            avail_group_vml.Insert(drop_idx,drop_bmgvm);
                        } else {
                            avail_group_vml.Move(tvm.BoundBookmarkGroups.IndexOf(drop_bmgvm),drop_idx);
                        }

                        foreach((BookmarkGroupViewModel avail_bgvm,int idx) in avail_group_vml.WithIndex()) {
                            avail_bgvm.SortOrderIdx = avail_bgvm.IsAddGroupPlaceholder ? int.MaxValue : idx;
                            avail_bgvm.IsDropOverLeft = false;
                            avail_bgvm.IsDropOverRight = false;
                            avail_bgvm.IsDragging = false;
                            avail_bgvm.IsDragCopy = false;
                        }

                        tvm.UpdateAvailableSortOrder(true);

                        Prefs.Instance.Save();
                    });
            }


        }

        #endregion

    }
}