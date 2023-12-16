using MG.Types.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSProperties
{
    public abstract class ReadOnlyPSNoteProperty : PSNotePropertyBase
    {
        public sealed override bool IsSettable => false;

        /// <summary>
        ///     Gets the value of the <see cref="ReadOnlyPSNoteProperty"/>.
        /// </summary>
        /// <remarks>
        ///     Even though this property has a setter, a <see cref="SetValueException"/> will be thrown if 
        ///     trying to overwrite the initial value set from the constructor.
        /// </remarks>
        /// <exception cref="SetValueException"/>
        public sealed override object? Value
        {
            get => this.GetValue();
            set
            {
                var ex = new ReadOnlyPropertyException(this.Name);
                throw new SetValueException(ex.Message, ex);
            }
        }

        protected ReadOnlyPSNoteProperty(string propertyName)
        {
            Guard.NotNullOrEmpty(propertyName, nameof(propertyName));
            this.SetMemberName(propertyName);
        }
        protected ReadOnlyPSNoteProperty(string propertyName, object? value)
            : this(propertyName)
        {
            this.SetValue(value);
        }

        protected abstract ReadOnlyPSNoteProperty Copy(object? clonedValue);
        public sealed override PSMemberInfo Copy()
        {
            object? value = this.Value;
            if (value is ICloneable clonable)
            {
                value = clonable.Clone();
            }

            return this.Copy(value);
        }

        protected abstract object? GetValue();
        protected override string GetValueToString()
        {
            return this.GetValue()?.ToString() ?? string.Empty;
        }
        protected abstract void SetValue(object? value);
    }
}

