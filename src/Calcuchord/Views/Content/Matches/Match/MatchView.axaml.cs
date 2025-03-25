using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchView : UserControl {
        public MatchView() {
            InitializeComponent();

        }

        void MatchContainerBorder_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(DataContext is not MatchViewModel mtvm ||
               Parent is not ContentPresenter presenter) {
                return;
            }

            mtvm.SelectMatchCommand.Execute(null);
            presenter.BringIntoView();
        }

        void TranslateFlyoutBase_OnOpening(object sender,EventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               sender is not Flyout flyout ||
               flyout.Content is not Control c ||
               c.GetVisualDescendant<ContextMenu>() is not { } cm ||
               DataContext is not MatchViewModel mtvm) {
                return;
            }

            if(cm.Items.Count == 2) {
                // remove last translate menu
                cm.Items.RemoveAt(1);
            }

            MenuItem trans_mi = new MenuItem
            {
                Header = "Translate",
                Icon = new MaterialIcon
                {
                    Kind = MaterialIconKind.MusicClefTreble,
                },
            };
            foreach(InstrumentViewModel inst in mvm.Instruments) {
                MenuItem inst_mi = new MenuItem
                {
                    Header = inst.Name,
                    Icon = new MaterialIcon
                    {
                        Kind = inst.Icon.ToEnum<MaterialIconKind>(),
                    },
                };
                foreach(TuningViewModel tuning in inst.Tunings) {
                    if(tuning.Tuning == mtvm.NotePattern.Parent.Parent) {
                        // hide current tuning
                        continue;
                    }

                    MenuItem tuning_mi = new MenuItem
                    {
                        Header = tuning.Name,
                        Command = mvm.TranslatePatternCommand,
                        CommandParameter = new[] { DataContext,tuning },
                    };
                    inst_mi.Items.Add(tuning_mi);
                }

                if(inst_mi.Items.None()) {
                    continue;
                }

                trans_mi.Items.Add(inst_mi);
            }

            if(trans_mi.Items.None()) {
                // hide translate when no other tunings
                return;
            }

            cm.Items.Add(trans_mi);
        }

        void BookmarkFlyout_OnOpened(object sender,EventArgs e) {
            if(DataContext is not MatchViewModel mtvm ||
               sender is not Flyout flyout ||
               flyout.Content is not Control c ||
               c.GetVisualDescendant<ContextMenu>() is not { } cm ||
               MainViewModel.Instance is not { } mvm ||
               mvm.SelectedTuning is not { } stvm) {
                return;
            }

            if(stvm.AvailableBookmarkGroups.Count() <= 2) {
                // auto toggle default
                stvm.DefaultBookmarkGroup.TogglePatternCommand.Execute(mtvm.NotePattern);
                Extensions.CloseFlyout(BookmarkButton);
                return;
            }

            cm.Items.Clear();

            foreach(BookmarkGroupViewModel avail_bmvm in stvm.AvailableBookmarkGroups) {
                MenuItem mi = new MenuItem
                {
                    // BorderBrush = ThemeViewModel.Instance.P[PaletteColorType.Fg].ToAvBrush(),
                    // BorderThickness = avail_bmvm.IsAddGroupPlaceholder ? new Thickness() : new Thickness(0,0,0,1),
                    DataContext = avail_bmvm,
                    Header = avail_bmvm.GroupName,
                    Icon = avail_bmvm.IsAddGroupPlaceholder ?
                        new MaterialIcon
                        {
                            Margin = new Thickness(5),
                            Kind = MaterialIconKind.Add,
                        } :
                        new Border
                        {
                            CornerRadius = new CornerRadius(5),
                            Background = avail_bmvm.HexColor.ToAvBrush(),
                            Child = new MaterialIcon
                            {
                                Kind = MaterialIconKind.Check,
                                Foreground = avail_bmvm.HexColor.IsHexStringBright() ? Brushes.Black : Brushes.White,
                                IsVisible = mtvm.NotePattern.IsInBookmarkGroup(avail_bmvm.BookmarkGroup),
                            },
                        },
                    Command = avail_bmvm.IsAddGroupPlaceholder ? avail_bmvm.BeginEditCommand :
                        avail_bmvm.TogglePatternCommand,
                    CommandParameter = mtvm.NotePattern,
                };
                cm.Items.Add(mi);
            }
        }
    }
}