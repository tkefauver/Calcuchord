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
               GetBuilder(ng) is not { } builder ||
               builder.Build(ng,parameter) is not { } htmlNode) {
                return "<svg></svg>";
            }

            return htmlNode.OuterHtml;
        }

        SvgBuilderBase GetBuilder(NotePattern ng) {
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