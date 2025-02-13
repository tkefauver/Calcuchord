using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class InstrumentView : UserControl {
        public static InstrumentView Instance { get; private set; }

        bool WasLastPressHold { get; set; }

        public InstrumentView() {
            Instance = this;
            InitializeComponent();
        }

        public void ScrollSelectionIntoView() {
            if(MainViewModel.Instance is not { } mvm ||
               mvm.IsPianoSelected ||
               mvm.SelectedTuning is not { } stvm ||
               stvm.SelectedNotes is not { } sel_notes ||
               sel_notes.OrderBy(x => x.NoteNum).FirstOrDefault() is not { } low_note_vm ||
               sel_notes.OrderByDescending(x => x.NoteNum).FirstOrDefault() is not { } hi_note_vm ||
               this.GetVisualDescendants<FretView>() is not { } fvl ||
               fvl.FirstOrDefault(x => x.DataContext == low_note_vm) is not { } low_note_v ||
               fvl.FirstOrDefault(x => x.DataContext == hi_note_vm) is not { } hi_note_v) {

                if(this.GetVisualDescendants<ScrollViewer>().FirstOrDefault(x => x.Classes.Contains("inst-scroll")) is
                   { } inst_sv) {
                    // scroll to home when nothing selected
                    inst_sv.ScrollToHome();
                }

                return;
            }

            low_note_v.BringIntoView();
            hi_note_v.BringIntoView();
        }

        public void AttachHandlers(Control c) {
            c.PointerPressed += NoteView_OnPointerPressed;
            c.PointerReleased += NoteView_OnPointerReleased;
            c.Holding += NoteView_OnHolding;
            Gestures.SetIsHoldingEnabled(c,true);
            Gestures.SetIsHoldWithMouseEnabled(c,true);
        }


        public void DetachHandlers(Control c) {
            c.PointerPressed -= NoteView_OnPointerPressed;
            c.PointerReleased -= NoteView_OnPointerReleased;
            c.Holding -= NoteView_OnHolding;
            Gestures.SetIsHoldingEnabled(c,false);
            Gestures.SetIsHoldWithMouseEnabled(c,false);
        }

        public void MeasureInstrument() {
            if(this.GetVisualDescendant<KeyboardView>() is { } kbv) {
                kbv.MeasureKeyboard();
            }

            if(this.GetVisualDescendant<FretboardView>() is { } fbv) {
                fbv.MeasureFretboard();
            }
        }

        void NoteView_OnPointerPressed(object sender,PointerPressedEventArgs e) {
            WasLastPressHold = false;
        }

        void NoteView_OnHolding(object sender,HoldingRoutedEventArgs e) {
            if(WasLastPressHold ||
               sender is not Control c ||
               c.DataContext is not NoteViewModel nvm ||
               nvm.Parent is not { } nrvm) {
                return;
            }

            WasLastPressHold = true;
            nrvm.ToggleNoteAsDesiredRootCommand.Execute(nvm);
        }

        void NoteView_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(WasLastPressHold ||
               sender is not Control c ||
               c.DataContext is not NoteViewModel nvm ||
               nvm.Parent is not { } nrvm) {
                return;
            }

            if(e.IsRightRelease(c)) {
                nrvm.ToggleNoteAsDesiredRootCommand.Execute(nvm);
            } else {
                nrvm.ToggleNoteSelectedCommand.Execute(nvm);
            }
        }
    }
}