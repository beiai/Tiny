using UniEngine.Module.Resource;
using UniEngine.Module.UIForm.UIGroup;
using UnityEngine;
using UnityEngine.Pool;
using YooAsset;

namespace UniEngine.Module.UIForm.UIForm
{
    /// <summary>
    /// 默认界面辅助器。
    /// </summary>
    public class UIFormHelperDefault : UIFormHelperBase
    {
        private IObjectPool<UIFormInstanceObject> _uiFormObjectPool;

        /// <summary>
        /// 加载界面资源
        /// </summary>
        /// <param name="uiAssetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        public override void LoadUIFormAsset(string uiAssetName, int priority, object userData, LoadAssetCallbacks loadAssetCallbacks)
        {
            var handle = YooAssets.LoadAssetAsync<GameObject>(uiAssetName);
            handle.Completed += assetOperationHandle =>
            {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    var openUIFormInfo = (OpenUIFormInfo)userData;
                    loadAssetCallbacks.LoadAssetSuccessCallback.Invoke(uiAssetName, handle.AssetObject, userData);
                }
                else
                {
                    loadAssetCallbacks.LoadAssetFailureCallback.Invoke(uiAssetName, handle.LastError, userData);
                }
            };
        }

        /// <summary>
        /// 获取界面资源
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称</param>
        /// <returns></returns>
        public override UIFormInstanceObject GetUIFormAsset(string uiFormAssetName)
        {
            if (_uiFormObjectPool == null)
            {
                // _uiFormObjectPool =
                //     ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
                return null;
            }

            // return _uiFormObjectPool.Spawn(uiFormAssetName);
            return null;
        }

        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAssetName"></param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>实例化后的界面。</returns>
        public override object AddUIFormAsset(string uiFormAssetName, object userData)
        {
            var openUIFormInfo = (OpenUIFormInfo)userData;
            return null;
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public override IUIForm CreateUIForm(object uiFormInstance, UIGroup.UIGroup uiGroup, object userData)
        {
            var formInstance = uiFormInstance as GameObject;
            if (formInstance == null)
            {
                Debug.LogError("UI form instance is invalid.");
                return null;
            }

            var formInstanceTransform = formInstance.transform;
            formInstanceTransform.SetParent(uiGroup.transform);
            formInstanceTransform.localScale = Vector3.one;
            var rectTransform = formInstanceTransform.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            return formInstance.GetOrAddComponent<UIForm>();
        }

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public override void ReleaseUIForm(object uiFormInstance)
        {
            // _uiFormObjectPool.UnSpawn(uiFormInstance);
        }
    }
}