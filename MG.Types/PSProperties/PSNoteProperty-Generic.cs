using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSProperties
{
    public class PSNoteProperty<T> : PSNotePropertyBase
    {
        private T? _value;

        public sealed override bool IsSettable => true;

        public virtual T? ValueAsT
        {
            get => _value;
            set => _value = value;
        }
        public sealed override object? Value
        {
            get => this.ValueAsT;
            set => this.SetBackingValue(value);
        }

        public PSNoteProperty(string propertyName)
        {
            this.SetMemberName(propertyName);
        }
        public PSNoteProperty(string propertyName, T? value)
            : this(propertyName)
        {
            _value = value;
        }

        public sealed override PSMemberInfo Copy()
        {
            T? val = _value;
            if (_value is ICloneable clonable)
            {
                val = (T?)clonable.Clone();
            }

            return this.Copy(val);
        }
        protected virtual PSNoteProperty<T> Copy(T? clonedValue)
        {
            return new PSNoteProperty<T>(this.Name, clonedValue);
        }
        protected override string GetValueToString()
        {
            return this.ValueAsT?.ToString() ?? string.Empty;
        }
        private void SetBackingValue(object? value)
        {
            if (this.TryConvertValue(value, value is null, out T? tVal))
            {
                this.ValueAsT = tVal;
            }
        }
        protected virtual bool TryConvertValue(object? value, bool valueIsNull, [NotNullWhen(true)] out T? valueToSet)
        {
            if (value is T tVal)
            {
                valueToSet = tVal;
                return true;
            }
            else
            {
                valueToSet = default;
                return false;
            }
        }
    }
}

