using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MG.PowerShell.Types
{
    public abstract class BaseObject
    {
        private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
        protected private object _backingField;

        public string Name { get; protected private set; }

        public static IEnumerable<string> GetTypeAlias(bool forPowerShell, params Type[] types)
        {
            var strs = new List<string>(types.Length);
            using (var cdp = CodeDomProvider.CreateProvider("CSharp"))
            {
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    string result = cdp.GetTypeOutput(new CodeTypeReference(type));

                    string realString = !string.IsNullOrEmpty(result) && forPowerShell
                        ? result.Replace("<", "[").Replace(">", "]")
                        : result;
                    strs.Add(realString);
                    if (type.ContainsGenericParameters && !type.IsGenericParameter)
                    {
                        strs.AddRange(GetTypeAlias(forPowerShell, type.GetGenericArguments()));
                    }
                }

                if (strs.Count == types.Length)
                {
                    return strs;
                }
                else if (strs.Count > 1 && types.Length == 1)
                {
                    string fullFormat = "{0}[{1}]";
                    string first = strs[0];
                    Match regEx = Regex.Match(first, @"^(.{1,})\[");
                    if (!regEx.Success)
                        return null;

                    string firstPart = regEx.Groups[1].Value;
                    string formattedRest = string.Join(", ", strs.Skip(1));
                    strs.Clear();
                    strs.Add(string.Format(fullFormat, firstPart, formattedRest));
                    return strs;
                }
                else
                    return null;
            }
        }

        protected private virtual void SetProperties<T>(T obj)
        {
            Type tt = typeof(T);
            Type thisType = this.GetType();
            IEnumerable<PropertyInfo> origProps = tt.GetProperties(FLAGS).Where(x => x.CanRead);

            PropertyInfo[] thisProps = thisType.GetProperties(FLAGS);

            for (int i = 0; i < thisProps.Length; i++)
            {
                var prop = thisProps[i];
                foreach (var thatProp in origProps)
                {
                    if (thatProp.Name.Equals(prop.Name))
                    {
                        object oVal = thatProp.GetValue(obj);
                        prop.SetValue(this, oVal);
                        break;
                    }
                }
            }
        }
    }
}
