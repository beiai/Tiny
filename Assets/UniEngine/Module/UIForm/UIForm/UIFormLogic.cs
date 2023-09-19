using UnityEngine;

namespace UniEngine.Module.UIForm.UIForm
{
    /// <summary>
    /// 界面逻辑基类。
    /// </summary>
    public abstract class UIFormLogic : MonoBehaviour
    {
        private bool _available;
        private bool _visible;
        private UIForm _uiForm;
        private Transform _cachedTransform;
        private int _originalLayer;

        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIForm UIForm => _uiForm;

        /// <summary>
        /// 获取或设置界面名称。
        /// </summary>
        public string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        public bool Available => _available;

        /// <summary>
        /// 获取或设置界面是否可见。
        /// </summary>
        public bool Visible
        {
            get => _available && _visible;
            set
            {
                if (!_available)
                {
                    Debug.LogWarning($"UI form '{Name}' is not available.");
                    return;
                }

                if (_visible == value)
                {
                    return;
                }

                _visible = value;
                InternalSetVisible(value);
            }
        }

        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Transform CachedTransform => _cachedTransform;

        /// <summary>
        /// 界面初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnInit(object userData)
        {
            if (_cachedTransform == null)
            {
                _cachedTransform = transform;
            }

            _uiForm = GetComponent<UIForm>();
            _originalLayer = gameObject.layer;
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnOpen(object userData)
        {
            _available = true;
            Visible = true;
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnClose(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(_originalLayer);
            Visible = false;
            _available = false;
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        protected internal virtual void OnPause()
        {
            Visible = false;
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        protected internal virtual void OnResume()
        {
            Visible = true;
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        protected internal virtual void OnCover()
        {
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        protected internal virtual void OnReveal()
        {
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnRefocus(object userData)
        {
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        protected internal virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
        }

        /// <summary>
        /// 设置界面的可见性。
        /// </summary>
        /// <param name="visible">界面的可见性。</param>
        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}