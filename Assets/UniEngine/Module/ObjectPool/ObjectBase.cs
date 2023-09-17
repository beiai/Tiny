using UniEngine.Base.ReferencePool;

namespace UniEngine.Module.ObjectPool
{
    /// <summary>
    /// 对象基类。
    /// </summary>
    public abstract class ObjectBase : IReference
    {
        /// <summary>
        /// 初始化对象基类的新实例。
        /// </summary>
        public ObjectBase()
        {
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public abstract void Clear();
        
        /// <summary>
        /// 创建对象。
        /// </summary>
        protected internal abstract void OnCreate();

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        protected internal virtual void Spawn()
        {
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        protected internal virtual void UnSpawn()
        {
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="isShutdown">是否是关闭对象池时触发。</param>
        protected internal abstract void OnDestroy(bool isShutdown);
    }
}