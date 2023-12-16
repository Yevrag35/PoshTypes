using MG.Types.Attributes;
using MG.Types.PSObjects;
using MG.Types.Statics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PSField", DefaultParameterSetName = PSConstants.FROM_PIPELINE)]
    [Alias("Get-Field")]
    public sealed class GetFieldCmdlet : TypeCmdletBase
    {
        BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;

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

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = PSConstants.FROM_PIPELINE)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = PSConstants.WITH_TYPE)]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace]
#else
        [ValidateNotNullOrEmpty]
#endif
        [SupportsWildcards]
        public string[] Name { get; set; } = Array.Empty<string>();

        [Parameter]
        public SwitchParameter Static
        {
            get => _flags.HasFlag(BindingFlags.Static);
            set
            {
                if (value)
                {
                    _flags &= ~BindingFlags.Instance;
                    _flags |= BindingFlags.Static;
                }
                else
                {
                    _flags &= ~BindingFlags.Static;
                    _flags |= BindingFlags.Instance;
                }
            }
        }

        [Parameter]
        public SwitchParameter NonPublic
        {
            get => _flags.HasFlag(BindingFlags.NonPublic);
            set
            {
                if (value)
                {
                    _flags |= BindingFlags.NonPublic;
                }
                else
                {
                    _flags &= ~BindingFlags.NonPublic;
                }
            }
        }

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

            foreach (FieldInfo fi in GetFields(type, _flags, this.Name))
            {
                this.WriteObject(new PSFieldInfoObject(fi, type));
            }
        }

        private static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flags, string[] names)
        {
            WildcardPattern[] patterns = names.Length > 0
                ? ArrayPool<WildcardPattern>.Shared.Rent(names.Length)
                : Array.Empty<WildcardPattern>();

            for (int i = 0; i < names.Length; i++)
            {
                patterns[i] = new WildcardPattern(names[i], WildcardOptions.IgnoreCase);
            }

            FieldInfo[] properties = type.GetFields(flags);
            foreach (FieldInfo fi in properties)
            {
                if (names.Length <= 0 || MatchesName(names.Length, patterns, fi))
                {
                    yield return fi;
                }
            }

            ArrayPool<WildcardPattern>.Shared.Return(patterns);
        }

        private static bool MatchesName(int namesLength, WildcardPattern[] patterns, FieldInfo propertyInfo)
        {
            bool flag = false;
            for (int i = 0; i < namesLength; i++)
            {
                if (patterns[i].IsMatch(propertyInfo.Name))
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }
    }
}

