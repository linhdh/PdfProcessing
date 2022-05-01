using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace PdfProcessing.Entities.GridPage
{
    public interface IFilter
    {
        bool DoFilter(List<FilterCriteria> filterCriterias);
    }

    public class Filter
    {
        public static Expression<Func<T, bool>> GetExpression<T>(IList<FilterCriteria> filters)
        {
            if (filters.Count == 0)
                return t => true;

            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            Expression exp = null;

            if (filters.Count == 1)
                exp = GetExpression<T>(param, filters[0]);
            else if (filters.Count == 2)
                exp = GetExpression<T>(param, filters[0], filters[1]);
            else
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];

                    if (exp == null)
                        exp = GetExpression<T>(param, filters[0], filters[1]);
                    else
                        exp = Expression.OrElse(exp, GetExpression<T>(param, filters[0], filters[1]));

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.OrElse(exp, GetExpression<T>(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        /// <summary>
        ///  "=", "<>", ">", ">=", "<", "<=", "startswith", "endswith", "contains", "notcontains".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static Expression GetExpression<T>(ParameterExpression param, FilterCriteria filter)
        {
            MethodInfo containsMethod = typeof(String).GetMethod("Contains", new Type[] { typeof(String) });
            MethodInfo startsWithMethod = typeof(String).GetMethod("StartsWith", new Type[] { typeof(String) });
            MethodInfo endsWithMethod = typeof(String).GetMethod("EndsWith", new Type[] { typeof(String) });

            MemberExpression member = Expression.Property(param, filter.FieldName);
            ConstantExpression constant = Expression.Constant(filter.Value);

            switch (filter.FilterType)
            {
                case "=":
                    return Expression.Equal(member, constant);
                case "<>":
                    return Expression.NotEqual(member, constant);
                case ">":
                    return Expression.GreaterThan(member, constant);
                case ">=":
                    return Expression.GreaterThanOrEqual(member, constant);
                case "<":
                    return Expression.LessThan(member, constant);
                case "<=":
                    return Expression.LessThanOrEqual(member, constant);
                case "contains":
                    var toLower = Expression.Call(member,
                                 typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));

                    return Expression.Call(toLower,
                                containsMethod,
                                Expression.Constant(filter.Value.ToLower()));

                case "notcontains":
                    return Expression.Call(member, containsMethod, constant);
                case "startswith":
                    return Expression.Call(member, startsWithMethod, constant);
                case "endswith":
                    return Expression.Call(member, endsWithMethod, constant);
            }

            return null;
        }

        private static BinaryExpression GetExpression<T>
        (ParameterExpression param, FilterCriteria filter1, FilterCriteria filter2)
        {
            Expression bin1 = GetExpression<T>(param, filter1);
            Expression bin2 = GetExpression<T>(param, filter2);

            return Expression.OrElse(bin1, bin2);
        }
    }
}
