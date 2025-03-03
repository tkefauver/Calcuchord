using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using org.matheval;

namespace Calcuchord {
    public class MathBinding {
        public BindingBase a { get; set; }
        public BindingBase b { get; set; }
        public BindingBase c { get; set; }
        public BindingBase d { get; set; }
        public bool IsBoolResult { get; set; }
        public string exp { get; set; }

        public object ProvideValue() {
            MultiBinding mb = new MultiBinding
            {
                Bindings = new[] { a,b,c,d }.Where(x => x != null).Cast<IBinding>().ToList(),
                Converter = new MathMultiValueConverter(exp,IsBoolResult),
            };

            return mb;
        }

        internal class MathMultiValueConverter : IMultiValueConverter {
            static readonly string[] VariableNames = ["a","b","c","d"];
            Expression Exp { get; }
            bool IsBoolResult { get; }

            internal MathMultiValueConverter(string exp,bool isBoolResult) {
                Exp = new Expression(exp);
                IsBoolResult = isBoolResult;
            }

            public object Convert(IList<object> values,Type targetType,object parameter,CultureInfo culture) {
                for(int i = 0; i < values.Count; i++) {
                    if(values[i] == null ||
                       !double.TryParse(values[i].ToString(),out double dbl_val) ||
                       double.IsNaN(dbl_val) ||
                       double.IsPositiveInfinity(dbl_val) ||
                       double.IsNegativeInfinity(dbl_val)) {
                        // when variable unset, undefined, NAN etc. default to 0
                        dbl_val = 0;
                    }

                    Exp.Bind(VariableNames[i],dbl_val);
                }

                if(Exp.GetError().Count > 0) {
                    // handle error (dumps variables)
                    var variables = Exp.getVariables();
                    foreach(string variable in variables) {
                        Console.WriteLine(variable); // will print x, a
                    }

                    return IsBoolResult ? false : 0;
                }


                double result = Exp.Eval<double>();
                if(IsBoolResult) {
                    // example:
                    // exp: IF(a+5<b,1,0)
                    // ie: IF(cond,true val,false val)
                    return (int)result == 1;
                }

                return result;
            }
        }
    }


}