using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Models
{
    internal class PSTypeObject : PSReflectionObject<Type>
    {
        static readonly string _typeName = typeof(PSTypeObject).GetTypeName();
        public override ReflectionType ReflectionType => this.ReflectionObject.IsInterface
            ? ReflectionType.Interface
            : ReflectionType.Class;

        internal PSTypeObject(Type type)
            : base(type, type.BaseType, new List<string>(1) { _typeName })
        {
        }

        protected PSTypeObject(Type type, Type? parentType, IList<string> names)
            : base(type, parentType, AddTypeName(_typeName, names))
        {
        }

        protected override PSReflectionObject Copy(IList<string> customTypes)
        {
            
        }

        public static explicit operator PSTypeObject?(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            else if (!type.IsClass || !type.IsInterface)
            {
                throw new ArgumentException("Type must be a class or interface.");
            }

            return new PSTypeObject(type);
        }
    }
}

