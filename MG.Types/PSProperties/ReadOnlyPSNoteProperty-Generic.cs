using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MG.Types.PSProperties
{
    public class ReadOnlyPSNoteProperty<T> : ReadOnlyPSNoteProperty
    {
        private bool _hasValue;

#if NET6_0_OR_GREATER
        private T? _value;
        [MemberNotNullWhen(true, nameof(_value), nameof(ValueAsT))]
        public bool HasValue => _hasValue;
        public T? ValueAsT => _value;
#else
        [MaybeNull]
        private T _value;

        public bool HasValue => _hasValue;
        public T ValueAsT => _value;
#endif


        protected ReadOnlyPSNoteProperty(string propertyName)
            : base(propertyName)
        {
            _value = default;
        }
        public ReadOnlyPSNoteProperty(string propertyName, T value)
            : base(propertyName)
        {
            _value = value;
        }

        protected sealed override ReadOnlyPSNoteProperty Copy(object? clonedValue)
        {
            if (!(clonedValue is T clonedTVal))
            {
                throw new InvalidOperationException($"The cloned value is not of the generic type '{this.TypeNameOfValue}'.");
            }

            return this.Copy(clonedTVal, clonedTVal is null);
        }
        protected virtual ReadOnlyPSNoteProperty<T> Copy(T clonedValue, bool valueIsNull)
        {
            return new ReadOnlyPSNoteProperty<T>(this.Name, clonedValue);
        }

        protected override string GetPSTypeName()
        {
            return typeof(T).GetPSTypeName(removeBrackets: true);
        }
        protected sealed override object? GetValue()
        {
            return _value;
        }
        protected override string GetValueToString()
        {
            return _value?.ToString() ?? string.Empty;
        }

        protected sealed override void SetValue(object? value)
        {
            if (this.TryConvertValue(value, value is null, out T valueToSet))
            {
                this.SetValue(valueToSet, valueToSet is null);
            }
        }
        protected virtual void SetValue([NotNullIfNotNull(nameof(valueIsNull))] T value, bool valueIsNull)
        {
            _value = value;
        }
        protected virtual bool TryConvertValue(object? value, bool valueIsNull, [NotNullWhen(true)] out T valueToSet)
        {
            if (value is T tVal)
            {
                valueToSet = tVal;
                return true;
            }
            else
            {
                valueToSet = default!;
                return false;
            }
        }
    }
}

