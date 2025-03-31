using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord;

[DoNotNotify]
public partial class InstrumentView : UserControl {
    public InstrumentView() {
        Instance = this;
        InitializeComponent();
        this.GetObservable(IsVisibleProperty).Subscribe(value => MeasureInstrument());
        this.GetObservable(BoundsProperty).Subscribe(value => MeasureInstrument());
    }

    public static InstrumentView Instance { get; private set; }

    bool WasLastPressHold { get; set; }

    public void ScrollSelectionIntoView() {
        if(MainViewModel.Instance is not { } mvm ||
           mvm.IsPianoSelected ||
           mvm.SelectedTuning is not { } stvm ||
           stvm.ScrollToNotes is not { } primary_notes ||
           primary_notes.OrderBy(x => x.NoteNum).FirstOrDefault() is not { } low_note_vm ||
           primary_notes.OrderByDescending(x => x.NoteNum).FirstOrDefault() is not { } hi_note_vm ||
           this.GetVisualDescendants<FretView>() is not { } fvl ||
           fvl.FirstOrDefault(x => x.DataContext == low_note_vm) is not { } low_note_v ||
           fvl.FirstOrDefault(x => x.DataContext == hi_note_vm) is not { } hi_note_v) {

            if(this.GetVisualDescendants<ScrollViewer>().FirstOrDefault(x => x.Classes.Contains("inst-scroll")) is
               { } inst_sv2) {
                // scroll to home when nothing selected
                inst_sv2.ScrollToHome();
            }

            return;
        }

        // if(this.GetVisualDescendant<FretboardView>() is {} fbv &&
        //    low_note_v.GetVisualAncestor<ContentPresenter>() is {} low_note_cp &&
        //    // low_note_cp.TranslatePoint(new(),fbv.FretboardViewbox) is {} low_vb_p &&
        //    // fbv.FretboardViewbox.TranslatePoint(low_vb_p,fbv.FretboardScrollViewer) is {} fb_sv_p &&
        //    this.GetVisualDescendants<ScrollViewer>().FirstOrDefault(x => x.Classes.Contains("inst-scroll")) is
        //    { } inst_sv &&
        //    low_note_cp.TranslatePoint(new(), inst_sv) is {} low_sv_p) {
        //     if(low_sv_p.X > 800) {
        //         
        //     }
        //     void OnOffsetChanged() {
        //         var test = this.GetVisualDescendants<ScrollViewer>()
        //             .FirstOrDefault(x => x.Classes.Contains("inst-scroll"));
        //         if(test.Offset.X < 10) {
        //             
        //         }
        //
        //     }
        //
        //     inst_sv.Offset += new Vector(low_sv_p.X, 0);
        //     if(inst_sv.Offset.X == 0) {
        //         
        //     }
        //     inst_sv.GetObservable(ScrollViewer.OffsetProperty).Subscribe(value => OnOffsetChanged());
        //     // BUG since fret in viewbox(s) BringIntoView always thinks its fret is in view
        //     //inst_sv.ScrollToHorizontalOffset(-low_sv_p.X);
        // } else {
        //     low_note_v.BringIntoView();
        //     hi_note_v.BringIntoView();
        // }
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

    public bool MeasureInstrument() {
        bool success = false;
        if(this.GetVisualDescendant<KeyboardView>() is { } kbv &&
           kbv.IsVisible) {
            success = kbv.MeasureKeyboard();
        } else if(this.GetVisualDescendant<FretboardView>() is { } fbv &&
                  fbv.IsVisible) {
            success = fbv.MeasureFretboard();
        } else {
            success = true;
        }

        InvalidateMeasure();

        if(success) {
            Dispatcher.UIThread.Invoke(
                async () => {
                    while(!IsArrangeValid) {
                        await Task.Delay(10);
                    }

                    ScrollSelectionIntoView();

                });

        }

        return success;
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