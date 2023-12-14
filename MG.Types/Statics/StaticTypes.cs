using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types
{
    internal static partial class StaticTypes
    {
        internal static readonly Type Object = typeof(object);
        internal static readonly Type PSObject = typeof(PSObject);
        internal static readonly Type String = typeof(string);
    }
}