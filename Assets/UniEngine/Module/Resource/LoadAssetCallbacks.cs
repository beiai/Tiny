using System;

namespace UniEngine.Module.Resource
{
    /// <summary>
    /// 加载资源回调函数集。
    /// </summary>
    public sealed class LoadAssetCallbacks
    {
        private readonly LoadAssetSuccessCallback _loadAssetSuccessCallback;
        private readonly LoadAssetFailureCallback _loadAssetFailureCallback;

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback,
            LoadAssetFailureCallback loadAssetFailureCallback)
        {
            if (loadAssetSuccessCallback == null)
            {
                throw new Exception("Load asset success callback is invalid.");
            }

            _loadAssetSuccessCallback = loadAssetSuccessCallback;
            _loadAssetFailureCallback = loadAssetFailureCallback;
        }

        /// <summary>
        /// 获取加载资源成功回调函数。
        /// </summary>
        public LoadAssetSuccessCallback LoadAssetSuccessCallback => _loadAssetSuccessCallback;

        /// <summary>
        /// 获取加载资源失败回调函数。
        /// </summary>
        public LoadAssetFailureCallback LoadAssetFailureCallback => _loadAssetFailureCallback;
    }
}