using MG.Types.Pools;
using MG.Types.PSObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types.Cmdlets
{
    public abstract class TypeCmdletBase : PSCmdlet
    {
        HashSet<Type>? _set;
        ObjectPool<HashSet<Type>> _pool = null!;

        protected sealed override void BeginProcessing()
        {
            try
            {
                this.Begin();
            }
            catch
            {
                this.Cleanup();
                throw;
            }
        }
        protected sealed override void ProcessRecord()
        {
            try
            {
                this.Process();
            }
            catch
            {
                this.Cleanup();
                throw;
            }
        }
        protected sealed override void EndProcessing()
        {
            try
            {
                this.End();
            }
            finally
            {
                this.Cleanup();
            }
        }
        protected sealed override void StopProcessing()
        {
            try
            {
                this.OnStopping();
            }
            finally
            {
                this.Cleanup();
            }
        }
        protected virtual void Begin()
        {
            return;
        }
        protected virtual void Process()
        {
            return;
        }
        protected virtual void End()
        {
            return;
        }
        protected virtual void OnStopping()
        {
            return;
        }

        protected HashSet<Type> GetPooledSet()
        {
            return _set ??= this.ConstructSet();
        }

        private void Cleanup()
        {
            if (!(_pool is null || _set is null))
            {
                _pool.Return(_set);
            }
        }
        private static void ConfigureHashSetPool(IPoolable<HashSet<Type>> policy)
        {
            policy.AddConstructor(() =>
            {
#if NET6_0_OR_GREATER
                return new HashSet<Type>(50);
#else
                return new HashSet<Type>();
#endif
            })
                .AddReset(set =>
                {
                    set.Clear();
#if NET6_0_OR_GREATER
                    int capacity = set.EnsureCapacity(50);
                    if (capacity > 10000)
                    {
                        set.TrimExcess();
                        set.EnsureCapacity(50);
                    }
#endif
                    return true;
                });
        }
        private HashSet<Type> ConstructSet()
        {
            _pool = Pool<HashSet<Type>>.Get(ConfigureHashSetPool);
            _set = _pool.Get();

            return _set;
        }
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

