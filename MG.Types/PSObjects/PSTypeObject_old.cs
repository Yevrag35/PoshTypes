using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Models
{
    public class PSTypeObject : PSObject, IComparable<PSTypeObject>, IEquatable<PSTypeObject>, IEquatable<Type>
    {
        static readonly string _typeName = typeof(PSTypeObject).GetTypeName();
        readonly Type _type;
        readonly string _objAsStr;
        Type[]? _interfaces;

        protected Type BackingType => _type;
        protected bool IsAbstract => _type.IsAbstract;
        protected bool IsClass => _type.IsClass;
        protected bool IsEnum => _type.IsEnum;
        protected bool IsInterface => _type.IsInterface;
        protected bool IsSealed => _type.IsSealed;
        protected bool IsStatic => this.IsAbstract && this.IsSealed;
        public string FullAssemblyName { get; }
        public string FullName { get; }
        public string Name => _type.Name;
        public string PSName { get; }

        public PSTypeObject(Type type)
            : base(type)
        {
            _type = type;
            _objAsStr = string.Empty;
            this.FullAssemblyName = type.GetFullAssemblyTypeName();
            this.FullName = type.GetTypeName();
            this.PSName = type.GetPSTypeName();
            this.AddTypeName();
        }
        public PSTypeObject(object obj)
            : base(2)
        {
            Guard.NotNull(obj, nameof(obj));
            _type = obj.GetType();
            this.Properties.Add(new PSNoteProperty("Object", obj.ToString()));
            this.Properties.Add(new PSNoteProperty("TypeName", _type.GetPSTypeName()));
            this.Properties.Add(new PSNoteProperty("Type", _type));
            this.AddTypeName();
        }
        protected PSTypeObject(Type type, Type[] interfaces)
            : this(type)
        {
            _interfaces = interfaces;
        }

        protected virtual void AddTypeName()
        {
            this.TypeNames.Insert(0, _typeName);
        }
        [DebuggerStepThrough]
        public int CompareTo(PSTypeObject? other)
        {
            int code = StringComparer.InvariantCultureIgnoreCase.Compare(_type.Namespace, other?._type.Namespace);

            if (code == 0)
            {
                code = StringComparer.InvariantCultureIgnoreCase.Compare(_type.Name, other?._type.Name);
            }

            return code;
        }
        [DebuggerStepThrough]
        public bool Equals(PSTypeObject? other)
        {
            return ReferenceEquals(this, other) || _type.Equals(other?._type);
        }
        public bool Equals(Type? other)
        {
            return _type.Equals(other);
        }
        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }

        [DebuggerStepThrough]
        public Type[] GetInterfaces()
        {
            return _interfaces ??= _type.GetInterfaces();
        }
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(type))]
        public static implicit operator PSTypeObject?(Type? type)
        {
            return type is null ? null : new PSTypeObject(type);
        }
        [DebuggerStepThrough]
        public static implicit operator Type(PSTypeObject pso)
        {
            return pso._type;
        }
    }
}

