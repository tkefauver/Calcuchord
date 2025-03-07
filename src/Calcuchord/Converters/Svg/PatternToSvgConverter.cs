using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class PatternToSvgConverter : IValueConverter {
        public static readonly PatternToSvgConverter Instance = new PatternToSvgConverter();

        ChordSvgBuilder ChordBuilder { get; } = new ChordSvgBuilder();
        ScaleSvgBuilder ScaleBuilder { get; } = new ScaleSvgBuilder();
        PianoSvgBuilder PianoBuilder { get; } = new PianoSvgBuilder();


        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not NotePattern ng ||
               GetBuilder(ng,true) is not { } builder ||
               builder.Build(ng,parameter) is not { } htmlNode) {
                return "<svg></svg>";
            }

            return htmlNode.OuterHtml;
        }

        public SvgBuilderBase GetBuilder(NotePattern ng,bool common) {
            if(ng.Parent.Parent.Parent.InstrumentType == InstrumentType.Piano) {
                return common ? PianoBuilder : new PianoSvgBuilder();
            }

            if(ng.Parent.PatternType == MusicPatternType.Chords) {
                return common ? ChordBuilder : new ChordSvgBuilder();
            }

            return common ? ScaleBuilder : new ScaleSvgBuilder();
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }

}