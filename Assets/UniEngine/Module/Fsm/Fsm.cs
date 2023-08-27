using System;
using System.Collections.Generic;
using UniEngine.Base.ReferencePool;
using UnityEngine;

namespace UniEngine.Module.Fsm
{
    /// <summary>
    /// 有限状态机。
    /// </summary>
    public sealed class Fsm: IReference
    {
        private readonly Dictionary<Type, FsmState> _states;
        private Dictionary<string, object> _dataDict;
        private FsmState _currentState;
        private float _currentStateTime;
        private bool _isDestroyed;
        private string _name;

        /// <summary>
        /// 初始化有限状态机的新实例。
        /// </summary>
        public Fsm()
        {
            _states = new Dictionary<Type, FsmState>();
            _dataDict = null;
            _currentState = null;
            _currentStateTime = 0f;
            _isDestroyed = true;
            _name = string.Empty;
        }

        /// <summary>
        /// 获取有限状态机名称。
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 获取有限状态机中状态的数量。
        /// </summary>
        public int FsmStateCount => _states.Count;

        /// <summary>
        /// 获取有限状态机是否正在运行。
        /// </summary>
        public bool IsRunning => _currentState != null;

        /// <summary>
        /// 获取有限状态机是否被销毁。
        /// </summary>
        public bool IsDestroyed => _isDestroyed;

        /// <summary>
        /// 获取当前有限状态机状态。
        /// </summary>
        public FsmState CurrentState => _currentState;

        /// <summary>
        /// 获取当前有限状态机状态名称。
        /// </summary>
        public string CurrentStateName => _currentState?.GetType().FullName;

        /// <summary>
        /// 获取当前有限状态机状态持续时间。
        /// </summary>
        public float CurrentStateTime => _currentStateTime;

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>创建的有限状态机。</returns>
        public static Fsm Create(string name, params FsmState[] states)
        {
            if (states == null || states.Length < 1)
            {
                throw new Exception("FSM states is invalid.");
            }

            var fsm = ReferencePool.Acquire<Fsm>();
            fsm._name = name;
            fsm._isDestroyed = false;
            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new Exception("FSM states is invalid.");
                }

                var stateType = state.GetType();
                if (fsm._states.ContainsKey(stateType))
                {
                    throw new Exception($"FSM '{fsm._name}' state '{stateType.FullName}' is already exist.");
                }

                fsm._states.Add(stateType, state);
                state.OnCreate();
            }

            return fsm;
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>创建的有限状态机。</returns>
        public static Fsm Create(string name, List<FsmState> states)
        {
            if (states == null || states.Count < 1)
            {
                throw new Exception("FSM states is invalid.");
            }

            var fsm = ReferencePool.Acquire<Fsm>();
            fsm._name = name;
            fsm._isDestroyed = false;
            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new Exception("FSM states is invalid.");
                }

                var stateType = state.GetType();
                if (fsm._states.ContainsKey(stateType))
                {
                    throw new Exception($"FSM '{fsm._name}' state '{stateType.FullName}' is already exist.");
                }

