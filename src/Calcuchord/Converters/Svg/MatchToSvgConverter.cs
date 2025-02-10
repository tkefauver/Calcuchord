using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class MatchToSvgConverter : IValueConverter {
        public static readonly MatchToSvgConverter Instance = new MatchToSvgConverter();

        ChordSvgBuilder ChordBuilder { get; } = new ChordSvgBuilder();
        ScaleSvgBuilder ScaleBuilder { get; } = new ScaleSvgBuilder();
        PianoSvgBuilder PianoBuilder { get; } = new PianoSvgBuilder();


        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not NoteGroup ng ||
               GetBuilder(ng) is not { } builder ||
               builder.Build(ng,parameter) is not { } htmlNode) {
                return "<svg></svg>";
            }

            return htmlNode.OuterHtml;
        }

        SvgBuilderBase GetBuilder(NoteGroup ng) {
            if(ng.Parent.Parent.Parent.InstrumentType == InstrumentType.Piano) {
                return PianoBuilder;
            }

            if(ng.Parent.PatternType == MusicPatternType.Chords) {
                return ChordBuilder;
            }

            return ScaleBuilder;
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }

}