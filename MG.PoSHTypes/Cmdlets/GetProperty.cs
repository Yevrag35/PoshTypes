﻿using MG.Posh.Extensions.Bound;
using MG.Posh.Extensions.Pipe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.PowerShell.Types.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Property", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = "ByTypeName")]
    [Alias("getprop", "gpt")]
    [OutputType(typeof(PoshProperty))]
    [CmdletBinding(PositionalBinding = false)]
    public class GetProperty : BaseTypeCmdlet
    {
        #region FIELDS/CONSTANTS
        private List<Type> list;
        private static readonly string NL = Environment.NewLine;
        private BindingFlags RealFlags;
        private const string LI_ELE = @"<ListItem>
    <PropertyName>{0}</PropertyName>
</ListItem>";
        private const string LI_ELE_COND = @"<ListItem>
    <PropertyName>{0}</PropertyName>
    <ItemSelectionCondition>
        <ScriptBlock>$_.{0} -ne $null</ScriptBlock>
    </ItemSelectionCondition>
</ListItem>";

        private bool _cSharp;
        private bool _enum;
        private bool _psForm;
        private bool _useItemSel;

        #endregion

        #region PARAMETERS
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByPipelineObject")]
        [Alias("io")]
        public object InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByTypeName")]
        [Alias("Type", "t")]
        public string TypeName { get; set; }

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "ByPipelineObject")]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByTypeName")]
        [Alias("Name", "n")]
        public string[] PropertyName { get; set; }

        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByPipelineObject")]
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = "ByTypeName")]
        [ValidateSet("All", "Get", "GetSet", "Set")]
        public string Accessors = "All";

        [Parameter(Mandatory = false)]
        public BindingFlags[] Flags = new BindingFlags[2] { BindingFlags.Public, BindingFlags.Instance };

        [Parameter(Mandatory = false)]
        public SwitchParameter CSharpFormat
        {
            get => _cSharp;
            set => _cSharp = value;
        }

        [Parameter(Mandatory = false, ParameterSetName = "ByPipelineObject")]
        [Alias("enum")]
        public SwitchParameter Enumerate
        {
            get => _enum;
            set => _enum = value;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter PSFormatFile
        {
            get => _psForm;
            set => _psForm = value;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter UseItemSelectionCondition
        {
            get => _useItemSel;
            set => _useItemSel = value;
        }

        #endregion

        #region CMDLET PROCESSING
        protected override void BeginProcessing()
        {
            list = new List<Type>();
            RealFlags = base.JoinFlags(Flags);
        }
        protected override void ProcessRecord()
        {
            if (this.ContainsParameter(x => x.InputObject))
            {
                if (InputObject is Type inputType)
                {
                    list.Add(inputType);
                }
                else if (InputObject is PSObject psObj)
                {
                    if (_enum && psObj.ImmediateBaseObject is IEnumerable ienum)
                    {
                        list.AddRange(base.GetTypesFromArray(ienum));
                    }
                    else if (psObj.ImmediateBaseObject is Type type)
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list.Add(psObj.ImmediateBaseObject.GetType());
                    }
                }
                else
                {
                    list.Add(InputObject.GetType());
                }
            }
            else
            {
                list.AddRange(base.ResolveTypeThroughPowerShell(TypeName));
            }
        }

        protected override void EndProcessing()
        {
            var props = new List<PoshProperty>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Type t = list[i];
                IEnumerable<PropertyInfo> typeProps = t.GetProperties(RealFlags);
                if (this.ContainsParameter(x => x.PropertyName))
                {
                    typeProps = base.FilterByStrings(typeProps, x => x.Name, this.PropertyName);
                }
                foreach (PropertyInfo pi in typeProps)
                {
                    props.Add(pi);
                }
            }
            props.Sort();

            Func<PoshProperty, bool> function = this.GetCondition();
            IEnumerable<PoshProperty> finalProps = props.Where(function);

            if (_cSharp)
            {
                WriteObject(this.ToCSharpFormat(finalProps), true);
            }
            else if (_psForm)
            {
                WriteObject(this.ToPSFileFormat(finalProps));
            }
            else
            {
                WriteObject(finalProps, true);
            }
        }

        #endregion

        #region BACKEND METHODS
        private Func<PoshProperty, bool> GetCondition()
        {
            switch (Accessors)
            {
                case "GetSet":
                    return x => x.CanRead && x.CanWrite;

                case "Set":
                    return x => !x.CanRead && x.CanWrite;

                default:
                    return x => x.CanRead;
            }
        }

        private List<PSObject> ToCSharpFormat(IEnumerable<PoshProperty> props)
        {
            var list = new List<PSObject>();
            foreach (PoshProperty p in props)
            {
                var psObj = new PSObject();
                var get = new PSNoteProperty("get;", p.CanRead);
                var set = new PSNoteProperty("set;", p.CanWrite);
                var name = new PSNoteProperty("Name", p.Name);
                var type = new PSNoteProperty("Type", BaseObject.GetTypeAlias(true, p.PropertyType).FirstOrDefault());

                psObj.Properties.Add(get);
                psObj.Properties.Add(set);
                psObj.Properties.Add(name);
                psObj.Properties.Add(type);
                list.Add(psObj);
            }
            return list;
        }

        private string ToPSFileFormat(IEnumerable<PoshProperty> props)
        {
            var list = new List<string>();
            //string format = this.MyInvocation.BoundParameters.ContainsKey("UseItemSelectionCondition")
            string format = _useItemSel
                ? LI_ELE_COND
                : LI_ELE;

            foreach (PoshProperty p in props)
            {
                string liStr = string.Format(format, p.Name);
                list.Add(liStr);
            }
            return string.Join(NL, list);
        }

        #endregion
    }
}