                fsm._states.Add(stateType, state);
                state.OnCreate();
            }

            return fsm;
        }
        
        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>创建的有限状态机。</returns>
        public static Fsm Create(string name)
        {
            var fsm = ReferencePool.Acquire<Fsm>();
            fsm._name = name;
            fsm._isDestroyed = false;
            return fsm;
        }
        
        /// <summary>
        /// 添加有限状态机状态
        /// </summary>
        public void AddState<TState>() where TState : FsmState, new()
        {
            var stateNode = new TState();
            AddState(stateNode);
        }

        public void AddState(FsmState state)
        {
            if (state == null )
            {
                throw new Exception("FSM states is invalid.");
            }
            
            var stateType = state.GetType();
            if (_states.ContainsKey(stateType))
            {
                throw new Exception($"FSM '{_name}' state '{stateType.FullName}' is already exist.");
            }

            _states.Add(stateType, state);
            state._fsm = this;
            state.OnCreate();
        }

        /// <summary>
        /// 开始有限状态机。
        /// </summary>
        /// <typeparam name="TState">要开始的有限状态机状态类型。</typeparam>
        public void Start<TState>() where TState : FsmState
        {
            if (IsRunning)
            {
                throw new Exception("FSM is running, can not start again.");
            }

            FsmState state = GetState<TState>();
            if (state == null)
            {
                throw new Exception(
                    $"FSM '{_name}' can not start state '{typeof(TState).FullName}' which is not exist.");
            }

            _currentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter();
        }

        /// <summary>
        /// 开始有限状态机。
        /// </summary>
        /// <param name="stateType">要开始的有限状态机状态类型。</param>
        public void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new Exception("FSM is running, can not start again.");
            }

            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState).IsAssignableFrom(stateType))
            {
                throw new Exception($"State type '{stateType.FullName}' is invalid.");
            }

            var state = GetState(stateType);
            if (state == null)
            {
                throw new Exception($"FSM '{_name}' can not start state '{stateType.FullName}' which is not exist.");
            }

            _currentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter();
        }

        /// <summary>
        /// 是否存在有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要检查的有限状态机状态类型。</typeparam>
        /// <returns>是否存在有限状态机状态。</returns>
        public bool HasState<TState>() where TState : FsmState
        {
            return _states.ContainsKey(typeof(TState));
        }

        /// <summary>
        /// 是否存在有限状态机状态。
        /// </summary>
        /// <param name="stateType">要检查的有限状态机状态类型。</param>
        /// <returns>是否存在有限状态机状态。</returns>
        public bool HasState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState).IsAssignableFrom(stateType))
            {
                throw new Exception($"State type '{stateType.FullName}' is invalid.");
            }

            return _states.ContainsKey(stateType);
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
        /// <returns>要获取的有限状态机状态。</returns>
        public TState GetState<TState>() where TState : FsmState
        {
            if (_states.TryGetValue(typeof(TState), out var state))
            {
                return (TState)state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <param name="stateType">要获取的有限状态机状态类型。</param>
        /// <returns>要获取的有限状态机状态。</returns>
        public FsmState GetState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState).IsAssignableFrom(stateType))
            {
                throw new Exception($"State type '{stateType.FullName}' is invalid.");
            }

            if (_states.TryGetValue(stateType, out var state))
            {
                return state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机的所有状态。
        /// </summary>
        /// <returns>有限状态机的所有状态。</returns>
        public FsmState[] GetAllStates()
        {
            var index = 0;
            var results = new FsmState[_states.Count];
            foreach (var state in _states)
            {
                results[index++] = state.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取有限状态机的所有状态。
        /// </summary>
        /// <param name="results">有限状态机的所有状态。</param>
        public void GetAllStates(List<FsmState> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var state in _states)
            {
                results.Add(state.Value);
            }
        }

        /// <summary>
        /// 是否存在有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>有限状态机数据是否存在。</returns>
        public bool HasData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (_dataDict == null)
            {
                return false;
            }

            return _dataDict.ContainsKey(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public TData GetData<TData>(string name)
        {
            return (TData)GetData(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public object GetData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (_dataDict == null)
            {
                return null;
            }

            if (_dataDict.TryGetValue(name, out var data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// 设置有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        public void SetData<TData>(string name, TData data)
        {
            SetData(name, (object)data);
        }

        /// <summary>
        /// 设置有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        public void SetData(string name, object data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (_dataDict == null)
            {
                _dataDict = new Dictionary<string, object>(StringComparer.Ordinal);
            }

            _dataDict[name] = data;
        }

        /// <summary>
        /// 移除有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>是否移除有限状态机数据成功。</returns>
        public bool RemoveData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (_dataDict == null)
            {
                return false;
            }

            return _dataDict.Remove(name);
        }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        internal void Update()
        {
            if (_currentState == null)
            {
                return;
            }

            _currentStateTime += Time.unscaledDeltaTime;
            _currentState.OnUpdate();
        }

        /// <summary>
        /// 关闭并清理有限状态机。
        /// </summary>
        internal void OnDestroy()
        {
            ReferencePool.Release(this);
        }
        
        /// <summary>
        /// 清理有限状态机。
        /// </summary>
        public void Clear()
        {
            _currentState?.OnExit(true);

            foreach (var state in _states)
            {
                state.Value.OnDestroy();
            }

            _name = string.Empty;
            _states.Clear();
            if (_dataDict != null)
            {
                _dataDict.Clear();
            }
            _currentState = null;
            _currentStateTime = 0f;
            _isDestroyed = true;
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        internal void ChangeState<TState>() where TState : FsmState
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        internal void ChangeState(Type stateType)
        {
            if (_currentState == null)
            {
                throw new Exception("Current state is invalid.");
            }

            var state = GetState(stateType);
            if (state == null)
            {
                throw new Exception(
                    $"FSM '{_name}' can not change state to '{stateType.FullName}' which is not exist.");
            }

            _currentState.OnExit(false);
            _currentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter();
        }
    }
}