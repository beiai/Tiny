using System;
using System.Collections.Generic;
using UniEngine.Base.ReferencePool;

namespace UniEngine.Module.ObjectPool
{
    /// <summary>
    /// 对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public sealed class ObjectPool<T> : ObjectPoolBase where T : ObjectBase, new()
    {
        private readonly Stack<T> _stack = new();
        private readonly HashSet<T> _hashSet = new();
        private int _capacity;
        private int _count;
        private bool _enableCheck;

        /// <summary>
        /// 初始化对象池的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="enableCheck">是否开启检查</param>
        public ObjectPool(string name, int capacity, int priority, bool enableCheck) : base(name)
        {
            _enableCheck = enableCheck;
            Capacity = capacity;
            Priority = priority;
        }

        /// <summary>
        /// 获取对象池对象类型。
        /// </summary>
        public override Type ObjectType => typeof(T);

        /// <summary>
        /// 获取对象池中所有对象的数量。
        /// </summary>
        public override int Count => _count;

        /// <summary>
        /// 获取对象池中能已使用的对象的数量。
        /// </summary>
        public override int SpawnCount => Count - UnSpawnCount;

        /// <summary>
        /// 获取对象池中能未使用的对象的数量。
        /// </summary>
        public override int UnSpawnCount => _stack.Count;

        /// <summary>
        /// 获取或设置对象池的容量。
        /// </summary>
        public override int Capacity
        {
            get => _capacity;
            set
            {
                if (value < 0)
                {
                    throw new Exception("Capacity is invalid.");
                }

                _capacity = value;
                Release(_capacity - UnSpawnCount);
            }
        }

        /// <summary>
        /// 获取或设置对象池的优先级。
        /// </summary>
        public override int Priority { get; set; }

        /// <summary>
        /// 是否开启检查对象池。
        /// </summary>
        public override bool EnableCheck => _enableCheck;

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <returns>要获取的对象。</returns>
        public T Spawn()
        {
            T objectBase;
            if (UnSpawnCount == 0)
            {
                objectBase = ReferencePool.Acquire<T>();
                objectBase.OnCreate();
                objectBase.Spawn();
                _count++;
            }
            else
            {
                objectBase = _stack.Pop();
                if (EnableCheck)
                {
                    _hashSet.Remove(objectBase);
                }

                objectBase.Spawn();
            }

            return objectBase;
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="objectBase">要回收的对象。</param>
        public void UnSpawn(T objectBase)
        {
            if (objectBase == null)
            {
                throw new Exception("Object is invalid.");
            }

            if (EnableCheck && _hashSet.Contains(objectBase))
            {
                throw new Exception("Object has already been unSpawn to the pool.");
            }

            if (UnSpawnCount > Capacity)
            {
                Release(objectBase);
                return;
            }

            objectBase.UnSpawn();

            _stack.Push(objectBase);
            if (EnableCheck)
            {
                _hashSet.Add(objectBase);
            }
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="objectBase">要释放的对象。</param>
        /// <returns>释放对象是否成功。</returns>
        private void Release(T objectBase)
        {
            if (objectBase == null)
            {
                throw new Exception("Object is invalid.");
            }

            objectBase.OnDestroy(false);

            if (EnableCheck)
            {
                _hashSet.Remove(objectBase);
            }

            ReferencePool.Release(objectBase);
            _count--;
        }

        /// <summary>
        /// 释放指定数量的对象
        /// </summary>
        /// <param name="releaseCount">要释放的数量</param>
        private void Release(int releaseCount)
        {
            if (releaseCount <= 0) return;
            if (releaseCount > Count) releaseCount = Count;
            for (var i = 0; i < releaseCount; i++)
            {
                var objectBase = _stack.Pop();
                Release(objectBase);
            }
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public override void ReleaseAllUnused()
        {
            for (var i = _stack.Count; i > 0; i--)
            {
                var objectBase = _stack.Pop();
                Release(objectBase);
            }
        }

        internal override void OnDestroy()
        {
            for (var i = _stack.Count; i > 0; i--)
            {
                var objectBase = _stack.Pop();
                objectBase.OnDestroy(true);
            }

            _stack.Clear();
            _hashSet.Clear();
            _count = 0;
            _capacity = 0;
            _enableCheck = false;
        }
        
        internal override void Update()
        {
        }

        private static string GetFullName(Type type, string name)
        {
            return string.IsNullOrEmpty(name) ? type.Name : $"{type.Name}.{name}";
        }
    }
}