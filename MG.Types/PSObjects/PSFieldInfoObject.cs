using MG.Types.Components;
using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSObjects
{
    internal sealed class PSFieldInfoObject : PSReflectionObject<FieldInfo, PSFieldInfoObject>
    {
        static readonly string _typeName = typeof(PSFieldInfoObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Field;
        protected override int MyNumberOfTypeNames => 2;
        public AccessModifier AccessModifier { get; }
        public Type Type { get; }

        internal PSFieldInfoObject(FieldInfo fieldInfo, Type fromType)
            : base(fieldInfo, fromType)
        {
            this.AccessModifier = GetAccessModifier(fieldInfo);
            this.Type = fieldInfo.FieldType;
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = PSMemberObject.TypeName;
            addToNames[1] = _typeName;
        }

        private static AccessModifier GetAccessModifier(FieldInfo fieldInfo)
        {
            AccessModifier modifier = AccessModifier.None;

            if (fieldInfo.IsPrivate)
            {
                modifier |= AccessModifier.Private;

            }
            else if (fieldInfo.IsPublic)
            {
                modifier |= AccessModifier.Public;
            }
            else if (fieldInfo.IsAssembly)
            {
                modifier |= AccessModifier.Internal;
            }
            else if (fieldInfo.IsFamily)
            {
                modifier |= AccessModifier.Protected;
            }
            else if (fieldInfo.IsFamilyAndAssembly)
            {
                modifier |= AccessModifier.Private | AccessModifier.Protected;
            }
            else if (fieldInfo.IsFamilyOrAssembly)
            {
                modifier |= AccessModifier.Internal | AccessModifier.Protected;
            }

            return modifier;
        }

        protected override int ReflectionObjectCompareTo(FieldInfo thisObj, FieldInfo other, PSFieldInfoObject otherParent)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(thisObj.Name, other.Name);
        }
        protected override bool ReflectionObjectEquals(FieldInfo thisObj, FieldInfo other)
        {
            return thisObj.Name == other.Name && thisObj.DeclaringType == other.DeclaringType;
        }
    }
}

