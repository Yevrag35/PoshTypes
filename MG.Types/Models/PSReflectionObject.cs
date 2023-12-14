using MG.Types.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace MG.Types.Models
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
        readonly IList<string> _customTypes;

        public abstract ReflectionType ReflectionType { get; }
        public Type? ParentType { get; private set; }

        protected PSReflectionObject(object baseObj, Type? parentType, string? typeName)
            : base(baseObj)
        {
            ArgumentNullException.ThrowIfNull(baseObj);
            this.ParentType = parentType;
            this.TypeNames.Insert(0, PSReflectionTypeName);
            _customTypes = new List<string>(1) { PSReflectionTypeName };

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                _customTypes.Insert(0, typeName);
                this.TypeNames.Insert(0, typeName);
            }
        }
        protected PSReflectionObject(object baseObj, Type? parentType, IList<string> typeNames)
            : base(baseObj)
        {
            ArgumentNullException.ThrowIfNull(baseObj);
            ArgumentNullException.ThrowIfNull(typeNames);
            typeNames.Add(PSReflectionTypeName);

            this.ParentType = parentType;
            _customTypes = typeNames;

            if (typeNames.Count > 0)
            {
                int total = this.TypeNames.Count + typeNames.Count;
                string[] rented = ArrayPool<string>.Shared.Rent(total);
                this.InsertTypeNames(total, rented, typeNames);
            }
        }
        protected PSReflectionObject(object baseObj, Type? parentType, IList<string> typeNames, bool useNamesAsIs)
        {
            ArgumentNullException.ThrowIfNull(baseObj);
            ArgumentNullException.ThrowIfNull(typeNames);
            typeNames.Add(PSReflectionTypeName);

            this.ParentType = parentType;
            _customTypes = typeNames;
            if (useNamesAsIs)
            {
                this.TypeNames.Clear();
                foreach (string name in typeNames)
                {
                    this.TypeNames.Add(name);
                }
            }
            else if (typeNames.Count > 0)
            {
                int total = this.TypeNames.Count + typeNames.Count;
                string[] rented = ArrayPool<string>.Shared.Rent(total);
                this.InsertTypeNames(total, rented, typeNames);
            }
        }
        protected static IList<string> AddTypeName(string typeName, IList<string> list)
        {
            list.Add(typeName);
            return list;
        }
        public override PSObject Copy()
        {
            return this.Copy(_customTypes);
        }
        protected abstract PSReflectionObject Copy(IList<string> customTypes);
        private void InsertTypeNames(int count, string[] rented, IList<string> typeNames)
        {
            this.TypeNames.CopyTo(rented, typeNames.Count);

            typeNames.CopyTo(rented, 0);
            this.TypeNames.Clear();

            Span<string> reversed = rented.AsSpan(0, typeNames.Count);
            reversed.Reverse();

            foreach (string newName in reversed)
            {
                this.TypeNames.Add(newName);
            }

            foreach (string name in rented.AsSpan(typeNames.Count, count - typeNames.Count))
            {
                this.TypeNames.Add(name);
            }

            ArrayPool<string>.Shared.Return(rented);
        }
    }

    public abstract class PSReflectionObject<T> : PSReflectionObject where T : notnull
    {
        public T ReflectionObject { get; }

        protected PSReflectionObject(T obj, Type? parentType, IList<string> typeNames)
            : base(obj, parentType, typeNames)
        {
            ArgumentNullException.ThrowIfNull(obj);
            this.ReflectionObject = obj;
        }
    }
}
