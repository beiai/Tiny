using System;
using System.Collections.Generic;

namespace UniEngine.Base.EasyPool
{
    public class EasyPool<T> : IDisposable, IEasyPool<T> where T : class
    {
        private readonly Stack<T> _stack;

        private readonly Func<T> _createFunc;

        private readonly Action<T> _actionOnGet;

        private readonly Action<T> _actionOnRelease;

        private readonly Action<T> _actionOnDestroy;

        private readonly int _maxSize;

        private bool _collectionCheck;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => _stack.Count;

        public EasyPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if (createFunc == null)
            {
                throw new Exception("createFunc Can Not Be Null.");
            }

            if (maxSize <= 0)
            {
                throw new Exception("Max Size must be greater than 0");
            }

            _stack = new Stack<T>(defaultCapacity);
            _createFunc = createFunc;
            _maxSize = maxSize;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _collectionCheck = collectionCheck;
        }

        public T Get()
        {
            T val;
            if (_stack.Count == 0)
            {
                val = _createFunc();
                CountAll++;
            }
            else
            {
                val = _stack.Pop();
            }

            _actionOnGet?.Invoke(val);
            return val;
        }

        public void Release(T element)
        {
            if (_collectionCheck && _stack.Count > 0 && _stack.Contains(element))
            {
                throw new InvalidOperationException(
                    "Trying to release an object that has already been released to the pool.");
            }

            _actionOnRelease?.Invoke(element);
            if (CountInactive < _maxSize)
            {
                _stack.Push(element);
            }
            else
            {
                _actionOnDestroy?.Invoke(element);
            }
        }

        public void Clear()
        {
            if (_actionOnDestroy != null)
            {
                foreach (var item in _stack)
                {
                    _actionOnDestroy(item);
                }
            }

            _stack.Clear();
            CountAll = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}