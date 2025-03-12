using System;
using Avalonia;
using Avalonia.Controls;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class ChordView : UserControl {
        public ChordView() {
            InitializeComponent();

            this.GetObservable(DataContextProperty).Subscribe(value => OnDataContextChanged());
        }

        void OnDataContextChanged() {

        }

    }
}