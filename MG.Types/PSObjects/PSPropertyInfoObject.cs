using MG.Types.Components;
using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.Types.PSObjects
{
    internal sealed class PSPropertyInfoObject : PSReflectionObject<PropertyInfo, PSPropertyInfoObject>
    {
        static readonly string _typeName = typeof(PSPropertyInfoObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Property;
        protected override int MyNumberOfTypeNames => 2;
        public AccessModifier AccessModifier { get; }
        public Type Type { get; }

        internal PSPropertyInfoObject(PropertyInfo propertyInfo, Type fromType)
            : base(propertyInfo, fromType)
        {
            this.AccessModifier = GetAccessModifier(propertyInfo);
            this.Type = propertyInfo.PropertyType;
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = PSMemberObject.TypeName;
            addToNames[1] = _typeName;
        }

        private static AccessModifier GetAccessModifier(PropertyInfo propertyInfo)
        {
            MethodInfo? getMeth = propertyInfo.GetGetMethod(true);
            MethodInfo? setMeth = propertyInfo.GetSetMethod(true);

            if (setMeth is null)
            {
                return AccessModifierFromGetOrSet(getMeth!, out _);
            }

            if (getMeth is null)
            {
                return AccessModifierFromGetOrSet(setMeth!, out _);
            }

            AccessModifier getAccess = AccessModifierFromGetOrSet(getMeth, out int getNumber);
            AccessModifier setAccess = AccessModifierFromGetOrSet(setMeth, out int setNumber);

            return getAccess | setAccess;
        }

        private static AccessModifier AccessModifierFromGetOrSet(MethodInfo getOrSetMethod, out int number)
        {
            AccessModifier modifier = AccessModifier.None;
            number = 0;

            if (getOrSetMethod.IsPrivate)
            {
                modifier |= AccessModifier.Private;
                
            }
            else if (getOrSetMethod.IsPublic)
            {
                modifier |= AccessModifier.Public;
            }
            else if (getOrSetMethod.IsAssembly)
            {
                modifier |= AccessModifier.Internal;
            }
            else if (getOrSetMethod.IsFamily)
            {
                modifier |= AccessModifier.Protected;
            }
            else if (getOrSetMethod.IsFamilyAndAssembly)
            {
                modifier |= AccessModifier.Private | AccessModifier.Protected;
            }
            else if (getOrSetMethod.IsFamilyOrAssembly)
            {
                modifier |= AccessModifier.Internal | AccessModifier.Protected;
            }

            number = (int)modifier;
            return modifier;
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

