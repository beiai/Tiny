using System;
using System.Collections.Generic;

namespace UniEngine.Base.ReferencePool
{
    /// <summary>
    /// 引用池。
    /// </summary>
    public static class ConcurrentReferencePool
    {
        private static readonly Dictionary<Type, ReferencePoolObject> ReferencePools = new();

        /// <summary>
        /// 获取或设置是否开启强制检查。
        /// </summary>
        public static bool EnableStrictCheck { get; set; } = false;

        /// <summary>
        /// 获取引用池的数量。
        /// </summary>
        public static int Count
        {
            get
            {
                lock (ReferencePools)
                {
                    return ReferencePools.Count;
                }
            }
        }

        /// <summary>
        /// 清除所有引用池。
        /// </summary>
        public static void ClearAll()
        {
            lock (ReferencePools)
            {
                foreach (var referenceCollection in ReferencePools)
                {
                    referenceCollection.Value.RemoveAll();
                }

                ReferencePools.Clear();
            }
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <returns>引用。</returns>
        public static IReference Acquire<T>() where T : class, IReference, new()
        {
            return GetReferencePool(typeof(T)).Acquire<T>();
        }
        
        /// <summary>
        /// 将引用归还引用池。
        /// </summary>
        /// <param name="reference">引用。</param>
        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new Exception("Reference is invalid.");
            }

            var referenceType = reference.GetType();
            InternalCheckReferenceType(referenceType);
            GetReferencePool(referenceType).Release(reference);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferencePool(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public static void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferencePool(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            GetReferencePool(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public static void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferencePool(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferencePool(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferencePool(referenceType).RemoveAll();
        }

        /// <summary>
        /// 检查引用类型。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        private static void InternalCheckReferenceType(Type referenceType)
        {
            if (!EnableStrictCheck)
            {
                return;
            }

            if (referenceType == null)
            {
                throw new Exception("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new Exception("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(referenceType))
            {
                throw new Exception($"Reference type '{referenceType.FullName}' is invalid.");
            }
        }
        
        /// <summary>
        /// 获取指定类型的引用池
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <returns>引用池</returns>
        public static ReferencePoolObject GetReferencePool<T>() where T : class, IReference
        {
            return InternalGetReferencePool(typeof(T));
        }
        
        /// <summary>
        /// 获取指定类型的引用池
        /// </summary>
        /// <param name="referenceType">引用池类型</param>
        /// <returns>引用池</returns>
        public static ReferencePoolObject GetReferencePool(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            return InternalGetReferencePool(referenceType);
        }

        /// <summary>
        /// 获取指定类型的引用池
        /// </summary>
        /// <param name="referenceType">引用池类型</param>
        /// <returns>引用池</returns>
        private static ReferencePoolObject InternalGetReferencePool(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new Exception("ReferenceType is invalid.");
            }

            ReferencePoolObject referencePoolObject;
            lock (ReferencePools)
            {
                if (!ReferencePools.TryGetValue(referenceType, out referencePoolObject))
                {
                    referencePoolObject = new ReferencePoolObject(referenceType);
                    ReferencePools.Add(referenceType, referencePoolObject);
                }
            }

            return referencePoolObject;
        }
    }
}