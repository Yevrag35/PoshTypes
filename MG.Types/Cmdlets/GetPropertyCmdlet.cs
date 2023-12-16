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
    [Cmdlet(VerbsCommon.Get, "PSProperty", DefaultParameterSetName = PSConstants.FROM_PIPELINE)]
    [Alias("Get-Property")]
    public sealed class GetPropertyCmdlet : TypeCmdletBase
    {
        BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;
        HashSet<Type> _types = null!;

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
        [ValidateNotNullOrWhiteSpace]
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

        protected override void BeginProcessing()
        {
            _types = new HashSet<Type>();
        }
        protected override void ProcessRecord()
        {
            Type type = PSConstants.IsFromPipeline(this)
                ? GetTypeFromObject(this.InputObject)
                : this.Type;

            if (!_types.Add(type))
            {
                return;
            }

            foreach (PropertyInfo pi in GetProperties(type, _flags, this.Name))
            {
                this.WriteObject(new PSPropertyInfoObject(pi, type));
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags, string[] names)
        {
            WildcardPattern[] patterns = names.Length > 0
                ? ArrayPool<WildcardPattern>.Shared.Rent(names.Length)
                : Array.Empty<WildcardPattern>();

            for (int i = 0; i < names.Length; i++)
            {
                patterns[i] = new WildcardPattern(names[i], WildcardOptions.IgnoreCase);
            }

            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (PropertyInfo pi in properties)
            {
                if (names.Length <= 0 || MatchesName(names.Length, patterns, pi))
                {
                    yield return pi;
                }
            }

            ArrayPool<WildcardPattern>.Shared.Return(patterns);
        }

        private static bool MatchesName(int namesLength, WildcardPattern[] patterns, PropertyInfo propertyInfo)
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

