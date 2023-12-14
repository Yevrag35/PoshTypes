using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Models
{
    internal sealed class PSInterfaceObject : PSTypeObject
    {
        static readonly string _typeName = typeof(PSInterfaceObject).GetTypeName();

        public override ReflectionType ReflectionType => ReflectionType.Interface;

        internal PSInterfaceObject(Type type, Type parentType)
            : base(type, parentType, new List<string>(1) { _typeName })
        {
            this.Properties.Add(new PSNoteProperty("InterfaceType", type));
            //this.Properties.Add(new PSNoteProperty("ParentType", parentType));
        }
    }
}

