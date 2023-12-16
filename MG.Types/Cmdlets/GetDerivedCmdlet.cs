using MG.Types.Attributes;
using MG.Types.PSObjects;
using MG.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSDerivedType", DefaultParameterSetName = PSConstants.FROM_PIPELINE)]
    [Alias("Get-DerivedType")]
    public sealed class GetDerivedCmdlet : TypeCmdletBase
    {
        static readonly Lazy<HashSet<Type>> _allExportedTypes = 
            new Lazy<HashSet<Type>>(GetAllExportedTypes(new HashSet<Type>(2000)));

        private static HashSet<Type> GetAllExportedTypes(HashSet<Type> set)
        {
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly ass in asses)
            {
                if (ass.IsDynamic)
                {
                    continue;
                }

                IEnumerable<Type> typesToAdd = ass.GetExportedTypes()
                    .Where(type => !type.IsAbstract && !type.IsSealed);

                set.UnionWith(typesToAdd);
            }

            return set;
        }
        private static HashSet<Type> ReloadTypes()
        {
            if (!_allExportedTypes.IsValueCreated)
            {
                return _allExportedTypes.Value;
            }

            return GetAllExportedTypes(_allExportedTypes.Value);
            
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = PSConstants.FROM_PIPELINE)]
        [AllowEmptyCollection]
        [AllowEmptyString]
        [ValidateNotNull]
        [RawObjectTransform]
        public object InputObject { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PSConstants.WITH_TYPE)]
        [ArgumentToTypeTransform]
        [ValidateNotNull]
        public Type Type { get; set; } = null!;

        [Parameter]
        public SwitchParameter ReloadAssemblies { get; set; }

        protected override void BeginProcessing()
        {
            if (this.ReloadAssemblies)
            {
                ReloadTypes();
            }
        }
        protected override void ProcessRecord()
        {
            Type baseType = PSConstants.IsFromPipeline(this)
               ? GetTypeFromObject(this.InputObject)
               : this.Type;

            if (baseType.IsSealed)
            {
                return;
            }

            foreach (Type derivedType in GetDerivedTypes(baseType))
            {
                if (derivedType.IsInterface)
                {
                    this.WriteObject(new PSInterfaceObject(derivedType, baseType));
                }
                else
                {
                    this.WriteObject(new PSClassObject(derivedType, baseType));
                }
            }
        }

        private static IEnumerable<Type> GetDerivedTypes(Type baseType)
        {
            IEnumerable<Type> exportedTypes = _allExportedTypes.Value;

            foreach (Type type in exportedTypes)
            {
                if (baseType.IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }
    }
}

