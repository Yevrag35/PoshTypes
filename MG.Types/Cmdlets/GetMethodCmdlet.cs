using MG.Types.Attributes;
using MG.Types.PSObjects;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSMethod", DefaultParameterSetName = FROM_PIPE)]
    [Alias("Get-Method")]
    public sealed class GetMethodCmdlet : PSCmdlet
    {
        const string FROM_PIPE = "FromPipeline";
        const string WITH_TYPE = "WithType";
        HashSet<Type> _types = null!;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = FROM_PIPE)]
        [AllowEmptyCollection]
        [AllowEmptyString]
        [ValidateNotNull]
        [RawObjectTransform]
        public object InputObject { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = WITH_TYPE)]
        [ArgumentToTypeTransform]
        [ValidateNotNull]
        public Type Type { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_PIPE)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = WITH_TYPE)]
        [ValidateNotNullOrWhiteSpace]
        [SupportsWildcards]
        public string[] Name { get; set; } = null!;

        protected override void BeginProcessing()
        {
            _types = new HashSet<Type>();
        }
        protected override void ProcessRecord()
        {
            Type type = this.IsFromPipe() ? GetTypeFromObject(this.InputObject) : this.Type;
            foreach (MethodInfo mi in GetMethods(type, BindingFlags.Public | BindingFlags.Instance, this.Name))
            {
                this.WriteObject(new PSMethodInfoObject(mi, type));
            }
        }

        private static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags flags, string[] names)
        {
            WildcardPattern[] patterns = ArrayPool<WildcardPattern>.Shared.Rent(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                patterns[i] = new WildcardPattern(names[i], WildcardOptions.IgnoreCase);
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo mi in methods)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (patterns[i].IsMatch(mi.Name))
                    {
                        yield return mi;
                    }
                }
            }

            ArrayPool<WildcardPattern>.Shared.Return(patterns);
        }

        private static Type GetTypeFromObject(object inputObject)
        {
            switch (inputObject)
            {
                case PSClassObject psc:
                    return psc.ReflectionObject;

                case PSInterfaceObject psi:
                    return psi.ReflectionObject;

                case PSMethodInfoObject psmi:
                    return GetTypeFromMember(psmi);

                case PSPropertyInfoObject pspi:
                    return GetTypeFromMember(pspi);

                case Type type:
                    return type;

                default:
                    return inputObject.GetType();
            }
        }

        private static Type GetTypeFromMember<T, TSelf>(PSReflectionObject<T, TSelf> refObj) where T : MemberInfo where TSelf : PSReflectionObject<T, TSelf>
        {
            return refObj.ParentType ?? refObj.ReflectionObject.DeclaringType ?? refObj.ReflectionObject.ReflectedType ?? typeof(object);
        }

        private bool IsFromPipe()
        {
            return FROM_PIPE == this.ParameterSetName;
        }
    }
}

