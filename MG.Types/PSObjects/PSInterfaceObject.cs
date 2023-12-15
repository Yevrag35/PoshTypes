using MG.Types.Extensions;
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
        protected override int MyNumberOfTypeNames => 1;

        internal PSInterfaceObject(Type type, Type parentType)
            : base(type, parentType)
        {
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = _typeName;
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

