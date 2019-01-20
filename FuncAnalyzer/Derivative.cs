using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FuncAnalyzer
{
    public class Derivative
    {
        #region Constructor
        public Derivative(Expression<Func<double,double>> f)
        {
            PrimaryFunction = f;
            convertexpression();
        }
        public Derivative(string f)
        {
            PrimaryFunction = CSharpScript.EvaluateAsync<Expression<Func<double,double>>>(f).Result;
            convertexpression();
        }
        #endregion

        #region Properties
        public Expression<Func<double,double>> PrimaryFunction { get; protected set; }
        protected localexpression e;
        #endregion

        #region Methods
            protected void convertexpression()
            {

            }
            public Expression<Func<double,double>> GetDerivative()
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");
                return Expression.Lambda<Func<double,double>>(e.GetDerivative(x), x);
            }
        #endregion

        #region Classes
        protected class localexpression
        {
            public ParameterExpression parameter;
            public localexpression left;
            public localexpression right;
            public virtual Expression Original => Expression.Empty();
            public virtual Expression GetDerivative(ParameterExpression param) { return Expression.Empty(); }
        }

        protected class constant : localexpression
        {
            public constant(double v) { value = v; }
            public double value;
            public override Expression Original => Expression.Constant(value);
            public override Expression GetDerivative(ParameterExpression param) { return Expression.Constant(0); }
        }

        protected class parameter : localexpression
        {
            public new ParameterExpression Original { get; set; }
            public override Expression GetDerivative(ParameterExpression param) { return Expression.Constant(1); }
        }


        protected class addition : localexpression
        {
            public addition(localexpression left, localexpression right) { }
            public override Expression Original => Expression.Add(left.Original, right.Original);
            public override Expression GetDerivative(ParameterExpression param) => Expression.Add(left.GetDerivative(param), right.GetDerivative(param));
            
            public override string ToString() => left.ToString() + "+" + right.ToString();
        }

        protected class substitution : localexpression
        {
            public substitution(localexpression left, localexpression right) { }
            public override Expression Original => Expression.Subtract(left.Original, right.Original);
            public override Expression GetDerivative(ParameterExpression param) => Expression.Subtract(left.GetDerivative(param), right.GetDerivative(param));

            public override string ToString() => left.ToString() + "-" + right.ToString();
        }

        protected class multiplication : localexpression
        {
            public multiplication(localexpression left, localexpression right) { }
            public override Expression Original => Expression.Multiply(left.Original, right.Original);
            public override Expression GetDerivative(ParameterExpression param)
            {
                if (left is constant & right is parameter)
                    return left.Original;
                else if (left is constant & (!(right is constant) & !(right is parameter)))
                    return Expression.Multiply(left.Original, right.GetDerivative(param));
                else if(right is constant & (!(left is constant) & !(left is parameter)))
                    return Expression.Multiply(right.Original, left.GetDerivative(param));
                else
                    return Expression.Add(
                        Expression.Multiply(left.GetDerivative(param), right.Original),
                        Expression.Multiply(left.Original, right.GetDerivative(param))
                        );
            }

            public override string ToString() => left.ToString() + "*" + right.ToString();
        }

        protected class division : localexpression
        {
            public division(localexpression left, localexpression right) { }
            public override Expression Original => Expression.Divide(left.Original, right.Original);
            public override Expression GetDerivative(ParameterExpression param) => 
                Expression.Divide(
                    Expression.Subtract(
                        Expression.Multiply(left.GetDerivative(param),right.Original),
                        Expression.Multiply(left.Original,right.GetDerivative(param))),
                    Expression.Call(null, typeof(Math).GetMethod("Pow"), right.Original, Expression.Constant(2)));

            public override string ToString() => left.ToString() + "/" + right.ToString();
        }

        protected class root : localexpression
        {
            public root(constant left, localexpression right)
            {
                if(left.value)
            }
            public override Expression Original
            {
                get
                {
                    if (left is constant & (left as constant).value == 2.0) return Expression.Call(null, typeof(Math).GetMethod("Sqrt"), right.Original);
                    else if (left is constant) return Expression.Call(null, typeof(Math).GetMethod("Pow"), right.Original, Expression.Divide(Expression.Constant(1), Expression.Constant((left as constant).value)));
                    else if (left is parameter) return Expression.Call(null, typeof(Math).GetMethod("Pow"), right.Original, Expression.Divide(Expression.Constant(1), (left as parameter).parameter));
                    else throw new InvalidOperationException("Left value of root must be a natural");
                }
            }
            public override Expression GetDerivative(ParameterExpression param) =>
                Expression.Divide(
                    Expression.Subtract(
                        Expression.Multiply(left.GetDerivative(param), right.Original),
                        Expression.Multiply(left.Original, right.GetDerivative(param))),
                    Expression.Call(null, typeof(Math).GetMethod("Pow"), right.Original, Expression.Constant(2)));

            public override string ToString() => left.ToString() + "/" + right.ToString();
        }
        #endregion
    }
}
