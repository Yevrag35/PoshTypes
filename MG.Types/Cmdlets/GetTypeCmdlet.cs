using MG.Types.Attributes;
using MG.Types.Completers;
using MG.Types.Extensions;
using MG.Types.PSObjects;
using MG.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSType", DefaultParameterSetName = PSConstants.FROM_PIPELINE)]
    [Alias("Get-Type")]
    public sealed class GetTypeCmdlet : TypeCmdletBase
    {
        List<PSReflectionObject> _output = null!;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = PSConstants.FROM_PIPELINE)]
        [Alias("Object", "FromObject")]
        [AllowEmptyCollection]
        [AllowEmptyString]
        [ValidateNotNull]
        [RawObjectTransform]
        public object InputObject { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PSConstants.WITH_TYPE)]
        [ArgumentToTypeTransform]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeCompleter))]
        public Type Type { get; set; } = null!;

        [Parameter]
        public SwitchParameter Interfaces { get; set; }

        protected override void Begin()
        {
            _output = new List<PSReflectionObject>(1);
        }
        protected override void Process()
        {
            HashSet<Type> types = this.GetPooledSet();

            Type type = PSConstants.IsFromPipeline(this)
                ? this.InputObject.GetType()
                : this.Type;

            if (this.Interfaces)
            {
                if (!types.Add(type))
                {
                    return;
                }

                Type[] intTypes = type.GetInterfaces();
                foreach (Type t in intTypes)
                {
                    _output.Add(new PSInterfaceObject(t, type));
                }

                return;
            }
            else if (types.Add(type))
            {
                _output.Add(new PSClassObject(type));
            }
        }

        protected override void End()
        {
            this.WriteObject(_output, true);
        }
    }
}

