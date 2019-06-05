using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Property", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "ByPipelineType")]
    [Alias("getprop", "gpt")]
    [OutputType(typeof(PoshProperty))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetProperty : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private List<Type> list;
        private static readonly string NL = Environment.NewLine;
        private BindingFlags RealFlags;
        private const string LI_ELE = @"<ListItem>
    <PropertyName>{0}</PropertyName>
</ListItem>";
        private const string LI_ELE_COND = @"<ListItem>
    <PropertyName>{0}</PropertyName>
    <ItemSelectionCondition>
        <ScriptBlock>$_.{0} -ne $null</ScriptBlock>
    </ItemSelectionCondition>
</ListItem>";

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByPipelineObject")]
        [Alias("io")]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "ByTypeName")]
        [Alias("Type", "t")]
        public string TypeName { get; set; }

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "ByPipelineObject")]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByTypeName")]
        [Alias("Name", "n")]
        public string[] PropertyName { get; set; }

        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByPipelineObject")]
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = "ByTypeName")]
        [ValidateSet("All", "Get", "GetSet", "Set")]
        public string Accessors = "All";

        [Parameter(Mandatory = false)]
        public BindingFlags[] Flags = new BindingFlags[2] { BindingFlags.Public, BindingFlags.Instance };

        [Parameter(Mandatory = false)]
        public SwitchParameter CSharpFormat { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PSFormatFile { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter UseItemSelectionCondition { get; set; }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            list = new List<Type>();
            RealFlags = base.JoinFlags(Flags);
        }
        protected override void ProcessRecord()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("InputObject"))
            {
                if (InputObject is Type inputType)
                {
                    list.Add(inputType);
                }
                else if (InputObject is PSObject psObj)
                {
                    if (psObj.ImmediateBaseObject.GetType().IsArray)
                    {
                        list.AddRange(base.GetTypesFromArray(psObj.ImmediateBaseObject));
                    }
                    else if (psObj.ImmediateBaseObject is Type type)
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list.Add(psObj.ImmediateBaseObject.GetType());
                    }
                }
            }
            else
            {
                list.AddRange(base.ResolveTypeThroughPowerShell(TypeName));
            }
        }

        protected override void EndProcessing()
        {
            var props = new List<PoshProperty>();
            for (int i = 0; i < list.Count; i++)
            {
                Type t = list[i];
                IEnumerable<PropertyInfo> typeProps = t.GetProperties(RealFlags);
                if (this.MyInvocation.BoundParameters.ContainsKey("PropertyName"))
                {
                    typeProps = typeProps.Where(x => PropertyName.Any(n => n.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase)));
                }
                foreach (PropertyInfo pi in typeProps)
                {
                    props.Add(pi);
                }
            }
            props.Sort(new PoshPropertySorter());
            WriteObject(props, true);
        }

        #endregion

        #region BACKEND METHODS
        private Func<PropertyInfo, bool> GetCondition()
        {
            switch (Accessors)
            {
                case "GetSet":
                    return x => x.CanRead && x.CanWrite;

                case "Set":
                    return x => !x.CanRead && x.CanWrite;

                default:
                    return x => x.CanRead;
            }
        }

        #endregion
    }
}
