using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MG.Types.Pools
{
    internal static class Pool<T> where T : class
    {
        static ObjectPool<T> _pool = null!;

        internal static ObjectPool<T> Get(Action<IPoolable<T>> configurePool)
        {
            if (_pool is null)
            {
                var policy = new Poolable<T>();
                configurePool(policy);

                _pool = new ObjectPool<T>(policy);
            }
            
            return _pool;
        }

        internal static ObjectPool<T> Get<TState>(TState state, Action<IPoolable<T, TState>> configurePool)
        {
            if (_pool is null)
            {
                var policy = new Poolable<T, TState>(state);
                configurePool(policy);

                _pool = new ObjectPool<T>(policy);
            }

            return _pool;
        }
    }
}

