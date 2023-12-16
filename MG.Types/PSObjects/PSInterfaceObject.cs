using MG.Types.Extensions;
using MG.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.PSObjects
{
    internal sealed class PSInterfaceObject : PSReflectionObject<Type, PSInterfaceObject>
    {
        static readonly string _typeName = typeof(PSInterfaceObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Interface;
        public string PSName { get; }

        internal PSInterfaceObject(Type type, Type parentType)
            : base(type, parentType)
        {
            string name = type.GetPSTypeName();
            this.PSName = name;
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
            return new PSInterfaceObject(this.ReflectionObject, this.ParentType!);
        }

        protected override int ReflectionObjectCompareTo(Type thisObj, Type other, PSInterfaceObject otherParent)
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

