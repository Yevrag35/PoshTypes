using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.Posh.Types
{
    public class PoshMethodParameter : BaseObject
    {
        public ParameterAttributes Attributes { get; private set; }
        public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
        public object DefaultValue { get; private set; }
        public bool HasDefaultValue { get; private set; }
        public bool IsIn { get; private set; }
        public bool IsLcid { get; private set; }
        public bool IsOptional { get; private set; }
        public bool IsOut { get; private set; }
        public bool IsRetval { get; private set; }
        public MemberInfo Member { get; private set; }
        public int MetadataToken { get; private set; }
        //public string Name { get; private set; }
        public Type ParameterType { get; private set; }
        public int Position { get; private set; }
        public object RawDefaultValue { get; private set; }

        private PoshMethodParameter(ParameterInfo pi)
        {
            base.SetProperties(pi);
            base._backingField = pi;
        }

        public static implicit operator PoshMethodParameter(ParameterInfo pi) => new PoshMethodParameter(pi);
        public static implicit operator ParameterInfo(PoshMethodParameter pmp) => (ParameterInfo)pmp._backingField;
    }
}
