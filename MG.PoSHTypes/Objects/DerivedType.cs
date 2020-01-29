using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.PowerShell.Types
{
    public static class DerivedType
    {
        public static List<Type> GetDerivedTypes(string baseType, IEnumerable<Assembly> assemblies, bool recurse = true)
        {
            var list = new List<Type>();
            foreach (Assembly ass in assemblies)
            {
                foreach (Type type in ass.ExportedTypes)
                {
                    if (type.BaseType != null && !string.IsNullOrEmpty(type.BaseType.FullName) && type.BaseType.FullName.StartsWith(baseType))
                    {
                        list.Add(type);
                        if (recurse)
                        {
                            list.AddRange(GetDerivedTypes(type.FullName, assemblies, recurse));
                        }
                    }
                }
            }
            return list;
        }
    }
}
