using MG.Types.Attributes;
using MG.Types.PSObjects;
using MG.Types.Statics;
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
    [Cmdlet(VerbsCommon.Get, "PSMethod", DefaultParameterSetName = PSConstants.FROM_PIPELINE)]
    [Alias("Get-Method")]
    public sealed class GetMethodCmdlet : TypeCmdletBase
    {
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

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PSConstants.FROM_PIPELINE)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = PSConstants.WITH_TYPE)]
        [SupportsWildcards]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace]
#else
        [ValidateNotNullOrEmpty]
#endif
        public string[] Name { get; set; } = null!;

        protected override void Process()
        {
            HashSet<Type> types = this.GetPooledSet();

            Type type = PSConstants.IsFromPipeline(this)
                ? GetTypeFromObject(this.InputObject)
                : this.Type;

            if (!types.Add(type))
            {
                return;
            }

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
    }
}

