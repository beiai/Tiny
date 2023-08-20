using UniEngine.Base.ReferencePool;

namespace UniEngine.Module.Timer
{
    /// <summary>
    /// 暂停的定时器
    /// </summary>
    public class PausedTimer : IReference
    {
        /// <summary>
        /// 被暂停的定时器
        /// </summary>
        public Timer Timer { get; private set; }

        /// <summary>
        /// 暂停时间
        /// </summary>
        public long PausedTime { get; private set; }

        /// <summary>
        /// 获取剩余运行时间
        /// </summary>
        /// <returns>剩余运行时间</returns>
        public long GetResidueTime()
        {
            return Timer.Time + Timer.StartTime - PausedTime;
        }

        /// <summary>
        /// 创建定时器
        /// </summary>
        /// <param name="pausedTime">暂停时间</param>
        /// <param name="pauseTimer">暂停的计时器</param>
        /// <returns>暂停的定时器</returns>
        public static PausedTimer Create(long pausedTime, Timer pauseTimer)
        {
            var timer = ReferencePool.Acquire<PausedTimer>();
            timer.Timer = pauseTimer;
            timer.PausedTime = pausedTime;
            return timer;
        }

        public void Clear()
        {
            Timer = null;
            PausedTime = 0;
        }
    }
}