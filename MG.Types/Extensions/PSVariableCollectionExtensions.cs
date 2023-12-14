using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Extensions
{
    internal static class PSVariableCollectionExtensions
    {
        [return: NotNullIfNotNull(nameof(defaultIfNull))]
        internal static T? GetFirstValue<T>(this Collection<PSObject>? collection, Func<object, T?> convert, T? defaultIfNull = default)
        {
            if (collection is null || collection.Count <= 0 || !collection[0].TryGetBaseObject(out object? o))
            {
                return defaultIfNull;
            }

            try
            {
                return convert(o) ?? defaultIfNull;
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                return defaultIfNull;
            }
        }
    }
}

