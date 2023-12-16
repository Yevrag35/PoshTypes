using MG.Types.Extensions;
using MG.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.PSObjects
{
    internal sealed class PSClassObject : PSReflectionObject<Type, PSClassObject>
    {
        static readonly string _typeName = typeof(PSClassObject).GetTypeName();

        public override ReflectionType ReflectionType { get; }
        public string PSName { get; }

        internal PSClassObject(Type type)
            : this(type, type.BaseType)
        {
        }
        internal PSClassObject(Type type, Type? baseType)
            : base(type, baseType)
        {
            string name = type.GetPSTypeName();
            this.PSName = name;
            this.ReflectionType = type.IsValueType && !type.IsClass
                ? ReflectionType.Struct
                : ReflectionType.Class;

            this.TryAddOrSetPropertyValue(nameof(this.PSName), name);
            this.TryAddOrSetPropertyValue(nameof(this.ReflectionType), this.ReflectionType);
        }

        protected override void AddTypeName()
        {
            if (!_typeName.Equals(this.TypeNames[0]))
            {
                this.TypeNames.Insert(0, PSReflectionTypeName);
                this.TypeNames.Insert(0, PSConstants.PS_TYPE);
                this.TypeNames.Insert(0, _typeName);
            }
        }

        public override PSObject Copy()
        {
            return new PSClassObject(this.ReflectionObject, this.ParentType);
        }

        protected override int ReflectionObjectCompareTo(Type thisObj, Type other, PSClassObject otherParent)
        {
            int code = StringComparer.InvariantCultureIgnoreCase.Compare(thisObj.Namespace, other.Namespace);

            if (code == 0)
            {
                code = StringComparer.InvariantCultureIgnoreCase.Compare(thisObj.Name, other.Name);
            }

            return code;
        }
        protected override bool ReflectionObjectEquals(Type thisObj, Type other)
        {
            return thisObj.Equals(other);
        }
    }
}

