using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MatchesView : UserControl {
        public static MatchesView Instance { get; private set; }

        public MatchesView() {
            if(Instance != null) {
                // singleton error
                //Debugger.Break();
            }

            Instance = this;
            InitializeComponent();
        }

        public void ScrollItemIntoView(MatchViewModel mtvm) {
            if(this.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == mtvm) is not { } mv) {
                return;
            }

            mv.BringIntoView();
        }

        public int GetVisualColCount() {
            if(this.GetVisualDescendants<MatchView>() is { } mvl &&
               mvl.FirstOrDefault() is { } head_mv &&
               head_mv.TranslatePoint(head_mv.Bounds.TopLeft,this) is { } head_tl) {
                return mvl.Count(
                    x => Math.Abs(x.TranslatePoint(x.Bounds.TopLeft,this).Value.Y - head_tl.Y) < double.Epsilon);
            }

            return 1;
        }

        public int GetVisualMatchCount() {
            var mvl = this.GetVisualDescendants<MatchView>().OrderBy(x => x.Bounds.Top).ThenBy(x => x.Bounds.Left);
            int count = 0;
            foreach(MatchView mv in mvl) {
                if(mv.TranslatePoint(mv.Bounds.BottomRight,MatchesScrollViewer) is not { } mv_sv_br_p) {
                    continue;
                }

                if(!MatchesScrollViewer.Bounds.Contains(mv_sv_br_p)) {
                    break;
                }

                count++;
            }

            return count;
        }

        public async Task DoBusyCheckAsync(int delay = 300) {
            if(MatchesBusyOverlay.IsVisible) {
                return;
            }

            MatchesBusyOverlay.IsVisible = true;
            await Task.Delay(delay);
            while(!MatchItemsRepeater.IsArrangeValid) {
                await Task.Delay(100);
            }

            MatchesBusyOverlay.IsVisible = false;
        }
    }
}