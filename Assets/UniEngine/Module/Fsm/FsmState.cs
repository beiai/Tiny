using System;

namespace UniEngine.Module.Fsm
{
    /// <summary>
    /// 有限状态机状态基类。
    /// </summary>
    public class FsmState
    {
        internal Fsm _fsm;

        /// <summary>
        /// 有限状态机引用。
        /// </summary>
        public Fsm Fsm => _fsm;
        
        /// <summary>
        /// 初始化有限状态机状态基类的新实例。
        /// </summary>
        public FsmState()
        {
        }

        /// <summary>
        /// 有限状态机状态初始化时调用。
        /// </summary>
        protected internal virtual void OnCreate()
        {
        }

        /// <summary>
        /// 有限状态机状态进入时调用。
        /// </summary>
        protected internal virtual void OnEnter()
        {
        }

        /// <summary>
        /// 有限状态机状态轮询时调用。
        /// </summary>
        protected internal virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 有限状态机状态离开时调用。
        /// </summary>
        /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
        protected internal virtual void OnExit(bool isShutdown)
        {
        }

        /// <summary>
        /// 有限状态机状态销毁时调用。
        /// </summary>
        protected internal virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        protected void ChangeState<TState>() where TState : FsmState
        {
            Fsm.ChangeState<TState>();
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        protected void ChangeState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState).IsAssignableFrom(stateType))
            {
                throw new Exception($"State type '{stateType.FullName}' is invalid.");
            }

            Fsm.ChangeState(stateType);
        }
    }
}