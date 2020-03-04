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
    [Cmdlet(VerbsCommon.Get, "Type", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "GetTypeFromName")]
    [Alias("gt")]
    [OutputType(typeof(Type))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetType : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        //private bool _finished;
        //private bool _force;
        private bool _full;
        private bool _int;
        private bool _nu;

        private List<Type> ResolvedTypes;
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

        //[Parameter(Mandatory = false)]
        //[Alias("m")]
        //public GetTypeOutput MemberType { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("nu", "ShowAllTypes")]
        public SwitchParameter NonUnique
        {
            get => _nu;
            set => _nu = value;
        }

        //[Parameter(Mandatory = false)]
        //public SwitchParameter Force
        //{
        //    get => _force;
        //    set => _force = value;
        //}

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
                this.AddStringTypesToResolved(this.TypeName, ref ResolvedTypes);
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
                        base.WriteObject(types.Select(x => x.FullName), true);
                    }
                    else
                    {
                        base.WriteObject(BaseObject.GetTypeAlias(true, types.SelectMany(x => x.GetInterfaces()), types.Count()), true);
                    }
                }
                else
                {
                    base.WriteObject(types, true);
                }
            }
        }
        /*
        protected override void ProcessRecord()
        {
            if (this.ContainsParameter(x => x.InputObject))
            {
                if (this.InputObject.BaseObject is ScriptBlock sb)
                    this.ProcessScriptBlock(sb);

                else if (this.InputObject.BaseObject is Type incomingType)
                    ResolvedTypes.Add(incomingType);

                else if (this.InputObject.ImmediateBaseObject is object[] objs)
                    this.ProcessObjectArray(objs);

                else if (this.MemberType == GetTypeOutput.Properties || this.MemberType == GetTypeOutput.Method)
                {
                    base.WriteObject(this.GetMemberCommand(this.InputObject, this.MemberType.ToString(), _force), true);
                    _finished = true;
                }

                else //if (InputObject.BaseObject is PSObject psObj)
                {
                    ResolvedTypes.Add(this.InputObject.ImmediateBaseObject.GetType());
                }
                //else
                    //ResolvedTypes.Add(InputObject.GetType());
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

                if ( ! this.ContainsParameter(x => x.MemberType))
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
        */
        #endregion

        #region BACKEND METHODS

        //private IEnumerable<MemberDefinition> GetMemberCommand(object pipedObject, string memberType, bool force)
        //{
        //    var cmdlet = new GetMemberCommand
        //    {
        //        Force = force,
        //        InputObject = PSObject.AsPSObject(pipedObject),
        //        MemberType = (PSMemberTypes)Enum.Parse(typeof(PSMemberTypes), memberType)
        //    };
        //    return cmdlet.Invoke<MemberDefinition>();
        //}

        #region PROCESSORS

        //private void ProcessObjectArray(params object[] objs)
        //{
        //    for (int i = 0; i < objs.Length; i++)
        //    {
        //        object o = objs[i];
        //        if (o != null)
        //        {
        //            if (o is Type t)
        //                ResolvedTypes.Add(t);

        //            else
        //                ResolvedTypes.Add(o.GetType());
        //        }
        //    }
        //}
        //private void ProcessPSObject(PSObject pso)
        //{
        //    if (pso.ImmediateBaseObject is IEnumerable ienum && !(pso.ImmediateBaseObject is string))
        //    {
        //        foreach (object o in ienum)
        //        {
        //            ResolvedTypes.Add(o.GetType());
        //        }
        //    }
        //    else if (pso.ImmediateBaseObject is Type t)
        //        ResolvedTypes.Add(t);

        //    else
        //        ResolvedTypes.Add(pso.ImmediateBaseObject.GetType());
            
        //}
        //private void ProcessScriptBlock(ScriptBlock sb)
        //{
        //    Collection<PSObject> sbResult = sb.Invoke();
        //    for (int i1 = 0; i1 < sbResult.Count; i1++)
        //    {
        //        PSObject one = sbResult[i1];
        //        if (one.ImmediateBaseObject is Type t)
        //        {
        //            ResolvedTypes.Add(t);
        //        }
        //        else if (one.ImmediateBaseObject is IEnumerable<string> strs)
        //        {
        //            ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(strs.ToArray()));
        //        }
        //        //else if (one.ImmediateBaseObject is IEnumerable<Type> types)
        //        else if (one.ImmediateBaseObject is IEnumerable ienum)
        //        {
        //            foreach (object o in ienum)
        //            {
        //                if (o is Type oType)
        //                    ResolvedTypes.Add(oType);
        //            }
        //        }
        //        else if (one.ImmediateBaseObject is string str)
        //        {
        //            ResolvedTypes.AddRange(base.ResolveTypeThroughPowerShell(str));
        //        }
        //        else
        //        {
        //            ResolvedTypes.Add(one.ImmediateBaseObject.GetType());
        //        }
        //    }
        //}

        #endregion

        #endregion
    }
}
