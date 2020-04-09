using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MG.Posh.Types.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> FilterByStrings<T>(this IEnumerable<T> filterThisCol, Expression<Func<T, string>> propertyExpressionOfCol, 
            IEnumerable<string> withThis)
        {
            if (withThis != null && propertyExpressionOfCol.Body is MemberExpression)
            {
                Func<T, string> propertyFunc = propertyExpressionOfCol.Compile();

                return filterThisCol
                    .Where(x => withThis
                        .Any(s => s
                            .Equals(propertyFunc(x), StringComparison.CurrentCultureIgnoreCase)));
            }
            else
                return filterThisCol;
        }
    }
}
