using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MG.Types
{
    public static class StringExtensions
    {
        public static bool StartsWith(this string input, char firstChar)
        {
            return !(input is null) && input.Length > 0 && firstChar == input[0];
        }
        public static bool EndsWith(this string input, char firstChar)
        {
            return !(input is null) && input.Length > 0 && firstChar == input[input.Length - 1];
        }
    }
}

