using System;
using Avalonia;
using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public static MainView Instance { get; private set; }

        public MainView() {
            Instance = this;
            InitializeComponent();
            //MidiPlayer.Instance.Init(HiddenWebview);
            EmptyTextBlock.GetObservable(IsVisibleProperty).Subscribe(value => Test());
        }

        void Test() {
            if(EmptyTextBlock.IsVisible) {

            }

        }
    }
}