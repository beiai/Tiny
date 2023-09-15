using System;
using System.Collections.Generic;

namespace UniEngine.Base.ReferencePool
{
    public sealed class ConcurrentReferencePoolObject
    {
        private readonly Stack<IReference> _references;

        public ConcurrentReferencePoolObject(Type referenceType)
        {
            _references = new Stack<IReference>();
            ReferenceType = referenceType;
            UsingReferenceCount = 0;
            AcquireReferenceCount = 0;
            ReleaseReferenceCount = 0;
            AddReferenceCount = 0;
            RemoveReferenceCount = 0;
        }

        /// <summary>
        /// 引用池的类型
        /// </summary>
        public Type ReferenceType { get; }

        /// <summary>
        /// 尚未使用的对象的次数
        /// </summary>
        public int UnusedReferenceCount
        {
            get
            {
                lock (_references)
                {
                    return _references.Count;
                }
            }
        }

        public int UsingReferenceCount { get; private set; }

        /// <summary>
        /// Acquire获取对象的次数
        /// </summary>
        public int AcquireReferenceCount { get; private set; }

        /// <summary>
        /// Release对象的次数
        /// </summary>
        public int ReleaseReferenceCount { get; private set; }

        /// <summary>
        /// 添加进对象池的次数
        /// </summary>
        public int AddReferenceCount { get; private set; }

        /// <summary>
        /// 移除对象池的次数
        /// </summary>
        public int RemoveReferenceCount { get; private set; }

        public T Acquire<T>() where T : class, IReference, new()
        {
            if (ReferencePool.EnableStrictCheck && typeof(T) != ReferenceType)
            {
                throw new Exception("Type is invalid.");
            }

            UsingReferenceCount++;
            AcquireReferenceCount++;
            lock (_references)
            {
                if (_references.Count > 0)
                {
                    return (T)_references.Pop();
                }
            }

            AddReferenceCount++;
            return new T();
        }

        public void Release(IReference reference)
        {
            reference.Clear();
            lock (_references)
            {
                if (ReferencePool.EnableStrictCheck && _references.Contains(reference))
                {
                    throw new Exception("The reference has been released.");
                }

                _references.Push(reference);
            }

            ReleaseReferenceCount++;
            UsingReferenceCount--;
        }

        public void Add<T>(int count) where T : class, IReference, new()
        {
            if (ReferencePool.EnableStrictCheck && typeof(T) != ReferenceType)
            {
                throw new Exception("Type is invalid.");
            }

            lock (_references)
            {
                AddReferenceCount += count;
                while (count-- > 0)
                {
                    _references.Push(new T());
                }
            }
        }

        public void Add(int count)
        {
            lock (_references)
            {
                AddReferenceCount += count;
                while (count-- > 0)
                {
                    _references.Push((IReference)Activator.CreateInstance(ReferenceType));
                }
            }
        }

        public void Remove(int count)
        {
            lock (_references)
            {
                if (count > _references.Count)
                {
                    count = _references.Count;
                }

                RemoveReferenceCount += count;
                while (count-- > 0)
                {
                    _references.Pop();
                }
            }
        }

        public void RemoveAll()
        {
            lock (_references)
            {
                RemoveReferenceCount += _references.Count;
                _references.Clear();
            }
        }
    }
}