using UniEngine.Module.Resource;
using UnityEngine;
using YooAsset;

namespace UniEngine.Module.Sound.SoundHelper
{
    /// <summary>
    /// 默认声音辅助器。
    /// </summary>
    public class SoundHelperDefault : SoundHelperBase
    {
        /// <summary>
        /// 加载音频资源
        /// </summary>
        /// <param name="soundAssetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="loadAssetCallback">加载资源回调函数</param>
        public override void LoadAudioAsset(string soundAssetName, int priority, object userData,
            LoadAssetCallbacks loadAssetCallback)
        {
            var assetOperationHandle = YooAssets.LoadAssetAsync<AudioClip>(soundAssetName);
            assetOperationHandle.Completed += handle =>
            {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    var playSoundInfo = (PlaySoundInfo)userData;
                    playSoundInfo.AssetHandle = handle;
                    loadAssetCallback.LoadAssetSuccessCallback.Invoke(soundAssetName, handle.AssetObject, userData);
                }
                else
                {
                    loadAssetCallback.LoadAssetFailureCallback.Invoke(soundAssetName, handle.LastError, userData);
                }
            };
        }
    }
}