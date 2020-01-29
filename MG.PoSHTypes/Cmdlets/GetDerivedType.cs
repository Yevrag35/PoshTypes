using MG.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DerivedType", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "FromTypeName")]
    [Alias("gdt")]
    [OutputType(typeof(Type))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetDerivedType : PSCmdlet, IDynamicParameters
    {
        #region FIELDS/CONSTANTS
        private DynamicLibrary _dynLib;
        private const string PNAME = "Assembly";
        private static readonly Type PTYPE = typeof(string[]);
        private List<Assembly> asses;

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObjectFromPipeline", DontShow = true)]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromTypeName")]
        public string BaseType { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        [ValidateSet("AppDomain", "File")]
        public string Scope = "AppDomain";

        [Parameter(Mandatory = false)]
        public SwitchParameter Recurse { get; set; }

        #endregion

        #region DYNAMIC
        public object GetDynamicParameters()
        {
            if (_dynLib == null)
            {
                _dynLib = new DynamicLibrary();
                DynamicParameter<Assembly> dp = null;
                RuntimeDefinedParameter rtParam = null;
                Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (!Scope.Equals("File"))
                {
                    dp = new DynamicParameter<Assembly>(PNAME, Assemblies, x => x.FullName, "FullName")
                    {
                        Mandatory = false
                    };
                    rtParam = dp.AsRuntimeParameter();
                }
                else
                {
                    var pAtt = new ParameterAttribute
                    {
                        Mandatory = true
                    };
                    rtParam = new RuntimeDefinedParameter(PNAME, PTYPE, new Collection<Attribute>
                    {
                        pAtt
                    });
                }
                _dynLib.Add(PNAME, rtParam);
            }
            return _dynLib;
        }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            asses = new List<Assembly>();
            if (this.MyInvocation.BoundParameters.ContainsKey(PNAME))
            {
                var assNames = new List<string>();
                if (this.MyInvocation.BoundParameters[PNAME] is string oneAss)
                {
                    assNames.Add(oneAss);
                }
                else if (this.MyInvocation.BoundParameters[PNAME] is string[] moreThanAss)
                {
                    assNames.AddRange(moreThanAss);
                }
                
                if (assNames.Count > 0 && Scope.Equals("AppDomain"))
                {
                    for (int i = 0; i < assNames.Count; i++)
                    {
                        asses.Add(Assembly.Load(assNames[i]));
                    }
                }
            }
            else
            {
                asses.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));
            }
        }

        protected override void ProcessRecord()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("InputObject"))
            {
                BaseType = InputObject is Type type 
                    ? type.FullName 
                    : InputObject.GetType().FullName;
            }

            List<Type> derivedTypes = DerivedType.GetDerivedTypes(BaseType, asses, Recurse.ToBool());
            WriteObject(derivedTypes, true);
        }

        #endregion
    }
}
