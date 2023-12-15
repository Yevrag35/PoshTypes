using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSObjects
{
    internal sealed class PSPropertyInfoObject : PSReflectionObject<PropertyInfo, PSPropertyInfoObject>
    {
        static readonly string _typeName = typeof(PSPropertyInfoObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Property;
        protected override int MyNumberOfTypeNames => 1;

        internal PSPropertyInfoObject(PropertyInfo propertyInfo, Type fromType)
            : base(propertyInfo, fromType)
        {
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = _typeName;
        }

        protected override int ReflectionObjectCompareTo(PropertyInfo thisObj, PropertyInfo other, PSPropertyInfoObject otherParent)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(thisObj.Name, other.Name);
        }
        protected override bool ReflectionObjectEquals(PropertyInfo thisObj, PropertyInfo other)
        {
            return thisObj.Name == other.Name && thisObj.DeclaringType == other.DeclaringType;
        }
    }
}

