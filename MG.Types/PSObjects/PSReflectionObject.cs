using MG.Types.Extensions;
using MG.Types.PSProperties;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace MG.Types.PSObjects
{
    public enum ReflectionType
    {
        Class,
        Interface,
        Method,
        Property,
        Field,
    }

    public abstract class PSReflectionObject : PSObject
    {
        protected static readonly string PSReflectionTypeName = typeof(PSReflectionObject).GetTypeName();
        const int THIS_COUNT = 1;

        protected abstract int MyNumberOfTypeNames { get; }

        public abstract ReflectionType ReflectionType { get; }
        public Type? ParentType { get; private set; }

        protected PSReflectionObject(object baseObj, Type? parentType)
            : base(baseObj)
        {
            ArgumentNullException.ThrowIfNull(baseObj);
            this.ParentType = parentType;
            this.AddThisName();
        }

        private void AddThisName()
        {
            int count = THIS_COUNT + (this.MyNumberOfTypeNames < 0 ? 0 : this.MyNumberOfTypeNames);

            string[] names = ArrayPool<string>.Shared.Rent(count);
            Span<string> span = names.AsSpan(0, count);
            span[0] = PSReflectionTypeName;

            this.AddTypeName(span.Slice(THIS_COUNT));

            foreach (string s in span)
            {
                this.TypeNames.Insert(0, s);
            }

            ArrayPool<string>.Shared.Return(names);
        }
        protected virtual void AddTypeName(Span<string> addToNames)
        {
            return;
        }

        private static int CalculateCapacity(int capacity, int internalCapacity)
        {
            capacity += internalCapacity;
            return capacity >= internalCapacity ? capacity : internalCapacity;
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
            ArgumentNullException.ThrowIfNull(obj);
            this.ReflectionObject = obj;
        }

        public int CompareTo(TSelf? other)
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
        public bool Equals(TSelf? other)
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
