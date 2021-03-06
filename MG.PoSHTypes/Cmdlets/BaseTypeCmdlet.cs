﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CodeDom;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types
{
    public abstract class BaseTypeCmdlet : PSCmdlet
    {
        private const string SCRIPT = "[type]$type = [{0}]; return $type";

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

        protected private List<Type> ResolveType(IEnumerable<string> typeNames)
        {
            var Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
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
            var types = new List<Type>(typeNames.Length);
            for (int i = 0; i < typeNames.Length; i++)
            {
                string name = typeNames[i];
                var psScript = string.Format(SCRIPT, name);
                using (var ps = System.Management.Automation.PowerShell.Create().AddScript(psScript))
                {
                    Collection<Type> output = ps.Invoke<Type>();
                    types.AddRange(output);
                }
            }
            return types;
        }
    }
}
