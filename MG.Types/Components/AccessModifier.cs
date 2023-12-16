using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Components
{
    [Flags]
    public enum AccessModifier
    {
        None = 0,
        Public = 1,
        Private = 2,
        Internal = 4,
        Protected = 8,
    }
}

