using MG.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Type", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "GetTypeFromPipeline")]
    [Alias("gt")]
    [OutputType(typeof(Type))]
    public class GetType : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private const string SCRIPT = @"param([object]$InputObject,[string]$MemberType,[bool]$Force)
return $(Get-Member -InputObject $InputObject -MemberType $MemberType -Force:$Force);
";
        private List<Type> ResolvedTypes;
        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetTypeFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetFullNameFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetPropertiesFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetMethodsFromPipeline")]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetTypeFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetFullNameFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetPropertiesFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetMethodsFromName")]
        [Alias("t", "Type")]
        public string[] TypeName { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromName")]
        [Alias("f")]
        public SwitchParameter FullName { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromName")]
        [Alias("p")]
        public SwitchParameter Properties { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromName")]
        [Alias("m")]
        public SwitchParameter Methods { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            ResolvedTypes = new List<Type>();
        }

        protected override void ProcessRecord()
        {
            if (this.ParameterSetName.Contains("Pipeline"))
            {
                if (InputObject is ScriptBlock sb)
                {
                    Collection<PSObject> sbResult = sb.Invoke();
                    for (int i1 = 0; i1 < sbResult.Count; i1++)
                    {
                        PSObject one = sbResult[i1];
                        if (one.ImmediateBaseObject is Type t)
                        {
                            ResolvedTypes.Add(t);
                        }
                        else if (one.ImmediateBaseObject is IEnumerable<Type> types)
                        {
                            ResolvedTypes.AddRange(types);
                        }
                        else if (one.ImmediateBaseObject is string str)
                        {
                            ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(new string[1] { str }));
                        }
                        else if (one.ImmediateBaseObject is IEnumerable<string> strs)
                        {
                            ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(strs));
                        }
                    }
                    WriteObject(ResolvedTypes);
                }
                else
                {
                    WriteObject(this.GetReturnObjectsFromPipeline());
                }
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("TypeName"))
            {
                ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(TypeName));
                for (int i = 0; i < ResolvedTypes.Count; i++)
                {
                    WriteObject(this.GetReturnObjects(ResolvedTypes[i]));
                }
            }
        }

        #endregion

        #region BACKEND METHODS
        private IEnumerable<object> GetReturnObjects(Type type)
        {
            switch (this.ParameterSetName)
            {
                case "GetFullNameFromPipeline":
                    return new string[1] { type.FullName };

                case "GetFullNameFromName":
                    return new string[1] { type.FullName };

                default:
                    return new Type[1] { type };
            }
        }

        private IEnumerable<object> GetReturnObjectsFromPipeline()
        {
            switch (this.ParameterSetName)
            {
                case "GetPropertiesFromPipeline":
                    return this.GetMemberCommand(InputObject, "Properties", Force.ToBool());

                case "GetMethodsFromPipeline":
                    return this.GetMemberCommand(InputObject, "Methods", Force.ToBool());

                default:
                    if (InputObject is PSObject psObj)
                    {
                        ResolvedTypes.Add(psObj.ImmediateBaseObject.GetType());
                    }
                    else if (InputObject.GetType().IsArray)
                    {
                        foreach (object obj in (object[])InputObject)
                        {
                            ResolvedTypes.Add(obj.GetType());
                        }
                    }
                    return ResolvedTypes;
            }
        }

        private IEnumerable<object> GetMemberCommand(object pipedObject, string memberType, bool force)
        {
            var dict = new Dictionary<string, object>
            {
                { "InputObject", pipedObject },
                { "MemberType", memberType },
                { "Force", force }
            };

            using (var ps = System.Management.Automation.PowerShell.Create().AddScript(SCRIPT).AddParameters(dict))
            {
                var col = ps.Invoke();
                return col;
            }
        }

        #endregion
    }
}
