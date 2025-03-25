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
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Crypto.Operators;
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

            string BOOKMARK_DND_ID = "bookmarkGroupItemView";
            DragDropEffects? result = null;
            FakeDragEventArgs fake_e = null;
            bool is_mobile = ThemeViewModel.Instance.IsMobile;

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

            async void Anchor_PointerPressed(object sender,PointerPressedEventArgs e) {
                //e.Handled = true;
                result = null;

                DateTime press_dt = DateTime.Now;
                bool is_dbl_tap =
                    anchor_bmgvm.LastPressDt.HasValue &&
                    press_dt - anchor_bmgvm.LastPressDt.Value <= TimeSpan.FromMilliseconds(500);
                anchor_bmgvm.LastPressDt = press_dt;
                bool was_selected = anchor_bmgvm.IsSelected;

                bool was_released = false;

                anchorControl.PointerReleased += AnchorControlOnPointerReleased;

                void AnchorControlOnPointerReleased(object o,PointerReleasedEventArgs pointerReleasedEventArgs) {
                    was_released = true;
                    anchor_bmgvm.IsDragging = false;
                    anchor_bmgvm.IsDragCopy = false;
                    anchorControl.PointerReleased -= AnchorControlOnPointerReleased;
                    if(fake_e != null) {
                        fake_e.RealE = pointerReleasedEventArgs;
                        HandleDrop(o,fake_e);
                        result = fake_e.DragEffects;
                    }
                }

                if(is_dbl_tap) {
                    anchor_bmgvm.BeginEditCommand.Execute(null);
                    return;
                }

                await Task.Delay(500);
                if(anchor_bmgvm.LastPressDt != press_dt) {
                    // dbl tap, ignore
                    return;
                }


                if(!was_released) {
                    if(is_mobile) {
                        fake_e = new FakeDragEventArgs() { RealE = e };
                        fake_e.Data.Set(BOOKMARK_DND_ID,anchor_bmgvm);
                        
                        this.BookmarkGroupsItemsControl.AddHandler(PointerMovedEvent,BookmarkItemsControl_PointerMoved,RoutingStrategies.Tunnel,true);
                        this.BookmarkGroupsItemsControl.AddHandler(PointerReleasedEvent,BookmarkGroupsItemsControlOnPointerReleased,RoutingStrategies.Tunnel,true);
                       

                        void BookmarkGroupsItemsControlOnPointerReleased(object o,PointerReleasedEventArgs pointerReleasedEventArgs) {
                            fake_e.RealE = pointerReleasedEventArgs;
                            if(tvm.AvailableBookmarkGroups.None(x => x.IsDragging) ||
                               this.BookmarkGroupsItemsControl.GetVisualDescendants<BookmarkGroupItemView>() is not {} bkivl ||
                               bkivl.FirstOrDefault(x=>x.Bounds.Contains(e.GetPosition(x))) is not {} over_bkv ||
                               over_bkv.DataContext is not {} over_bkvm) {
                                return;
                            }

                            tvm.AvailableBookmarkGroups.Where(x => x != over_bkvm).ForEach(
                                x => {
                                    x.IsDropOverLeft = false;
                                    x.IsDropOverRight = false;
                                });
                            HandleDrop(over_bkvm,fake_e);
                            result = fake_e.DragEffects;
                            
                            
                            this.BookmarkGroupsItemsControl.PointerMoved -= BookmarkItemsControl_PointerMoved;
                            this.BookmarkGroupsItemsControl.PointerReleased -= BookmarkGroupsItemsControlOnPointerReleased;
                        }

                        void BookmarkItemsControl_PointerMoved(object sender3,PointerEventArgs e3) {
                            fake_e.RealE = e3;
                            if(tvm.AvailableBookmarkGroups.None(x => x.IsDragging) ||
                               this.BookmarkGroupsItemsControl.GetVisualDescendants<BookmarkGroupItemView>() is not {} bkivl ||
                               bkivl.FirstOrDefault(x=>x.Bounds.Contains(e.GetPosition(x))) is not {} over_bkv ||
                               over_bkv.DataContext is not {} over_bkvm) {
                                return;
                            }

                            tvm.AvailableBookmarkGroups.Where(x => x != over_bkvm).ForEach(
                                x => {
                                    x.IsDropOverLeft = false;
                                    x.IsDropOverRight = false;
                                });
                            HandleOver(over_bkvm,fake_e);
                        }
                    }

                    DataObject drag_data = new DataObject();
                    drag_data.Set(BOOKMARK_DND_ID,anchor_bmgvm);

                    anchor_bmgvm.IsDragging = true;
                    result = await DragDrop.DoDragDrop(e,drag_data,DragDropEffects.Move | DragDropEffects.Copy);
                    if(is_mobile) {
                        while(result == null) {
                            await Task.Delay(100);
                        }
                    }
                }

                bool will_select = !was_selected;
                if(anchor_bmgvm.IsEditing || (result.HasValue && result != DragDropEffects.None)) {
                    will_select = was_selected;
                }

                if(will_select) {
                    if(!anchor_bmgvm.Parent.SelectedBookmarkGroups.Contains(anchor_bmgvm)) {
                        anchor_bmgvm.Parent.SelectedBookmarkGroups.Add(anchor_bmgvm);
                    }
                } else {
                    anchor_bmgvm.Parent.SelectedBookmarkGroups.Remove(anchor_bmgvm);
                }
            }


            void HandleLeave(object sender,object args) {
                dynamic e = args as DragEventArgs;
                if(e == null) {
                    e = args as FakeDragEventArgs;
                }

                if(sender is not Control c ||
                   c.DataContext is not BookmarkGroupViewModel bmgvm) {
                    return;
                }

                bmgvm.IsDropOverLeft = false;
                bmgvm.IsDropOverRight = false;
                PlatformWrapper.Services.Logger.WriteLine($"Drag Leave: '{bmgvm.GroupName}'");
            }


            void HandleOver(object sender,object args) {
                BookmarkGroupViewModel bmgvm = null;
                BookmarkGroupViewModel drop_bmgvm = null;
                dynamic e = args as DragEventArgs;

                if(e == null) {
                    e = args as FakeDragEventArgs;
                    drop_bmgvm = anchor_bmgvm;
                    bmgvm = sender as BookmarkGroupViewModel;//tvm.AvailableBookmarkGroups.FirstOrDefault(x => x.IsDropOverLeft || x.IsDropOverRight);

                } else {
                    drop_bmgvm = e.Data.Get(BOOKMARK_DND_ID);
                    if(sender is Control c) {
                        bmgvm = c.DataContext as BookmarkGroupViewModel;
                    }
                }

                if(bmgvm == null ||
                   drop_bmgvm == null) {
                    return;
                }

                if(drop_bmgvm == bmgvm) {
                    e.DragEffects = DragDropEffects.None;
                    return;
                }

                drop_bmgvm.IsDragCopy = e.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                                        e.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                                        e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if(bmgvm.IsAddGroupPlaceholder ||
                   (!drop_bmgvm.IsDragCopy && drop_bmgvm == bmgvm)) {
                    e.DragEffects = DragDropEffects.None;
                    drop_bmgvm.IsDragValid = false;
                    PlatformWrapper.Services.Logger.WriteLine("Drag Invalid");
                    return;
                }

                drop_bmgvm.IsDragValid = true;
                bmgvm.IsDropOverLeft = e.GetPosition(this).X / Bounds.Width <= 0.5;
                bmgvm.IsDropOverRight = !bmgvm.IsDropOverLeft;

                if(drop_bmgvm.IsDragCopy) {
                    e.DragEffects = DragDropEffects.Copy;
                } else {
                    int drop_idx = bmgvm.SortOrderIdx + (bmgvm.IsDropOverRight ? 1 : 0);
                    if(drop_bmgvm.SortOrderIdx == drop_idx) {
                        // invalidate self drop
                        bmgvm.IsDropOverLeft = false;
                        bmgvm.IsDropOverRight = false;
                        drop_bmgvm.IsDragValid = false;
                        e.DragEffects = DragDropEffects.None;
                    } else {
                        e.DragEffects = DragDropEffects.Move;
                    }
                }

                PlatformWrapper.Services.Logger.WriteLine($"Drag Over: '{bmgvm.GroupName}' Flags: {e.DragEffects}");
            }

            void HandleDrop(object sender,object args) {
                BookmarkGroupViewModel bmgvm = null;
                BookmarkGroupViewModel drop_bmgvm = null;
                dynamic e = args as DragEventArgs;

                if(e == null) {
                    e = args as FakeDragEventArgs;
                    drop_bmgvm = anchor_bmgvm;
                    bmgvm = sender as BookmarkGroupViewModel;

                } else {
                    drop_bmgvm = e.Data.Get(BOOKMARK_DND_ID);
                    if(sender is Control c) {
                        bmgvm = c.DataContext as BookmarkGroupViewModel;
                    }
                }

                if(bmgvm == null ||
                   drop_bmgvm == null) {
                    return;
                }


                if(e.DragEffects == DragDropEffects.None) {
                    return;
                }

                bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                               e.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                               e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;

                Dispatcher.UIThread.Post(
                    () => {

                        PlatformWrapper.Services.Logger.WriteLine($"Drop: '{bmgvm.GroupName}' ");

                        int drop_idx = tvm.AvailableBookmarkGroups.IndexOf(bmgvm) + (bmgvm.IsDropOverRight ? 1 : 0);
                        var avail_group_vml = tvm.AvailableBookmarkGroups.ToList();
                        if(is_copy) {
                            // clone drag group and link it w/ all assoc drops linked patterns
                            BookmarkGroup copied_bmg = drop_bmgvm.BookmarkGroup.Clone();
                            copied_bmg.Name += " copy";
                            var linked_patterns = tvm.Tuning.Collections.SelectMany(x => x.Value)
                                .SelectMany(x => x.Patterns).Where(x => x.IsInBookmarkGroup(drop_bmgvm.BookmarkGroup));
                            linked_patterns.ForEach(x => x.AddToBookmarkGroup(copied_bmg));

                            // add clone and prepare it for sort
                            drop_bmgvm = new BookmarkGroupViewModel(tvm,copied_bmg);
                            drop_bmgvm.SortOrderIdx = tvm.AvailableBookmarkGroups.Count - 2;
                            tvm.BookmarkGroups.Add(drop_bmgvm);
                            avail_group_vml.Insert(drop_idx,drop_bmgvm);
                        } else {
                            avail_group_vml.Move(tvm.AvailableBookmarkGroups.IndexOf(drop_bmgvm),drop_idx);
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

            void Anchor_DragLeave(object sender,DragEventArgs e) {
                HandleLeave(sender,e);
            }

            void Anchor_DragOver(object sender,DragEventArgs e) {
                HandleOver(sender,e);
            }

            void Anchor_Drop(object sender,DragEventArgs e) {
                HandleDrop(sender,e);
            }


        }

        #endregion

    }

    internal class FakeDragEventArgs {
        public PointerEventArgs RealE { get; set; }
        public DragDropEffects DragEffects { get; set; }
        public KeyModifiers KeyModifiers { get; } = KeyModifiers.None;
        public DataObject Data { get; } = new DataObject();

        public Point GetPosition(Visual v) {
            if(RealE is not { } e) {
                return new();
            }
            return e.GetPosition(v);
        }
    }
}