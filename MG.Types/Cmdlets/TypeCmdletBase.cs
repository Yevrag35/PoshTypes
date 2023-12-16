using MG.Types.PSObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MG.Types.Cmdlets
{
    public abstract class TypeCmdletBase : PSCmdlet
    {
        protected static Type GetTypeFromObject(object inputObject)
        {
            switch (inputObject)
            {
                case PSClassObject psc:
                    return psc.ReflectionObject;

                case PSInterfaceObject psi:
                    return psi.ReflectionObject;

                case PSMethodInfoObject psmi:
                    return GetTypeFromMember(psmi);

                case PSPropertyInfoObject pspi:
                    return GetTypeFromMember(pspi);

                case Type type:
                    return type;

                default:
                    return inputObject.GetType();
            }
        }

        private static Type GetTypeFromMember<T, TSelf>(PSReflectionObject<T, TSelf> refObj) where T : MemberInfo where TSelf : PSReflectionObject<T, TSelf>
        {
            return refObj.ParentType ?? refObj.ReflectionObject.DeclaringType ?? refObj.ReflectionObject.ReflectedType ?? typeof(object);
        }
    }
}

