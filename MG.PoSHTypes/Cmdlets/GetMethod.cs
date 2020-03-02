using MG.Posh.Extensions.Bound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Method", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "ByPipelineType")]
    [Alias("gmt")]
    [OutputType(typeof(PoshMethod))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetMethod : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private bool _force;
        private List<Type> list;
        private BindingFlags realFlags;

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByPipelineType")]
        [Alias("io")]
        public Type InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByTypeName")]
        [Alias("Type", "t")]
        public string TypeName { get; set; }

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "ByPipelineType")]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByTypeName")]
        [Alias("Name", "n")]
        public string[] MethodName { get; set; }

        [Parameter(Mandatory = false)]
        public BindingFlags[] Flags = new BindingFlags[2] { BindingFlags.Public, BindingFlags.Instance };

        [Parameter(Mandatory = false)]
        public SwitchParameter Force
        {
            get => _force;
            set => _force = value;
        }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = new List<Type>();
            realFlags = base.JoinFlags(Flags);
        }

        protected override void ProcessRecord()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("InputObject"))
                list.Add(InputObject);

            else
                list.AddRange(base.ResolveTypeThroughPowerShell(TypeName));
        }

        protected override void EndProcessing()
        {
            var outList = new List<PoshMethod>();
            for (int i = 0; i < list.Count; i++)
            {
                Type t = list[i];
                IEnumerable<MethodInfo> allMethods = t.GetMethods(realFlags);
                if (this.ContainsParameter(x => x.MethodName))
                {
                    allMethods = allMethods.Where(x => MethodName.Any(n => n.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase)));
                }
                else if (!_force)
                {
                    allMethods = allMethods.Where(x => !x.Name.Contains("_"));
                }

                foreach (MethodInfo mi in allMethods)
                {
                    outList.Add(mi);
                }
            }
            outList.Sort(new PoshMethodSorter());

            WriteObject(outList, true);
        }

        #endregion
    }
}