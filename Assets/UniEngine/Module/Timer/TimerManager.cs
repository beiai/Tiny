using System;
using System.Collections.Generic;
using UniEngine.Base.Collections;
using UniEngine.Base.Module;
using UniEngine.Base.ReferencePool;
using UnityEngine;

namespace UniEngine.Module.Timer
{
    public class TimerManager : IModule
    {
        /// <summary>
        /// 存储所有的计时器
        /// </summary>
        private readonly Dictionary<int, Timer> _timers = new();

        /// <summary>
        /// 所有暂停的计时器
        /// </summary>
        private readonly Dictionary<int, PausedTimer> _pausedTimer = new();

        /// <summary>
        /// 所有每帧回调的计时器
        /// </summary>
        private readonly Dictionary<int, Timer> _updateTimer = new();

        /// <summary>
        /// 根据timer的到期时间存储 对应的 N个timerId
        /// </summary>
        private readonly SortedDictionaryMultiValue<long, int> _timeId = new();

        /// <summary>
        /// 需要执行的 到期时间
        /// </summary>
        private readonly Queue<long> _timeOutTime = new();

        /// <summary>
        /// 到期的所有 timerId
        /// </summary>
        private readonly Queue<int> _timeOutTimerIds = new();

        private static TimerManager _instance;

        /// <summary>
        /// 记录最小时间，不用每次都去MultiMap取第一个值
        /// </summary>
        private long _minTime;

        public int Priority => 0;

        public static TimerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<TimerManager>();
                }

