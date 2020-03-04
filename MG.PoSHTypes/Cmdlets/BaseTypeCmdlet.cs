using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CodeDom;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types
{
    public abstract class BaseTypeCmdlet : PSCmdlet
    {
        //private const string SCRIPT = "[type]$type = [{0}]; return $type";
        private const string SCRIPT = "[{0}]";

        public static readonly Dictionary<string, Type> AliasTable = new Dictionary<string, Type>
        {
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "char", typeof(char) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "object", typeof(object) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "string", typeof(string) }
        };

        protected private virtual IEnumerable<Type> GetTypesFromArray(IEnumerable array)
        {
            var list = new List<Type>();
            foreach (object obj in array)
            {
                list.Add(obj.GetType());
            }
            return list;
        }

        #region PARAMETER VALIDATION
        //public bool HasParameterSpecified<T, U>(T cmdlet, Expression<Func<T, U>> cmdletParameterExpression) where T : PSCmdlet
        //{
        //    bool result = false;
        //    if (cmdletParameterExpression.Body is MemberExpression memEx)
        //        result = cmdlet.MyInvocation.BoundParameters.ContainsKey(memEx.Member.Name);

        //    else if (cmdletParameterExpression.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unMemEx)
        //        result = cmdlet.MyInvocation.BoundParameters.ContainsKey(unMemEx.Member.Name);

        //    return result;
        //}

        #endregion

        protected private virtual BindingFlags JoinFlags(params BindingFlags[] flags)
        {
            string[] strArr = new string[flags.Length];
            for (int i = 0; i < flags.Length; i++)
            {
                strArr[i] = flags[i].ToString();
            }
            string oneStr = string.Join(",", strArr);
            return (BindingFlags)Enum.Parse(typeof(BindingFlags), oneStr, true);
        }

        #region FILTERING
        public IEnumerable<T> FilterByStrings<T>(IEnumerable<T> filterThisCol, Expression<Func<T, string>> propertyExpressionOfCol, IEnumerable<string> withThis)
            where T : class
        {
            if (withThis != null && propertyExpressionOfCol.Body is MemberExpression)
            {
                Func<T, string> propertyFunc = propertyExpressionOfCol.Compile();

                return filterThisCol
                    .Where(x => withThis
                        .Any(s => s
                            .Equals(propertyFunc(x), StringComparison.CurrentCultureIgnoreCase)));
            }
            else
                return filterThisCol;
        }

        #endregion

        protected private void AddStringTypesToResolved(string[] names, ref List<Type> ResolvedTypes)
        {
            foreach (Type t in this.ResolveTypeThroughPowerShell(names))
            {
                ResolvedTypes.Add(t);
            }
        }

        protected private void ProcessInputObject(PSObject input, ref List<Type> ResolvedTypes)
        {
            if (input.ImmediateBaseObject is Type incomingType)
            {
                ResolvedTypes.Add(incomingType);
            }
            else
            {
                ResolvedTypes.Add(input.ImmediateBaseObject.GetType());
            }
        }

        protected private List<Type> ResolveType(IEnumerable<string> typeNames)
        {
            //var Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var types = new List<Type>();
            foreach (string name in typeNames)
            {
                if (AliasTable.ContainsKey(name))
                    types.Add(AliasTable[name]);

                else
                {
                    var tryType = Type.GetType(name, false, true);
                    if (tryType != null)
                    {
                        types.Add(tryType);
                    }
                }
            }
            return types;
        }
        protected private List<Type> ResolveTypeThroughPowerShell(params string[] typeNames)
        {
            //var types = new List<Type>(typeNames.Length);
            List<Type> types = null;
            string[] typeScript = new string[typeNames.Length];
            for (int i = 0; i < typeNames.Length; i++)
            {
                typeScript[i] = string.Format(SCRIPT, typeNames[i]);
            }
            if (typeScript.Length > 0)
            {
                types = new List<Type>(typeNames.Length);
                string oneScript = string.Join(Environment.NewLine, typeScript);
                using (var ps = System.Management.Automation.PowerShell.Create().AddScript(oneScript))
                {
                    Collection<Type> output = ps.Invoke<Type>();
                    types.AddRange(output);
                }
            }
            return types;
        }
    }
}
