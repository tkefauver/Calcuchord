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
        public bool IsBoolResult { get; set; }
        public string exp { get; set; }

        public object ProvideValue() {
            MultiBinding mb = new MultiBinding
            {
                Bindings = new[] { a,b,c,d }.Where(x => x != null).Cast<IBinding>().ToList(),
                Converter = new MathMultiValueConverter(exp,IsBoolResult)
            };

            return mb;
        }

        internal class MathMultiValueConverter : IMultiValueConverter {
            static readonly string[] VariableNames = ["a","b","c","d"];
            Expression Exp { get; }
            bool IsBoolResult { get; }

            internal MathMultiValueConverter(string exp,bool isBoolResult) {
                Exp = new(exp);
                IsBoolResult = isBoolResult;
                if(IsBoolResult) {

                }
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
                    if(IsBoolResult) {
                        int bool_result = Exp.Eval<int>();
                        if(bool_result == 1) {

                        }

                        // test
                        return bool_result == 1;
                    }

                    double result = Exp.Eval<double>();
                    if(!result.IsNumber() || result == 0) {

                    }

                    return result;
                }

                // get variables
                var variables = Exp.getVariables();
                foreach(string variable in variables) {
                    PlatformWrapper.Services.Logger.WriteLine(variable); // will print x, a
                }

                return null;
            }
        }
    }


}