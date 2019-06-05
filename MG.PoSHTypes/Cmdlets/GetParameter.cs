using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Parameter", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "NonPipeline")]
    [Alias("gpm", "pm")]
    [OutputType(typeof(PoshMethodParameter))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetParameter : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private List<PoshMethodParameter> parameters;

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NonPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ViaPipeline")]
        public MethodInfo Method { get; set; }

        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "NonPipeline")]
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "ViaPipeline")]
        [Alias("Name", "n")]
        public string[] ParameterdName { get; set; }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing() => parameters = new List<PoshMethodParameter>();

        protected override void ProcessRecord()
        {
            IEnumerable<ParameterInfo> allParams = Method.GetParameters();
            if (this.MyInvocation.BoundParameters.ContainsKey("MethodName"))
            {
                allParams = allParams.Where(x => ParameterdName.Any(n => n.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase)));
            }
            foreach (PoshMethodParameter pmp in allParams)
            {
                parameters.Add(pmp);
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(parameters, true);
        }

        #endregion

        #region BACKEND METHODS


        #endregion
    }
}
