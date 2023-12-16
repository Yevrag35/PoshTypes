using MG.Types.PSProperties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types.Extensions
{
    internal static class PSObjectExtensions
    {
        internal static void AddOrSetPropertyValue<T>(this PSObject pso, string name, [MaybeNull] T value)
        {
            Guard.NotNull(pso, nameof(pso));
            Guard.NotNullOrEmpty(name, nameof(name));

            PSPropertyInfo? prop = pso.Properties[name];
            if (prop is null)
            {
                pso.Properties.Add(new PSNoteProperty<T>(name, value));
            }
            else if (prop is PSNoteProperty<T> np)
            {
                np.ValueAsT = value;
            }
            else
            {
                prop.Value = value;
            }
        }
        internal static bool TryAddOrSetPropertyValue<T>(this PSObject pso, string name, [MaybeNull] T value)
        {
            Guard.NotNull(pso, nameof(pso));
            Guard.NotNullOrEmpty(name, nameof(name));

            PSPropertyInfo? prop = pso.Properties[name];
            bool flag = true;

            if (prop is null)
            {
                pso.Properties.Add(new PSNoteProperty<T>(name, value));
            }
            else if (prop is PSNoteProperty<T> np)
            {
                np.ValueAsT = value;
            }
            else if (prop.IsSettable)
            {
                prop.Value = value;
            }
            else
            {
                flag = false;
            }

            return flag;
        }
    }
}

