using MG.Types.Attributes;
using MG.Types.Extensions;
using MG.Types.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSType", DefaultParameterSetName = WITH_TYPE)]
    [Alias("Get-Type")]
    public sealed class GetTypeCmdlet : PSCmdlet
    {
        const string FROM_PIPE = "FromPipeline";
        const string WITH_TYPE = "WithType";
        HashSet<Type> _types = null!;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = FROM_PIPE)]
        [AllowEmptyCollection]
        [AllowEmptyString]
        [ValidateNotNull]
        [RawObjectTransform]
        public object InputObject { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = WITH_TYPE)]
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
            Type type = this.IsFromPipe() ? this.InputObject.GetType() : this.Type;
            if (this.Interfaces && _types.Add(type))
            {
                foreach (Type t in type.GetInterfaces())
                {
                    this.WriteObject(new PSInterfaceObject(t, type));
                }

                return;
            }

            //if (this.Full)
            //{
            //    this.WriteObject(new PSTypeObject(type));
            //}
            //else
            //{
            //    this.WriteObject(new PSTypeObject(this.InputObject));
            //}
        }

        private bool IsFromPipe()
        {
            return FROM_PIPE == this.ParameterSetName;
        }
    }
}

