using System;
using System.Management.Automation;

namespace MG.Types.Statics
{
    internal static class PSConstants
    {
        internal const string FROM_PIPELINE = "FromPipeline";
        internal const string WITH_TYPE = "WithType";

        internal const string PS_TYPE = "MG.Types.PSTypeBase";

        internal static bool IsFromPipeline(PSCmdlet cmdlet)
        {
            return FROM_PIPELINE == cmdlet.ParameterSetName;
        }
    }
}

