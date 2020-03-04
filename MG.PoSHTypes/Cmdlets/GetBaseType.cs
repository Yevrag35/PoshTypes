using MG.Dynamic;
using MG.Posh.Extensions.Bound;
using MG.Posh.Extensions.Pipe;
using Microsoft.PowerShell.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "BaseType", ConfirmImpact = ConfirmImpact.None)]
    [OutputType(typeof(Type))]
    [Alias("gbt")]
    [CmdletBinding(PositionalBinding = false)]
    public class GetBaseType : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private List<Type> ResolvedTypes;

        private bool _full;
        private bool _int;
        private bool _nu;

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetTypeFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetFullNameFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetInterfaceFromPipeline")]
        [Alias("io")]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetTypeFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetFullNameFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetInterfaceFromName")]
        [Alias("t", "Type")]
        public string[] TypeName { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromName")]
        [Alias("f")]
        public SwitchParameter ShowFullName
        {
            get => _full;
            set => _full = value;
        }

        [Parameter(Mandatory = true, ParameterSetName = "GetInterfaceFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetInterfaceFromName")]
        [Alias("i")]
        public SwitchParameter ShowInterfaces
        {
            get => _int;
            set => _int = value;
        }

        [Parameter(Mandatory = false)]
        [Alias("nu", "ShowAllTypes")]
        public SwitchParameter NonUnique
        {
            get => _nu;
            set => _nu = value;
        }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            ResolvedTypes = new List<Type>();
        }

        protected override void ProcessRecord()
        {
            if (this.ContainsParameter(x => x.InputObject))
            {
                this.ProcessInputObject(this.InputObject, ref ResolvedTypes);
            }
            else
            {
                base.AddStringTypesToResolved(this.TypeName, ref ResolvedTypes);
            }
        }

        protected override void EndProcessing()
        {
            if (ResolvedTypes.Count > 0)
            {
                IEnumerable<Type> types = ResolvedTypes.Distinct();
                if (_nu)
                    types = ResolvedTypes;

                if (this.ContainsAnyParameters(x => x.ShowInterfaces, x => x.ShowFullName))
                {
                    if (this.ContainsParameter(x => x.ShowFullName))
                    {
                        base.WriteObject(types.Select(x => x.BaseType.FullName), true);
                    }
                    else
                    {
                        base.WriteObject(BaseObject.GetTypeAlias(true, types.SelectMany(x => x.BaseType.GetInterfaces()), types.Count()), true);
                    }
                }
                else
                {
                    base.WriteObject(types.Select(x => x.BaseType), true);
                }
            }
        }

        #endregion

        #region BACKEND METHODS


        #endregion
    }
}