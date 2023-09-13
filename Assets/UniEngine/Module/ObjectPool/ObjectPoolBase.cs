using System;

namespace UniEngine.Module.ObjectPool
{
    /// <summary>
    /// 对象池基类。
    /// </summary>
    public abstract class ObjectPoolBase
    {
        private readonly string _name;

        /// <summary>
        /// 初始化对象池基类的新实例。
        /// </summary>
        public ObjectPoolBase()
            : this(null)
        {
        }

        /// <summary>
        /// 初始化对象池基类的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        public ObjectPoolBase(string name)
        {
            _name = name ?? string.Empty;
        }

        /// <summary>
        /// 获取对象池名称。
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 获取对象池完整名称。
        /// </summary>
        public string FullName => string.IsNullOrEmpty(_name) ? ObjectType.Name : $"{ObjectType.Name}.{_name}";

        /// <summary>
        /// 获取对象池对象类型。
        /// </summary>
        public abstract Type ObjectType { get; }

        /// <summary>
        /// 获取对象池中所有对象的数量。
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// 获取对象池中能已使用的对象的数量。
        /// </summary>
        public abstract int SpawnCount { get; }

        /// <summary>
        /// 获取对象池中能未使用的对象的数量。
        /// </summary>
        public abstract int UnSpawnCount { get; }
        
        /// <summary>
        /// 是否开启检查对象池。
        /// </summary>
        public abstract bool EnableCheck { get; }

        /// <summary>
        /// 获取或设置对象池的容量。
        /// </summary>
        public abstract int Capacity { get; set; }

        /// <summary>
        /// 获取或设置对象池的优先级。
        /// </summary>
        public abstract int Priority { get; set; }
        
        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public abstract void ReleaseAllUnused();

        internal abstract void Update();

        internal abstract void OnDestroy();
    }
}