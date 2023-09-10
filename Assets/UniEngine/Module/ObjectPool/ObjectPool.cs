using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dnlib.DotNet.Resources;
using UniEngine.Base.Collections;
using UniEngine.Base.ReferencePool;
using UnityEngine;

namespace UniEngine.Module.ObjectPool
{
    /// <summary>
    /// 对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public sealed class ObjectPool<T> : ObjectPoolBase where T : ObjectBase
    {
        private readonly Dictionary<string, HashSet<Object<T>>> _spawnObjects = new();
        private readonly Dictionary<string, HashSet<Object<T>>> _unSpawnObjects = new();
        private readonly Dictionary<object, Object<T>> _objectMap = new();
        private readonly SortedDictionary<long, HashSet<Object<T>>> _timer = new();
        private readonly Queue<long> _expireTimeQueue = new();
        private float _autoReleaseInterval;
        private int _capacity;
        private float _expireTime;
        private float _autoReleaseTime;
        private long _minExpireTime;

        /// <summary>
        /// 初始化对象池的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        public ObjectPool(string name, float autoReleaseInterval, int capacity, float expireTime,
            int priority)
            : base(name)
        {
            _autoReleaseInterval = autoReleaseInterval;
            Capacity = capacity;
            ExpireTime = expireTime;
            Priority = priority;
            _autoReleaseTime = 0f;
        }

        /// <summary>
        /// 获取对象池对象类型。
        /// </summary>
        public override Type ObjectType => typeof(T);

        /// <summary>
        /// 获取对象池中对象的数量。
        /// </summary>
        public override int Count => _objectMap.Count;

        /// <summary>
        /// 获取对象池中能被释放的对象的数量。
        /// </summary>
        public override int CanReleaseCount
        {
            get
            {
                var count = 0;
                foreach (var keyValuePair in _unSpawnObjects)
                {
                    var unSpawnHashSet = keyValuePair.Value;
                    count += unSpawnHashSet.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// 获取或设置对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public override float AutoReleaseInterval
        {
            get => _autoReleaseInterval;
            set => _autoReleaseInterval = value;
        }

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

                if (_capacity == value)
                {
                    return;
                }

                _capacity = value;
            }
        }

        /// <summary>
        /// 获取或设置对象池对象过期秒数。
        /// </summary>
        public override float ExpireTime
        {
            get => _expireTime;

            set
            {
                if (value < 0f)
                {
                    throw new Exception("ExpireTime is invalid.");
                }

                if (ExpireTime == value)
                {
                    return;
                }

                _expireTime = value;
            }
        }

        /// <summary>
        /// 获取或设置对象池的优先级。
        /// </summary>
        public override int Priority { get; set; }

        /// <summary>
        /// 创建对象。
        /// </summary>
        /// <param name="obj">对象。</param>
        /// <param name="spawned">对象是否已被获取。</param>
        public void Register(T obj, bool spawned)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }

