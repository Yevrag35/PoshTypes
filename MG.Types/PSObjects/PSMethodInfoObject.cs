using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSObjects
{
    internal sealed class PSMethodInfoObject : PSReflectionObject<MethodInfo, PSMethodInfoObject>
    {
        static readonly string _typeName = typeof(PSMethodInfoObject).GetTypeName();
        readonly ParameterInfo[] _parameters;
        readonly string _toStr;

        public string AsString => _toStr;
        public override ReflectionType ReflectionType => ReflectionType.Method;
        protected override int MyNumberOfTypeNames => 1;
        internal PSMethodInfoObject(MethodInfo methodInfo, Type fromType)
            : base(methodInfo, fromType)
        {
            _parameters = methodInfo.GetParameters() ?? Array.Empty<ParameterInfo>();
            _toStr = this.GetDisplayString(methodInfo, _parameters);
        }

        protected override void AddTypeName(Span<string> addToNames)
        {
            addToNames[0] = _typeName;
        }

        protected override int ReflectionObjectCompareTo(MethodInfo thisObj, MethodInfo other, PSMethodInfoObject otherParent)
        {
            int code = StringComparer.InvariantCultureIgnoreCase.Compare(thisObj.Name, other.Name);
            if (code == 0)
            {
                code = _parameters.Length.CompareTo(otherParent._parameters);
            }

            return code;
        }

        protected override bool ReflectionObjectEquals(MethodInfo thisObj, MethodInfo other)
        {
            return thisObj == other;
        }

        const string FORMAT = "{0} {1}({2})";
        private string GetDisplayString(MethodInfo method, ParameterInfo[] parameters)
        {
            StringBuilder builder = new(150);
            builder.Append(method.ReturnType.GetPSTypeName())
                   .Append(' ')
                   .Append(method.Name)
                   .Append('(');

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];

                builder.Append(p.ParameterType.GetPSTypeName())
                       .Append(' ')
                       .Append(p.Name);

                if (i < parameters.Length - 1)
                {
                    builder.Append(',').Append(' ');
                }
            }

            return builder.Append(')').ToString();
        }
    }
}

