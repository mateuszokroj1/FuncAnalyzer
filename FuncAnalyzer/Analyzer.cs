using System;
using System.Collections.Generic;

namespace FuncAnalyzer
{
    /// <summary>
    /// Function analyzer
    /// </summary>
    public sealed class Analyzer
    {
        #region Constructor
        public Analyzer()
        {

        }
        #endregion

        #region Properties
        public Func<double,double> Function { get; private set; }
        public Range Definition { get; set; }
        public IEnumerable<double> ZeroPlaces
        {
            get
            {
                double x = 0,
                    min = Definition.Minimum,
                    max = Definition.Maximum;
                if (min == double.NegativeInfinity) min += 0.000000001;
                if (max == double.PositiveInfinity) max -= 0.000000001;
                do
                {
                    try { x = zerosearch(min, max); }
                    catch(ArgumentException) { yield break; }
                    yield return x;
                    min = x + 0.1;
                } while (x < max - 0.5);
                yield break;
            }
        }
        #endregion

        #region Methods
            private double zerosearch(double min, double max)
            {
                double x = 0, E = 0.000000001, fmin = 0, fmax = 0;
                if (!Definition.IsValidValue(min) || !Definition.IsValidValue(max)) throw new ArgumentOutOfRangeException("Min or max is out of function definition");
                try { fmin = Function(min); fmax = Function(max); }
                catch (ArithmeticException) { throw new ArgumentOutOfRangeException("Min or max is out of function definition"); }

                if (fmin * fmax > 0) throw new ArgumentException("Empty result");
                while(Math.Abs(min - max) > E)
                {
                    x = (min-max) / 2.0;
                    if (!Definition.IsValidValue(x)) max = x - E;
                    if (Math.Abs(Function(x)) < E) break;
                    if (fmin * Function(x) < 0) max = x;
                    else { min = x; fmin = Function(x); }
                }
                return x;
            }
        #endregion
    }
}
