using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.PSObjects
{
    internal sealed class PSClassObject : PSReflectionObject<Type, PSClassObject>
    {
        static readonly string _typeName = typeof(PSClassObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Class;
        protected override int MyNumberOfTypeNames => 1;

        internal PSClassObject(Type type)
            : base(type, type.BaseType)
        {
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = _typeName;
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

