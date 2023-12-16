using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;

namespace MG.Types.Pools
{
    internal sealed class ObjectPool<T> where T : class
    {
        const int DEFAULT_CAP = 20;

        readonly ConcurrentBag<T> _bag;
        readonly IPoolPolicy<T> _policy;

        internal int MaxCapacity { get; }

        internal ObjectPool(IPoolPolicy<T> policy)
            : this(DEFAULT_CAP, policy)
        {
        }
        internal ObjectPool(int maxCapacity, IPoolPolicy<T> policy)
        {
            Guard.NotNull(policy, nameof(policy));

            _bag = new ConcurrentBag<T>();
            _policy = policy;
            this.MaxCapacity = maxCapacity > 0 ? maxCapacity : DEFAULT_CAP;
        }

        public T Get()
        {
            if (!_bag.TryTake(out T item))
            {
                item = _policy.Create();
            }

            return item!;
        }
        public void Return([MaybeNull] T item)
        {
            if (item is null || !_policy.Return(item))
            {
                return;
            }

            _bag.Add(item);
        }
    }
}

