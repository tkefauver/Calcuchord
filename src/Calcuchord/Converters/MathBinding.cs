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
        public BindingBase a { get; set; }
        public BindingBase b { get; set; }
        public BindingBase c { get; set; }
        public BindingBase d { get; set; }
        public string exp { get; set; }

        public object ProvideValue() {
            MultiBinding mb = new MultiBinding
            {
                Bindings = new[] { a,b,c,d }.Where(x => x != null).Cast<IBinding>().ToList(),
                Converter = new MathMultiValueConverter(exp)
            };

            return mb;
        }

        internal class MathMultiValueConverter : IMultiValueConverter {
            static readonly string[] VariableNames = ["a","b","c","d"];
            Expression Exp { get; }

            internal MathMultiValueConverter(string exp) {
                Exp = new(exp);
            }

            public object Convert(IList<object> values,Type targetType,object parameter,CultureInfo culture) {
                for(int i = 0; i < values.Count; i++) {
                    if(values[i] == null ||
                       !double.TryParse(values[i].ToString(),out double dbl_val) ||
                       !dbl_val.IsNumber()) {
                        continue;
                    }

                    Exp.Bind(VariableNames[i],dbl_val);
                }

                var errors = Exp.GetError();
                if(errors.Count == 0) {
                    double result = Exp.Eval<double>();
                    return result;
                }

                // get variables
                var variables = Exp.getVariables();
                foreach(string variable in variables) {
                    Console.WriteLine(variable); // will print x, a
                }

                return null;
            }
        }
    }


}