using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
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

        void FlyoutBase_OnOpened(object sender,EventArgs e) {
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
                    Kind = MaterialIconKind.Translate,
                },
            };
            foreach(InstrumentViewModel inst in mvm.Instruments) {
                MenuItem inst_mi = new MenuItem
                {
                    Header = inst.Title,
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
    }
}