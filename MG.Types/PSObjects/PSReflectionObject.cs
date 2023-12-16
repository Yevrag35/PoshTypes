using MG.Types.Extensions;
using MG.Types.PSProperties;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace MG.Types.PSObjects
{
    public enum ReflectionType
    {
        Class,
        Interface,
        Struct,
        Method,
        Property,
        Field,
    }

    public abstract class PSReflectionObject : PSObject
    {
        protected static readonly string PSReflectionTypeName = typeof(PSReflectionObject).GetTypeName();
        public abstract ReflectionType ReflectionType { get; }
        public Type? ParentType { get; private set; }

        protected PSReflectionObject(object baseObj, Type? parentType)
            : base(GetNonReflectionObject(baseObj))
        {
            this.ParentType = parentType;
            this.TryAddOrSetPropertyValue(nameof(this.ParentType), parentType);
            this.AddTypeName();
        }

        protected abstract void AddTypeName();

        private static object GetNonReflectionObject(object baseObj)
        {
            return baseObj is PSObject psro ? psro.BaseObject : baseObj;
        }
    }

    public abstract class PSReflectionObject<T, TSelf> : PSReflectionObject, IComparable<TSelf>, IEquatable<TSelf>
        where T : notnull
        where TSelf : PSReflectionObject<T, TSelf>
    {
        public T ReflectionObject { get; }

        protected PSReflectionObject(T obj, Type? parentType)
            : base(obj, parentType)
        {
            Guard.NotNull(obj, nameof(obj));
            this.ReflectionObject = obj;
        }

#if NET6_0_OR_GREATER
        public int CompareTo(TSelf? other)
#else
        public int CompareTo([MaybeNull] TSelf other)
#endif
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            else if (other is null)
            {
                return -1;
            }

            return this.ReflectionObjectCompareTo(this.ReflectionObject, other.ReflectionObject, other);
        }

#if NET6_0_OR_GREATER
        public bool Equals(TSelf? other)
#else
        public bool Equals([MaybeNull] TSelf other)
#endif
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            else if (other is null || this.ReflectionType != other.ReflectionType)
            {
                return false;
            }

            return this.ReflectionObjectEquals(this.ReflectionObject, other.ReflectionObject);
        }
        protected abstract int ReflectionObjectCompareTo(T thisObj, T other, TSelf otherParent);
        protected abstract bool ReflectionObjectEquals(T thisObj, T other);
    }
}
