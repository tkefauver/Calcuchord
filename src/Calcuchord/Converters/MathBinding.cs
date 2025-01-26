using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using org.matheval;

namespace Calcuchord {
    public class MathBinding {
        public BindingBase? a { get; set; }
        public BindingBase? b { get; set; }
        public BindingBase? c { get; set; }
        public BindingBase? d { get; set; }
        public object? _a { get; set; }
        public object? _b { get; set; }
        public object? _c { get; set; }
        public object? _d { get; set; }
        public string exp { get; set; }

        public object ProvideValue() {
            MultiBinding mb = new MultiBinding
            {
                Bindings = new[] { a,b,c,d }.Where(x => x != null).Cast<IBinding>().ToList(),
                //Converter = new FuncMultiValueConverter<double, double>(doubles => doubles.Aggregate(1d, (x, y) => x * y))
                Converter = new MathMultiValueConverter(exp,[_a,_b,_c,_d])
            };

            return mb;
        }
    }

    public class MathMultiValueConverter : IMultiValueConverter {
        static readonly string[] VariableNames = ["a","b","c","d"];
        double?[] Literals { get; }
        Expression Exp { get; }

        public MathMultiValueConverter(string exp,object[] literals) {
            Literals = literals.Select(x => int.TryParse(x.ToStringOrDefault(),out int i) ? new double?(i) : null)
                .ToArray();
            Exp = new(exp);
        }

        public object Convert(IList<object> values,Type targetType,object parameter,CultureInfo culture) {
            for(int i = 0; i < VariableNames.Length; i++) {
                double? cur_val = null;
                if(i < values.Count &&
                   values[i] is { } val_obj &&
                   double.TryParse(val_obj.ToStringOrDefault(),out double bound_val)) {
                    cur_val = bound_val;
                } else if(Literals[i] is { } lit_val) {
                    cur_val = lit_val;
                }

                if(cur_val is { } cur_dbl &&
                   cur_dbl.IsNumber()) {
                    Exp.Bind(VariableNames[i],cur_dbl);
                }
            }

            if(Exp.getVariables().Any()) {
                try {
                    double result = Exp.Eval<double>();
                    return result;
                } catch(Exception ex) {
                    //ex.Dump();
                }
            }

            return null;
        }
    }
}