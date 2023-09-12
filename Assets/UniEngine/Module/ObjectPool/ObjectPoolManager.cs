using System;
using System.Collections.Generic;
using UniEngine.Base.Module;

namespace UniEngine.Module.ObjectPool
{
    /// <summary>
    /// 对象池管理器。
    /// </summary>
    public sealed class ObjectPoolManager : IModule
    {
        private readonly Dictionary<string, ObjectPoolBase> _objectPools;
        private readonly List<ObjectPoolBase> _cachedAllObjectPools;
        private readonly Comparison<ObjectPoolBase> _objectPoolComparer;

        private static ObjectPoolManager _instance;

        /// <summary>
        /// 初始化对象池管理器的新实例。
        /// </summary>
        public ObjectPoolManager()
        {
            _objectPools = new Dictionary<string, ObjectPoolBase>();
            _cachedAllObjectPools = new List<ObjectPoolBase>();
            _objectPoolComparer = ObjectPoolComparer;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority => 0;

        /// <summary>
        /// 获取对象池数量。
        /// </summary>
        public int Count => _objectPools.Count;

        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<ObjectPoolManager>();
                }

                return _instance;
            }
        }

        public void OnCreate()
        {
            _instance = this;
        }

        /// <summary>
        /// 对象池管理器轮询。
        /// </summary>
        public void Update()
        {
            foreach (var objectPool in _objectPools)
            {
                objectPool.Value.Update();
            }
        }

        /// <summary>
        /// 关闭并清理对象池管理器。
        /// </summary>
        public void Shutdown()
        {
            foreach (var objectPool in _objectPools)
            {
                objectPool.Value.OnDestroy();
            }

            _objectPools.Clear();
            _cachedAllObjectPools.Clear();
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool<T>() where T : ObjectBase
        {
            return InternalHasObjectPool(typeof(T).Name);
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool<T>(string name) where T : ObjectBase
        {
            return InternalHasObjectPool(GetFullName(typeof(T), name));
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要获取的对象池。</returns>
        public ObjectPool<T> GetObjectPool<T>() where T : ObjectBase, new()
        {
            return (ObjectPool<T>)InternalGetObjectPool(typeof(T).Name);
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase, new()
        {
            return (ObjectPool<T>)InternalGetObjectPool(GetFullName(typeof(T), name));
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <param name="condition">要检查的条件。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPoolBase[] GetObjectPools(Predicate<ObjectPoolBase> condition)
        {
            if (condition == null)
            {
                throw new Exception("Condition is invalid.");
            }

            var results = new List<ObjectPoolBase>();
            foreach (var objectPool in _objectPools)
            {
                if (condition(objectPool.Value))
                {
                    results.Add(objectPool.Value);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <param name="condition">要检查的条件。</param>
        /// <param name="results">要获取的对象池。</param>
        public void GetObjectPools(Predicate<ObjectPoolBase> condition, List<ObjectPoolBase> results)
        {
            if (condition == null)
            {
                throw new Exception("Condition is invalid.");
            }

            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var objectPool in _objectPools)
            {
                if (condition(objectPool.Value))
                {
                    results.Add(objectPool.Value);
                }
            }
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <returns>所有对象池。</returns>
        public ObjectPoolBase[] GetAllObjectPools()
        {
            return GetAllObjectPools(false);
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="results">所有对象池。</param>
        public void GetAllObjectPools(List<ObjectPoolBase> results)
        {
            GetAllObjectPools(false, results);
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <returns>所有对象池。</returns>
        public ObjectPoolBase[] GetAllObjectPools(bool sort)
        {
            if (sort)
            {
                var results = new List<ObjectPoolBase>();
                foreach (var objectPool in _objectPools)
                {
                    results.Add(objectPool.Value);
                }

                results.Sort(_objectPoolComparer);
                return results.ToArray();
            }
            else
            {
                var index = 0;
                var results = new ObjectPoolBase[_objectPools.Count];
                foreach (var objectPool in _objectPools)
                {
                    results[index++] = objectPool.Value;
                }

                return results;
            }
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <param name="results">所有对象池。</param>
        public void GetAllObjectPools(bool sort, List<ObjectPoolBase> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var objectPool in _objectPools)
            {
                results.Add(objectPool.Value);
            }

            if (sort)
            {
                results.Sort(_objectPoolComparer);
            }
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="enableCheck">是否开启检查</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateObjectPool<T>(string name = "", int capacity = int.MaxValue, int priority = 0, bool enableCheck = false) where T : ObjectBase, new()
        {
            return InternalCreateObjectPool<T>(name, capacity, priority, enableCheck);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>() where T : ObjectBase
        {
            return InternalDestroyObjectPool(typeof(T).Name);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>(string name) where T : ObjectBase
        {
            return InternalDestroyObjectPool(GetFullName(typeof(T), name));
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="objectPool">要销毁的对象池。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>(ObjectPool<T> objectPool) where T : ObjectBase, new()
        {
            if (objectPool == null)
            {
                throw new Exception("Object pool is invalid.");
            }

            return InternalDestroyObjectPool(GetFullName(typeof(T), objectPool.Name));
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public void ReleaseAllUnused()
        {
            GetAllObjectPools(true, _cachedAllObjectPools);
            foreach (var objectPool in _cachedAllObjectPools)
            {
                objectPool.ReleaseAllUnused();
            }
        }

        private bool InternalHasObjectPool(string name)
        {
            return _objectPools.ContainsKey(name);
        }

        private ObjectPoolBase InternalGetObjectPool(string name)
        {
            if (_objectPools.TryGetValue(name, out var objectPool))
            {
                return objectPool;
            }

            return null;
        }

        private ObjectPool<T> InternalCreateObjectPool<T>(string name, int capacity, int priority, bool enableCheck) where T : ObjectBase, new()
        {
            var fullName = GetFullName(typeof(T), name);
            if (HasObjectPool<T>(name))
            {
                throw new Exception($"Already exist object pool '{fullName}'.");
            }

            var objectPool = new ObjectPool<T>(name, capacity, priority, enableCheck);
            _objectPools.Add(fullName, objectPool);
            return objectPool;
        }

        private bool InternalDestroyObjectPool(string name)
        {
            if (_objectPools.TryGetValue(name, out var objectPool))
            {
                objectPool.OnDestroy();
                return _objectPools.Remove(name);
            }

            return false;
        }

        private static int ObjectPoolComparer(ObjectPoolBase a, ObjectPoolBase b)
        {
            return a.Priority.CompareTo(b.Priority);
        }

        private static string GetFullName(Type type, string name)
        {
            return string.IsNullOrEmpty(name) ? type.Name : $"{type.Name}.{name}";
        }
    }
}