using MG.Dynamic;
using System;
using System.Collections;
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
    [CmdletBinding(PositionalBinding = false)]
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
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetInterfacesFromPipeline")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("io")]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetTypeFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetFullNameFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetPropertiesFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetMethodsFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFromName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetInterfacesFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFullNameFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("t", "Type")]
        public string[] TypeName { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromName")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        [Alias("f")]
        public SwitchParameter FullName { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFromName")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("b")]
        public SwitchParameter BaseType { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetInterfacesFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetInterfacesFromName")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        [Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("i")]
        public SwitchParameter Interfaces { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromName")]
        [Alias("p")]
        public SwitchParameter Properties { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromName")]
        [Alias("m")]
        public SwitchParameter Methods { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("nu")]
        public SwitchParameter NonUnique { get; set; }

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
            if (!this.MyInvocation.BoundParameters.ContainsKey("Properties") && !this.MyInvocation.BoundParameters.ContainsKey("Methods"))
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
                                ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(str));
                            }
                            else if (one.ImmediateBaseObject is IEnumerable<string> strs)
                            {
                                ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(strs.ToArray()));
                            }
                        }
                    }
                    else if (InputObject is PSObject psObj)
                    {
                        if (psObj.ImmediateBaseObject.GetType().IsArray)
                        {
                            foreach (object obj in (IEnumerable)psObj.ImmediateBaseObject)
                            {
                                ResolvedTypes.Add(obj.GetType());
                            }
                        }
                        else
                        {
                            ResolvedTypes.Add(psObj.ImmediateBaseObject.GetType());
                        }
                    }
                    else if (InputObject is object[])
                    {
                        ResolvedTypes.Add(typeof(object[]));
                    }
                    else
                        ResolvedTypes.Add(InputObject.GetType());
                }
                else if (this.MyInvocation.BoundParameters.ContainsKey("TypeName"))
                {
                    ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(TypeName));
                }
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("Properties"))
            {
                WriteObject(this.GetMemberCommand(InputObject, "Properties", Force.ToBool()), true);
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("Methods"))
            {
                WriteObject(this.GetMemberCommand(InputObject, "Methods", Force.ToBool()), true);
            }
        }

        protected override void EndProcessing()
        {
            if (!this.MyInvocation.BoundParameters.ContainsKey("NonUnique"))
                ResolvedTypes = ResolvedTypes.Distinct().ToList();

            if (!this.MyInvocation.BoundParameters.ContainsKey("Properties") && !this.MyInvocation.BoundParameters.ContainsKey("Methods")
                && !this.MyInvocation.BoundParameters.ContainsKey("FullName") && !this.MyInvocation.BoundParameters.ContainsKey("BaseType")
                && !this.MyInvocation.BoundParameters.ContainsKey("Interfaces"))
            {
                WriteObject(ResolvedTypes, true);
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("BaseType"))
            {
                if (this.MyInvocation.BoundParameters.ContainsKey("Interfaces"))
                {
                    base.WriteObject(ResolvedTypes.SelectMany(x => x.BaseType.GetInterfaces()).ToArray(), true);
                }
                else if (this.MyInvocation.BoundParameters.ContainsKey("FullName"))
                {
                    base.WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.Select(x => x.BaseType).ToArray()), true);
                }
                else
                {
                    WriteObject(ResolvedTypes.Select(x => x.BaseType), true);
                }
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("Interfaces"))
            {
                WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.SelectMany(x => x.GetInterfaces()).ToArray()), true);
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("FullName"))
            {
                WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.ToArray()));
            }
        }

        #endregion

        #region BACKEND METHODS

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
