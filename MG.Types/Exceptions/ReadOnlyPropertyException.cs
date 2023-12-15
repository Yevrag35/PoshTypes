using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Exceptions
{
    [Serializable]
    public sealed class ReadOnlyPropertyException : Exception, ISerializable
    {
        const string MSG = "Unable to set the property value because '{0}' is marked as read-only.";

        public string? PropertyName { get; }

        public ReadOnlyPropertyException(string propertyName)
            : base(string.Format(MSG, propertyName))
        {
            this.PropertyName = propertyName;
        }

        [Obsolete]
        private ReadOnlyPropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ArgumentNullException.ThrowIfNull(info);
            this.PropertyName = info.GetString(nameof(this.PropertyName));
        }

        [Obsolete]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);

            info.AddValue(nameof(this.PropertyName), this.PropertyName, typeof(string));

            base.GetObjectData(info, context);
        }
    }
}