            var internalObject = Object<T>.Create(obj, spawned);
            if (!_spawnObjects.ContainsKey(obj.Name))
            {
                var newHashSet = new HashSet<Object<T>>();
                _spawnObjects.Add(obj.Name, newHashSet);
                if (spawned)
                {
                    newHashSet.Add(internalObject);
                }
            }
            if (!_unSpawnObjects.ContainsKey(obj.Name))
            {
                var newHashSet = new HashSet<Object<T>>();
                _unSpawnObjects.Add(obj.Name, newHashSet);
                if (!spawned)
                {
                    newHashSet.Add(internalObject);
                    AddTimer(internalObject);
                }
            }
            _objectMap.Add(obj.Target, internalObject);
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn()
        {
            return CanSpawn(string.Empty);
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn(string name)
        {
            if (name == null)
            {
                throw new Exception("Name is invalid.");
            }

            return _unSpawnObjects.TryGetValue(name, out var hashSet) && hashSet.Count > 0;
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <returns>要获取的对象。</returns>
        public T Spawn()
        {
            return Spawn(string.Empty);
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要获取的对象。</returns>
        public T Spawn(string name)
        {
            if (name == null)
            {
                throw new Exception("Name is invalid.");
            }

            if (_unSpawnObjects.TryGetValue(name, out var unSpawnHashSet) && unSpawnHashSet.Count > 0)
            {
                foreach (var internalObject in unSpawnHashSet)
                {
                    if (_spawnObjects.TryGetValue(internalObject.Name, out var spawnHashSet))
                    {
                        spawnHashSet.Add(internalObject);
                    }

                    unSpawnHashSet.Remove(internalObject);
                    RemoveTimer(internalObject);
                    return internalObject.Peek();
                }
            }

            return null;
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="obj">要回收的对象。</param>
        public void UnSpawn(T obj)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }

            UnSpawn(obj.Target);
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="target">要回收的对象。</param>
        public void UnSpawn(object target)
        {
            if (target == null)
            {
                throw new Exception("Target is invalid.");
            }

            var internalObject = GetObject(target);
            if (internalObject != null)
            {
                internalObject.UnSpawn();
                if (internalObject.Locked)
                {
                    return;
                }
                if (_spawnObjects.TryGetValue(internalObject.Name, out var spawnHashSet))
                {
                    spawnHashSet.Remove(internalObject);
                }
                if (_unSpawnObjects.TryGetValue(internalObject.Name, out var unSpawnHashSet))
                {
                    unSpawnHashSet.Add(internalObject);
                    AddTimer(internalObject);
                }
            }
            else
            {
                throw new Exception(
                    $"Can not find target in object pool '{GetFullName(typeof(T), Name)}', target type is '{target.GetType().FullName}', target value is '{target}'.");
            }
        }

        /// <summary>
        /// 设置对象是否被加锁。
        /// </summary>
        /// <param name="obj">要设置被加锁的对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked(T obj, bool locked)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }

            SetLocked(obj.Target, locked);
        }

        /// <summary>
        /// 设置对象是否被加锁。
        /// </summary>
        /// <param name="target">要设置被加锁的对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked(object target, bool locked)
        {
            if (target == null)
            {
                throw new Exception("Target is invalid.");
            }

            var internalObject = GetObject(target);
            if (internalObject != null)
            {
                internalObject.Locked = locked;
                if (locked)
                {
                    if (_unSpawnObjects.TryGetValue(internalObject.Name, out var unSpawnHashSet) && unSpawnHashSet.Contains(internalObject))
                    {
                        unSpawnHashSet.Remove(internalObject);
                        RemoveTimer(internalObject);
                        if (_spawnObjects.TryGetValue(internalObject.Name, out var spawnHashSet))
                        {
                            spawnHashSet.Add(internalObject);
                        }
                    }
                }
                else
                {
                    if (!internalObject.IsInUse && _spawnObjects.TryGetValue(internalObject.Name, out var spawnHashSet) && spawnHashSet.Contains(internalObject))
                    {
                        spawnHashSet.Remove(internalObject);
                        if (_unSpawnObjects.TryGetValue(internalObject.Name, out var unSpawnHashSet))
                        {
                            unSpawnHashSet.Add(internalObject);
                            AddTimer(internalObject);
                        }
                    }
                }
            }
            else
            {
                throw new Exception(
                    $"Can not find target in object pool '{GetFullName(typeof(T), Name)}', target type is '{target.GetType().FullName}', target value is '{target}'.");
            }
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="obj">要释放的对象。</param>
        /// <returns>释放对象是否成功。</returns>
        public bool ReleaseObject(T obj)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }

            return ReleaseObject(obj.Target);
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="target">要释放的对象。</param>
        /// <returns>释放对象是否成功。</returns>
        public bool ReleaseObject(object target)
        {
            if (target == null)
            {
                throw new Exception("Target is invalid.");
            }

            var internalObject = GetObject(target);
            if (internalObject == null)
            {
                return false;
            }
            
            if (_unSpawnObjects.TryGetValue(internalObject.Name, out var unSpawnHashSet))
            {
                unSpawnHashSet.Remove(internalObject);
            }
            _objectMap.Remove(target);

            internalObject.Release(false);
            ReferencePool.Release(internalObject);
            return true;
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public override void Release()
        {
            Release(Count - _capacity);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        public override void Release(int toReleaseCount)
        {
            // 没有计时器不执行
            if (_timer.Count == 0)
            {
                return;
            }

            var timeNow = DateTime.UtcNow.Millisecond;
            // 不到最近的计时器时间不执行
            if (timeNow < _minExpireTime)
            {
                return;
            }
            // 判断哪些计时器可执行，并获取下一个最近的计时器时间
            foreach (var keyValuePair in _timer)
            {
                var untilTime = keyValuePair.Key;
                if (untilTime > timeNow)
                {
                    _minExpireTime = untilTime;
                    break;
                }

                _expireTimeQueue.Enqueue(untilTime);
            }
            
            while (_expireTimeQueue.Count > 0)
            {
                var expireTime = _expireTimeQueue.Dequeue();
                if (!_timer.TryGetValue(expireTime, out var hashSet))
                {
                    continue;
                }

                foreach (var internalObject in hashSet)
                {
                    ReleaseObject(internalObject.Peek());
                    toReleaseCount--;
                }
                hashSet.Clear();
                _timer.Remove(expireTime);
            }

            if (toReleaseCount <= 0)
            {
                return;
            }
            
            foreach (var keyValuePair in _unSpawnObjects)
            {
                var unSpawnHashSet = keyValuePair.Value;
                foreach (var internalObject in unSpawnHashSet)
                {
                    if (toReleaseCount > 0)
                    {
                        ReleaseObject(internalObject.Peek());
                        toReleaseCount--;
                    }
                }
            }
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public override void ReleaseAllUnused()
        {
            _autoReleaseTime = 0f;
            foreach (var keyValuePair in _unSpawnObjects)
            {
                var unSpawnHashSet = keyValuePair.Value;
                foreach (var internalObject in unSpawnHashSet)
                {
                    ReleaseObject(internalObject.Peek());
                }
            }
        }

        internal override void Update()
        {
            _autoReleaseTime += Time.unscaledDeltaTime;
            if (_autoReleaseTime < _autoReleaseInterval)
            {
                return;
            }

            Release();
        }

        internal override void Shutdown()
        {
            foreach (var objectInMap in _objectMap)
            {
                objectInMap.Value.Release(true);
                ReferencePool.Release(objectInMap.Value);
            }
            
            _spawnObjects.Clear();
            _unSpawnObjects.Clear();
            _objectMap.Clear();
        }

        private Object<T> GetObject(object target)
        {
            if (target == null)
            {
                throw new Exception("Target is invalid.");
            }

            if (_objectMap.TryGetValue(target, out var internalObject))
            {
                return internalObject;
            }

            return null;
        }

        private void AddTimer(Object<T> internalObject)
        {
            var untilTime = internalObject.LastUseTime.AddSeconds(_expireTime).Millisecond;
            if (_timer.TryGetValue(untilTime, out var hashSet))
            {
                hashSet.Add(internalObject);
            }
            else
            {
                var newHashSet = new HashSet<Object<T>> { internalObject };
                _timer.Add(untilTime, newHashSet);
            }
            
            if (untilTime < _minExpireTime)
            {
                _minExpireTime = untilTime;
            }
        }
        
        private void RemoveTimer(Object<T> internalObject)
        {
            var untilTime = internalObject.LastUseTime.AddSeconds(_expireTime).Millisecond;
            if (_timer.TryGetValue(untilTime, out var hashSet))
            {
                hashSet.Remove(internalObject);
            }
        }

        private static string GetFullName(Type type, string name)
        {
            return string.IsNullOrEmpty(name) ? type.Name : $"{type.Name}.{name}";
        }
    }
}