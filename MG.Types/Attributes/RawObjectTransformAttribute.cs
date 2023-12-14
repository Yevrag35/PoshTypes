using MG.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Attributes
{
    public sealed class RawObjectTransformAttribute : ArgumentTransformationAttribute
    {
        public override object? Transform(EngineIntrinsics engineIntrinsics, object? inputData)
        {
            object? target = inputData.GetBaseObject();
            return target;
        }
    }
}

