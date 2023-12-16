using MG.Types.Attributes;
using MG.Types.Extensions;
using MG.Types.PSObjects;
using MG.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSType", DefaultParameterSetName = PSConstants.WITH_TYPE)]
    [Alias("Get-Type")]
    public sealed class GetTypeCmdlet : TypeCmdletBase
    {
        HashSet<Type> _types = null!;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = PSConstants.FROM_PIPELINE)]
        [AllowEmptyCollection]
        [AllowEmptyString]
        [ValidateNotNull]
        [RawObjectTransform]
        public object InputObject { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PSConstants.WITH_TYPE)]
        [ArgumentToTypeTransform]
        [ValidateNotNull]
        public Type Type { get; set; } = null!;

        [Parameter]
        public SwitchParameter Full { get; set; }

        [Parameter]
        public SwitchParameter Interfaces { get; set; }

        protected override void BeginProcessing()
        {
            _types = new HashSet<Type>();
        }

        protected override void ProcessRecord()
        {
            Type type = PSConstants.IsFromPipeline(this)
                ? this.InputObject.GetType()
                : this.Type;

            if (this.Interfaces)
            {
                if (!_types.Add(type))
                {
                    return;
                }

                foreach (Type t in type.GetInterfaces())
                {
                    this.WriteObject(new PSInterfaceObject(t, type));
                }

                return;
            }
            else if (_types.Add(type))
            {
                this.WriteObject(new PSClassObject(type));
            }
        }
    }
}

