using System;
using System.Collections.Generic;

namespace UniEngine.Base.Module
{
    public static class ModuleManager
    {
        private static readonly LinkedList<IModule> Modules = new();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        public static void Update()
        {
            foreach (var module in Modules)
            {
                module.Update();
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (var current = Modules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            Modules.Clear();
            ReferencePool.ReferencePool.ClearAll();
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class, IModule, new()
        {
            foreach (var module in Modules)
            {
                if (module.GetType() == typeof(T))
                {
                    return (T)module;
                }
            }

            return CreateModule<T>();
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <returns>要创建的游戏框架模块。</returns>
        private static T CreateModule<T>() where T : class, IModule, new()
        {
            var module = new T();
            if (module == null)
            {
                throw new Exception($"Can not create module '{typeof(T).Name}'.");
            }

            var current = Modules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                Modules.AddBefore(current, module);
            }
            else
            {
                Modules.AddLast(module);
            }
            
            module.OnCreate();

            return (T)module;
        }
    }
}

