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
        private bool _finished;
        private bool _force;
        private bool _nu;

        private List<Type> ResolvedTypes;
        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetTypeFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetPropertiesFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetMethodsFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("io")]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetTypeFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetFullNameFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetPropertiesFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetMethodsFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetInterfacesFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeFullNameFromName")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        [Alias("t", "Type")]
        public string[] TypeName { get; set; }

        //[Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetFullNameFromName")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        //[Alias("f")]
        //public SwitchParameter FullName { get; set; }

        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFromName")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeFullNameFromName")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        //[Alias("b")]
        //public SwitchParameter BaseType { get; set; }

        //[Parameter(Mandatory = true, ParameterSetName = "GetInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetInterfacesFromName")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetBaseTypeInterfacesFromName")]
        //[Alias("i")]
        //public SwitchParameter Interfaces { get; set; }

        //[Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetPropertiesFromName")]
        //[Alias("p")]
        //public SwitchParameter Properties { get; set; }

        //[Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromPipeline")]
        //[Parameter(Mandatory = true, ParameterSetName = "GetMethodsFromName")]
        //[Alias("m")]
        //public SwitchParameter Methods { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("m")]
        public GetTypeOutput MemberType { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("nu")]
        public SwitchParameter NonUnique
        {
            get => _nu;
            set => _nu = value;
        }

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
            ResolvedTypes = new List<Type>();
        }

        protected override void ProcessRecord()
        {
            if (base.HasParameterSpecified(this, x => x.InputObject))
            {
                if (InputObject is ScriptBlock sb)
                    this.ProcessScriptBlock(sb);

                else if (this.InputObject is Type incomingType)
                    ResolvedTypes.Add(incomingType);

                else if (InputObject is PSObject psObj)
                {
                    this.ProcessPSObject(psObj);
                }

                else if (InputObject is object[] objs)
                    this.ProcessObjectArray(objs);

                else if (this.MemberType == GetTypeOutput.Properties || this.MemberType == GetTypeOutput.Method)
                {
                    base.WriteObject(this.GetMemberCommand(this.InputObject, this.MemberType.ToString(), _force), true);
                    _finished = true;
                }

                else
                    ResolvedTypes.Add(InputObject.GetType());
            }

            else
            {
                foreach (Type t in base.ResolveTypeThroughPowerShell(this.TypeName))
                {
                    if (this.MemberType == GetTypeOutput.Properties)
                    {
                        base.WriteObject(t.GetProperties(), true);
                        _finished = true;
                    }
                    else if (this.MemberType == GetTypeOutput.Method)
                    {
                        base.WriteObject(t.GetMethods(), true);
                        _finished = true;
                    }
                    else
                        ResolvedTypes.Add(t);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (!_finished)
            {
                if (!_nu)
                    ResolvedTypes = ResolvedTypes.Distinct().ToList();

                if (!base.HasParameterSpecified(this, x => x.MemberType))
                    base.WriteObject(ResolvedTypes, true);

                else if (this.MemberType == GetTypeOutput.Interfaces)
                    base.WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.SelectMany(x => x.GetInterfaces()), ResolvedTypes.Count), true);

                else if (this.MemberType == GetTypeOutput.FullName)
                    base.WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes, ResolvedTypes.Count), true);

                else if (this.MemberType == GetTypeOutput.BaseType)
                    base.WriteObject(ResolvedTypes.Select(x => x.BaseType), true);

                else if (this.MemberType == GetTypeOutput.BaseTypeFullName)
                    base.WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.Select(x => x.BaseType), ResolvedTypes.Count), true);

                else if (this.MemberType == GetTypeOutput.BaseTypeInterfaces)
                    base.WriteObject(BaseObject.GetTypeAlias(true, ResolvedTypes.SelectMany(x => x.BaseType.GetInterfaces()), ResolvedTypes.Count), true);
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

        #region PROCESSORS
        private void ProcessObjectArray(params object[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                object o = objs[i];
                if (o != null)
                {
                    if (o is Type t)
                        ResolvedTypes.Add(t);

                    else
                        ResolvedTypes.Add(o.GetType());
                }
            }
        }
        private void ProcessPSObject(PSObject pso)
        {
            if (pso.ImmediateBaseObject is IEnumerable ienum && !(pso.ImmediateBaseObject is string))
            {
                foreach (object o in ienum)
                {
                    ResolvedTypes.Add(o.GetType());
                }
            }
            else if (pso.ImmediateBaseObject is Type t)
                ResolvedTypes.Add(t);

            else
                ResolvedTypes.Add(pso.ImmediateBaseObject.GetType());
            
        }
        private void ProcessScriptBlock(ScriptBlock sb)
        {
            Collection<PSObject> sbResult = sb.Invoke();
            for (int i1 = 0; i1 < sbResult.Count; i1++)
            {
                PSObject one = sbResult[i1];
                if (one.ImmediateBaseObject is Type t)
                {
                    ResolvedTypes.Add(t);
                }
                else if (one.ImmediateBaseObject is IEnumerable<string> strs)
                {
                    ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(strs.ToArray()));
                }
                //else if (one.ImmediateBaseObject is IEnumerable<Type> types)
                else if (one.ImmediateBaseObject is IEnumerable ienum)
                {
                    foreach (object o in ienum)
                    {
                        if (o is Type oType)
                            ResolvedTypes.Add(oType);
                    }
                }
                else if (one.ImmediateBaseObject is string str)
                {
                    ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(str));
                }
            }
        }

        #endregion

        #endregion
    }
}
