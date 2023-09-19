using UniEngine.Module.Resource;

namespace UniEngine.Module.Sound.SoundHelper
{
    /// <summary>
    /// 声音辅助器接口。
    /// </summary>
    public interface ISoundHelper
    {
        /// <summary>
        /// 加载音频资源
        /// </summary>
        /// <param name="soundAssetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="loadAssetCallback">加载资源回调函数集</param>
        void LoadAudioAsset(string soundAssetName, int priority, object userData, LoadAssetCallbacks loadAssetCallback);
    }
}