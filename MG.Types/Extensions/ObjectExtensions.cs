using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Internal;
using System.Management.Automation;
using MG.Types.PSObjects;

namespace MG.Types.Extensions
{
    public static class ObjectExtensions
    {
        public static object? GetBaseObject(this object? obj)
        {
            if (obj is null || !(obj is PSObject mshObj))
            {
                return obj;
            }

            if (AutomationNull.Value.Equals(mshObj))
            {
                return null;
            }

            return PSObject.AsPSObject(mshObj.ImmediateBaseObject).ImmediateBaseObject;
        }
        //[return: NotNullIfNotNull(nameof(obj))]
        //internal static PSTypeObject? GetPSType(this object? obj)
        //{
        //    return (PSTypeObject?)obj?.GetType();
        //}
        public static bool TryGetBaseObject(this object? obj, [NotNullWhen(true)] out object? result)
        {
            result = GetBaseObject(obj);
            return !(result is null);
        }
    }
}