                return _instance;
            }
        }

        public void OnCreate()
        {
            _instance = this;
        }

        public void Update()
        {
            // 每帧 Update 回调
            RunUpdateCallBack();

            // 没有计时器不执行
            if (_timeId.Count == 0)
            {
                return;
            }

            // 不到最近的计时器时间不执行
            var timeNow = TimerUtility.Now();
            if (timeNow < _minTime)
            {
                return;
            }

            // 判断哪些计时器可执行，并获取下一个最近的计时器时间
            foreach (var keyValuePair in _timeId)
            {
                var untilTime = keyValuePair.Key;
                if (untilTime > timeNow)
                {
                    _minTime = untilTime;
                    break;
                }

                _timeOutTime.Enqueue(untilTime);
            }

            while (_timeOutTime.Count > 0)
            {
                var outTime = _timeOutTime.Dequeue();
                if (!_timeId.TryGetValue(outTime, out var timerIds))
                {
                    Debug.LogError("当前时间没有可执行的计时器！");
                    continue;
                }

                foreach (var timerId in timerIds)
                {
                    _timeOutTimerIds.Enqueue(timerId);
                }

                _timeId.RemoveAll(outTime);
            }

            while (_timeOutTimerIds.Count > 0)
            {
                var timerId = _timeOutTimerIds.Dequeue();

                _timers.TryGetValue(timerId, out var timer);
                if (timer == null)
                {
                    Debug.LogError($"获取Timer失败！ID:{timerId}");
                    continue;
                }

                RunTimer(timer);
            }
        }

        public void Shutdown()
        {
            foreach (var timer in _timers)
            {
                RemoveTimerInternal(timer.Value);
            }
            _timers.Clear();
        }

        /// <summary>
        /// 执行每帧回调
        /// </summary>
        private void RunUpdateCallBack()
        {
            if (_updateTimer.Count == 0)
            {
                return;
            }

            var timeNow = TimerUtility.Now();
            foreach (var timer in _updateTimer.Values)
            {
                timer.UpdateCallBack?.Invoke(timer.Time + timer.StartTime - timeNow);
            }
        }

        /// <summary>
        /// 执行定时器回调
        /// </summary>
        /// <param name="timer">定时器</param>
        private void RunTimer(Timer timer)
        {
            switch (timer.TimerType)
            {
                case TimerType.Once:
                {
                    var timeNow = TimerUtility.Now();
                    var timeLeft = timer.Time + timer.StartTime - timeNow;
                    timer.Callback?.Invoke(timeLeft, timer.UserData);
                    RemoveTimer(timer.ID);
                    break;
                }
                case TimerType.Repeated:
                {
                    var timeNow = TimerUtility.Now();
                    var timeLeft = timer.Time + timer.StartTime - timeNow;
                    var untilTime = timeNow + timer.Time;
                    timer.Callback?.Invoke(timeLeft, timer.UserData);
                    if (timer.RepeatCount == 1)
                    {
                        RemoveTimer(timer.ID);
                    }
                    else
                    {
                        if (timer.RepeatCount > 1)
                        {
                            timer.RepeatCount--;
                        }

                        timer.StartTime = timeNow;
                        AddTimer(untilTime, timer.ID);
                    }
                    
                    break;
                }
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="untilTime">延时时间</param>
        /// <param name="id">定时器ID</param>
        private void AddTimer(long untilTime, int id)
        {
            _timeId.Add(untilTime, id);
            if (untilTime < _minTime)
            {
                _minTime = untilTime;
            }
        }

        /// <summary>
        /// 删除定时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void RemoveTimer(int id)
        {
            _timers.TryGetValue(id, out var timer);
            if (timer == null)
            {
                Debug.LogError($"删除了不存在的Timer ID:{id}");
                return;
            }

            RemoveTimerInternal(timer);
            _timers.Remove(id);
        }

        /// <summary>
        /// 删除计时器
        /// </summary>
        /// <param name="timer">定时器</param>
        private void RemoveTimerInternal(Timer timer)
        {
            _timeId.Remove(timer.StartTime + timer.Time, timer.ID);
            ReferencePool.Release(timer);
            
            _updateTimer.Remove(timer.ID);
            if (!_pausedTimer.ContainsKey(timer.ID)) return;
            ReferencePool.Release(_pausedTimer[timer.ID]);
            _pausedTimer.Remove(timer.ID);
        }

        /// <summary>
        /// 查询是否存在计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public bool IsExistTimer(int id)
        {
            return _pausedTimer.ContainsKey(id) || _timers.ContainsKey(id);
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void PauseTimer(int id)
        {
            _timers.TryGetValue(id, out var timer);
            if (timer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timeId.Remove(timer.StartTime + timer.Time, timer.ID);
            _timers.Remove(id);
            _updateTimer.Remove(id);
            var pausedTimer = PausedTimer.Create(TimerUtility.Now(), timer);
            _pausedTimer.Add(id, pausedTimer);
        }

        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void ResumeTimer(int id)
        {
            _pausedTimer.TryGetValue(id, out var pausedTimer);
            if (pausedTimer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timers.Add(id, pausedTimer.Timer);
            if (pausedTimer.Timer.UpdateCallBack != null)
            {
                _updateTimer.Add(id, pausedTimer.Timer);
            }

            var untilTime = TimerUtility.Now() + pausedTimer.GetResidueTime();
            pausedTimer.Timer.StartTime += TimerUtility.Now() - pausedTimer.PausedTime;
            AddTimer(untilTime, pausedTimer.Timer.ID);
            ReferencePool.Release(pausedTimer);
            _pausedTimer.Remove(id);
        }

        /// <summary>
        /// 修改定时器启动时间
        /// </summary>
        /// <param name="id">定时器ID</param>
        /// <param name="time">时间偏移</param>
        /// <param name="isChangeRepeat">如果是 RepeatTimer 是否修改每次运行时间</param>
        public void ChangeTime(int id, long time, bool isChangeRepeat = false)
        {
            _pausedTimer.TryGetValue(id, out var pausedTimer);
            if (pausedTimer?.Timer != null)
            {
                pausedTimer.Timer.Time += time;
                return;
            }

            _timers.TryGetValue(id, out var timer);
            if (timer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timeId.Remove(timer.StartTime + timer.Time, timer.ID);
            if (timer.TimerType == TimerType.Repeated && !isChangeRepeat)
            {
                timer.StartTime += time;
            }
            else
            {
                timer.Time += time;
            }

            AddTimer(timer.StartTime + timer.Time, timer.ID);
        }

        /// <summary>
        /// 添加执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="callback">回调函数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="updateCallBack">每帧回调函数</param>
        /// <returns></returns>
        public int AddOnceTimer(long time, Action<long, object> callback, object userData = null,
            Action<long> updateCallBack = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            var nowTime = TimerUtility.Now();
            var timer = Timer.Create(time, nowTime, TimerType.Once, callback, userData, 1, updateCallBack);
            _timers.Add(timer.ID, timer);
            if (updateCallBack != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }

        /// <summary>
        /// 添加执行多次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="repeatCount">重复次数 (小于等于零 无限次调用） </param>
        /// <param name="callback">回调函数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="updateCallback">每帧回调函数</param>
        /// <returns>定时器 ID</returns>
        /// <exception cref="Exception">定时时间太短 无意义</exception>
        public int AddRepeatedTimer(long time, int repeatCount, Action<long, object> callback, object userData = null,
            Action<long> updateCallback = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            var nowTime = TimerUtility.Now();
            var timer = Timer.Create(time, nowTime, TimerType.Repeated, callback, userData, repeatCount,
                updateCallback);
            _timers.Add(timer.ID, timer);
            if (updateCallback != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }
    }
}