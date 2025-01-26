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