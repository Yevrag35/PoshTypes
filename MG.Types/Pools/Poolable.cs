using System;

namespace MG.Types.Pools
{
    public interface IPoolPolicy<T> where T : notnull
    {
        T Create();
        bool Return(T obj);
    }
    public interface IPoolable<T> where T : notnull
    {
        IPoolable<T> AddConstructor(Func<T> ctor);
        IPoolable<T> AddReset(Func<T, bool> reset);
    }
    public interface IPoolable<T, TState> where T : notnull
    {
        IPoolable<T, TState> AddConstructor(Func<TState, T> ctor);
        IPoolable<T, TState> AddReset(Func<T, TState, bool> reset);
    }

    internal class Poolable<T> : IPoolable<T>, IPoolPolicy<T> where T : notnull
    {
        Func<T> _factoryImpl = null!;
        Func<T, bool> _resetImpl = null!;

        internal Poolable()
        {
        }

        public IPoolable<T> AddConstructor(Func<T> ctor)
        {
            Guard.NotNull(ctor, nameof(ctor));

            _factoryImpl = ctor;
            return this;
        }
        public IPoolable<T> AddReset(Func<T, bool> reset)
        {
            Guard.NotNull(reset, nameof(reset));

            _resetImpl = reset;
            return this;
        }

        public T Create()
        {
            if (_factoryImpl is null)
            {
                throw new InvalidOperationException("No constructor callback has been provided.");
            }

            return _factoryImpl.Invoke();
        }
        public bool Return(T obj)
        {
            if (_resetImpl is null)
            {
                throw new InvalidOperationException("No reset callback has been provided.");
            }

            return _resetImpl.Invoke(obj);
        }
    }
    internal sealed class Poolable<T, TState> : IPoolable<T, TState>, IPoolPolicy<T> where T : notnull
    {
        Func<TState, T> _factoryImpl = null!;
        Func<T, TState, bool> _resetImpl = null!;

        readonly TState _state;

        internal Poolable(TState state)
        {
            Guard.NotNull(state, nameof(state));
            _state = state;
        }

        public IPoolable<T, TState> AddConstructor(Func<TState, T> ctor)
        {
            Guard.NotNull(ctor, nameof(ctor));

            _factoryImpl = ctor;
            return this;
        }
        public IPoolable<T, TState> AddReset(Func<T, TState, bool> reset)
        {
            Guard.NotNull(reset, nameof(reset));

            _resetImpl = reset;
            return this;
        }

        public T Create()
        {
            if (_factoryImpl is null)
            {
                throw new InvalidOperationException("No constructor callback has been provided.");
            }

            return _factoryImpl.Invoke(_state);
        }
        public bool Return(T obj)
        {
            if (_resetImpl is null)
            {
                throw new InvalidOperationException("No reset callback has been provided.");
            }

            return _resetImpl.Invoke(obj, _state);
        }
    }
}

