using UniEngine.Module.Resource;
using UniEngine.Module.UIForm.UIGroup;
using UnityEngine;

namespace UniEngine.Module.UIForm.UIForm
{
    /// <summary>
    /// 界面辅助器基类。
    /// </summary>
    public abstract class UIFormHelperBase : MonoBehaviour, IUIFormHelper
    {
        /// <summary>
        /// 加载界面资源
        /// </summary>
        /// <param name="uiAssetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public abstract void LoadUIFormAsset(string uiAssetName, int priority, object userData, LoadAssetCallbacks loadAssetCallbacks);

        /// <summary>
        /// 获取界面资源
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称</param>
        /// <returns></returns>
        public abstract UIFormInstanceObject GetUIFormAsset(string uiFormAssetName);

        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAssetName"></param>
        /// <param name="userData">用户自定义数据。</param>
        public abstract object AddUIFormAsset(string uiFormAssetName, object userData);

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public abstract IUIForm CreateUIForm(object uiFormInstance, UIGroup.UIGroup uiGroup, object userData);

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public abstract void ReleaseUIForm(object uiFormInstance);
    }
}
