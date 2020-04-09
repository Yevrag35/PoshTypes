using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.Posh.Types
{
    public class PoshMethod : BaseObject
    {
        private const string PARAMETER_FORMAT = "{0} {1}";
        private const string DEF_FORMAT = "{0} {1}({2})";
        private bool? _hasParams = null;

        public MethodAttributes Attributes { get; private set; }
        public CallingConventions CallingConvention { get; private set; }
        public bool ContainsGenericParameters { get; private set; }
        public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
        public Type DeclaringType { get; private set; }
        public string Definition { get; }
        public bool HasParameters => _hasParams.HasValue && _hasParams.Value;
        public bool IsAbstract { get; private set; }
        public bool IsAssembly { get; private set; }
        public bool IsConstructor { get; private set; }
        public bool IsFamily { get; private set; }
        public bool IsFamilyAndAssembly { get; private set; }
        public bool IsFamilyOrAssembly { get; private set; }
        public bool IsFinal { get; private set; }
        public bool IsGenericMethod { get; private set; }
        public bool IsGenericMethodDefinition { get; private set; }
        public bool IsHideBySig { get; private set; }
        public bool IsPrivate { get; private set; }
        public bool IsPublic { get; private set; }
        public bool IsSecurityCritical { get; private set; }
        public bool IsSecuritySafeCritical { get; private set; }
        public bool IsSecurityTransparent { get; private set; }
        public bool IsSpecialName { get; private set; }
        public bool IsStatic { get; private set; }
        public bool IsVirtual { get; private set; }
        public MemberTypes MemberType { get; private set; }
        public int MetadataToken { get; private set; }
        public RuntimeMethodHandle MethodHandle { get; private set; }
        public MethodImplAttributes MethodImplementationFlags { get; private set; }
        public Module Module { get; private set; }
        //public string Name { get; private set; }
        public int NumberOfParameters { get; }
        public Type ReflectedType { get; private set; }
        public ParameterInfo ReturnParameter { get; private set; }
        public Type ReturnType { get; private set; }
        public ICustomAttributeProvider ReturnTypeCustomAttributes { get; private set; }

        private PoshMethod(MethodInfo mi)
        {
            base.SetProperties(mi);
            base._backingField = mi;
            var prms = this.GetParameters();
            _hasParams = prms != null && prms.Length > 0;

            this.NumberOfParameters = _hasParams.Value
                ? prms.Length
                : 0;

            this.Definition = this.FormatDefinition(prms);
        }

        public PoshMethodParameter[] GetParameters()
        {
            if (_hasParams.HasValue && !_hasParams.Value)
                return null;

            ParameterInfo[] prms = ((MethodInfo)base._backingField).GetParameters();
            var newArr = new PoshMethodParameter[prms.Length];
            for (int i =  0; i < prms.Length; i++)
            {
                newArr[i] = prms[i];
            }
            return newArr;
        }

        private string FormatDefinition(params PoshMethodParameter[] parameters)
        {
            string str = string.Empty;
            if (parameters != null && parameters.Length > 0)
            {
                var strs = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    PoshMethodParameter p = parameters[i];
                    strs[i] = string.Format(PARAMETER_FORMAT, GetTypeAlias(true, p.ParameterType).FirstOrDefault(), p.Name);
                }
                str = string.Join(", ", strs);
            }
            string formatted = string.Format(DEF_FORMAT, GetTypeAlias(true, this.ReturnType).FirstOrDefault(), this.Name, str);
            return formatted;
        }

        public static implicit operator PoshMethod(MethodInfo mi) => new PoshMethod(mi);
        public static implicit operator MethodInfo(PoshMethod pm) => (MethodInfo)pm._backingField;
    }

    public class PoshMethodSorter : IComparer<PoshMethod>
    {
        public int Compare(PoshMethod x, PoshMethod y)
        {
            int retNum = x.Name.CompareTo(y.Name);
            if (retNum == 0)
            {
                int xLength = x.NumberOfParameters;
                int yLength = y.NumberOfParameters;
                retNum = xLength > yLength
                    ? 1
                    : xLength < yLength
                        ? -1
                        : 0;
                
                if (retNum == 0)
                {
                    Type xType = x.ReturnType;
                    Type yType = y.ReturnType;
                    retNum = !string.IsNullOrEmpty(xType.FullName) && !string.IsNullOrEmpty(yType.FullName)
                        ? xType.FullName.CompareTo(yType.FullName)
                        : string.IsNullOrEmpty(xType.FullName) && !string.IsNullOrEmpty(yType.FullName)
                            ? 1
                            : !string.IsNullOrEmpty(xType.FullName) && string.IsNullOrEmpty(yType.FullName)
                                ? -1
                                : 0;
                }
            }
            return retNum;
        }
    }
}
