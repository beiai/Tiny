namespace UniEngine.Module.Resource
{
    /// <summary>
    /// 加载资源成功回调函数。
    /// </summary>
    /// <param name="assetName">要加载的资源名称。</param>
    /// <param name="asset">已加载的资源。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadAssetSuccessCallback(string assetName, object asset, object userData);
}
