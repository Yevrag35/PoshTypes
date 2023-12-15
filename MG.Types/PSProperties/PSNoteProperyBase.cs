using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.PSProperties
{
    public abstract class PSNotePropertyBase : PSPropertyInfo
    {
        const string DEFAULT_PSTYPE = "object";
        const string DEFAULT_TYPE = "System.Object";

        public sealed override bool IsGettable => true;
        public sealed override PSMemberTypes MemberType => PSMemberTypes.NoteProperty;
        public override string TypeNameOfValue => this.Value?.GetType().GetTypeName() ?? DEFAULT_TYPE;

        protected virtual string GetPSTypeName()
        {
            return this.Value?.GetType().GetPSTypeName(removeBrackets: true) ?? DEFAULT_PSTYPE;
        }
        protected virtual string GetValueToString()
        {
            return this.Value?.ToString() ?? string.Empty;
        }

#if NET5_0_OR_GREATER
        const int SPACE_AND_EQUALS_LENGTH = 2;
        public override string ToString()
        {
            string psTypeName = this.GetPSTypeName();
            string valueAsStr = this.GetValueToString();

            int length = psTypeName.Length + this.Name.Length + valueAsStr.Length + SPACE_AND_EQUALS_LENGTH;

            return string.Create(length, (psTypeName, name: this.Name, valueAsStr), (chars, state) =>
            {
                state.psTypeName.AsSpan().CopyTo(chars);
                int position = state.psTypeName.Length;

                chars[position++] = ' ';

                state.name.AsSpan().CopyTo(chars.Slice(position));
                position += state.name.Length;

                chars[position++] = '=';

                state.valueAsStr.AsSpan().CopyTo(chars.Slice(position));
            });
        }
#else
        const string STR_FORMAT = "{0} {1}={2}";
        public override string ToString()
        {
            string psTypeName = this.GetPSTypeName();
            string valueAsStr = this.GetValueToString();
            return string.Format(STR_FORMAT, psTypeName, this.Name, valueAsStr);
        }
#endif
    }
}

