using System;
using UniEngine.Module.UIForm.UIGroup;
using UnityEngine;

namespace UniEngine.Module.UIForm.UIForm
{
    /// <summary>
    /// 界面。
    /// </summary>
    public sealed class UIForm : MonoBehaviour, IUIForm
    {
        private int _serialId;
        private string _uiFormAssetName;
        private UIGroup.UIGroup _uiGroup;
        private int _depthInUIGroup;
        private bool _pauseCoveredUIForm;
        private UIFormLogic _uiFormLogic;

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId => _serialId;

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName => _uiFormAssetName;

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        public object Handle => gameObject;

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public UIGroup.UIGroup UIGroup => _uiGroup;

        /// <summary>
        /// 获取界面深度。
        /// </summary>
        public int DepthInUIGroup => _depthInUIGroup;

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm => _pauseCoveredUIForm;

        /// <summary>
        /// 获取界面逻辑。
        /// </summary>
        public UIFormLogic Logic => _uiFormLogic;

        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所处的界面组。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int serialId, string uiFormAssetName, UIGroup.UIGroup uiGroup, bool pauseCoveredUIForm,
            bool isNewInstance, object userData)
        {
            _serialId = serialId;
            _uiFormAssetName = uiFormAssetName;
            _uiGroup = uiGroup;
            _depthInUIGroup = 0;
            _pauseCoveredUIForm = pauseCoveredUIForm;

            if (!isNewInstance)
            {
                return;
            }

            _uiFormLogic = GetComponent<UIFormLogic>();
            if (_uiFormLogic == null)
            {
                Debug.LogError($"UI form '{uiFormAssetName}' can not get UI form logic.");
                return;
            }

            try
            {
                _uiFormLogic.OnInit(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI form '[{_serialId}]{_uiFormAssetName}' OnInit with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                _uiFormLogic.OnRecycle();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI form '[{_serialId}]{_uiFormAssetName}' OnRecycle with exception '{exception}'.");
            }

            _serialId = 0;
            _depthInUIGroup = 0;
            _pauseCoveredUIForm = true;
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnOpen(object userData)
        {
            try
            {
                _uiFormLogic.OnOpen(userData);
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnOpen with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnClose(bool isShutdown, object userData)
        {
            try
            {
                _uiFormLogic.OnClose(isShutdown, userData);
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnClose with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        public void OnPause()
        {
            try
            {
                _uiFormLogic.OnPause();
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnPause with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        public void OnResume()
        {
            try
            {
                _uiFormLogic.OnResume();
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnResume with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        public void OnCover()
        {
            try
            {
                _uiFormLogic.OnCover();
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnCover with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        public void OnReveal()
        {
            try
            {
                _uiFormLogic.OnReveal();
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnReveal with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnRefocus(object userData)
        {
            try
            {
                _uiFormLogic.OnRefocus(userData);
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnRefocus with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        public void OnUpdate()
        {
            try
            {
                _uiFormLogic.OnUpdate();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI form '[{_serialId}]{_uiFormAssetName}' OnUpdate with exception '{exception}'.");
            }
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            _depthInUIGroup = depthInUIGroup;
            try
            {
                _uiFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception)
            {
                Debug.LogError("UI form '[{m_SerialId}]{m_UIFormAssetName}' OnDepthChanged with exception '{exception}'.");
            }
        }
    }
}