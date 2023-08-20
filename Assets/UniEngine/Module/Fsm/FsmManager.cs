using System;
using System.Collections.Generic;
using UniEngine.Base.Module;

namespace UniEngine.Module.Fsm
{
    /// <summary>
    /// 有限状态机管理器。
    /// </summary>
    public sealed class FsmManager : IModule
    {
        private readonly Dictionary<string, Fsm> _fsmDict;
        private readonly List<Fsm> _tempFsmList;
        private static FsmManager _instance;

        /// <summary>
        /// 初始化有限状态机管理器的新实例。
        /// </summary>
        public FsmManager()
        {
            _fsmDict = new Dictionary<string, Fsm>();
            _tempFsmList = new List<Fsm>();
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority => 0;

        /// <summary>
        /// 获取有限状态机数量。
        /// </summary>
        public int Count => _fsmDict.Count;

        public static FsmManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<FsmManager>();
                }

                return _instance;
            }
        }

        public void OnCreate()
        {
            _instance = this;
        }

        /// <summary>
        /// 有限状态机管理器轮询。
        /// </summary>
        public void Update()
        {
            _tempFsmList.Clear();
            if (_fsmDict.Count <= 0)
            {
                return;
            }

            // 防止状态机在Update过程中删除状态机所导致的问题
            foreach (var fsm in _fsmDict)
            {
                _tempFsmList.Add(fsm.Value);
            }

            foreach (var fsm in _tempFsmList)
            {
                if (fsm.IsDestroyed)
                {
                    continue;
                }

                fsm.Update();
            }
        }

        /// <summary>
        /// 关闭并清理有限状态机管理器。
        /// </summary>
        public void Shutdown()
        {
            foreach (var fsm in _fsmDict)
            {
                fsm.Value.OnDestroy();
            }

            _fsmDict.Clear();
            _tempFsmList.Clear();
        }

        /// <summary>
        /// 检查是否存在有限状态机。
        /// </summary>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm(string name)
        {
            return _fsmDict.ContainsKey(name);
        }

        /// <summary>
        /// 获取有限状态机。
        /// </summary>
        /// <returns>要获取的有限状态机。</returns>
        public Fsm GetFsm(string name)
        {
            if (_fsmDict.TryGetValue(name, out var fsm))
            {
                return fsm;
            }

            return null;
        }

        /// <summary>
        /// 获取所有有限状态机。
        /// </summary>
        /// <returns>所有有限状态机。</returns>
        public Fsm[] GetAllFsm()
        {
            var index = 0;
            var results = new Fsm[_fsmDict.Count];
            foreach (var fsm in _fsmDict)
            {
                results[index++] = fsm.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有有限状态机。
        /// </summary>
        /// <param name="results">所有有限状态机。</param>
        public void GetAllFsm(List<Fsm> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var fsm in _fsmDict)
            {
                results.Add(fsm.Value);
            }
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public Fsm CreateFsm(string name, params FsmState[] states)
        {
            if (HasFsm(name))
            {
                throw new Exception($"Already exist FSM '{name}'.");
            }

            var fsm = Fsm.Create(name, states);
            _fsmDict.Add(name, fsm);
            return fsm;
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public Fsm CreateFsm(string name, List<FsmState> states)
        {
            if (HasFsm(name))
            {
                throw new Exception($"Already exist FSM '{name}'.");
            }

            var fsm = Fsm.Create(name, states);
            _fsmDict.Add(name, fsm);
            return fsm;
        }
        
        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>要创建的有限状态机。</returns>
        public Fsm CreateFsm(string name)
        {
            if (HasFsm(name))
            {
                throw new Exception($"Already exist FSM '{name}'.");
            }

            var fsm = Fsm.Create(name);
            _fsmDict.Add(name, fsm);
            return fsm;
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <param name="name">要销毁的有限状态机名称。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm(string name)
        {
            if (_fsmDict.TryGetValue(name, out var fsm))
            {
                fsm.OnDestroy();
                return _fsmDict.Remove(name);
            }

            return false;
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm(Fsm fsm)
        {
            if (fsm == null)
            {
                throw new Exception("FSM is invalid.");
            }

            return DestroyFsm(fsm.Name);
        }
    }
}