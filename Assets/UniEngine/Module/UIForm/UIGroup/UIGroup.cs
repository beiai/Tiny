using System;
using System.Collections.Generic;
using UniEngine.Base.Collections;
using UniEngine.Base.ReferencePool;
using UniEngine.Module.UIForm.UIForm;
using UnityEngine;
using UnityEngine.UI;

namespace UniEngine.Module.UIForm.UIGroup
{
    /// <summary>
    /// 界面组。
    /// </summary>
    public sealed class UIGroup : MonoBehaviour
    {
        #region MonoBehaviour

        public const int DepthFactor = 100;
        private Canvas _cachedCanvas;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            _cachedCanvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
        }
        
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        private void SetDepth(int depth)
        {
            _depth = depth;
            _cachedCanvas.overrideSorting = true;
            _cachedCanvas.sortingOrder = DepthFactor * depth;
        }

        #endregion
        
        private string _groupName;
        private int _depth;
        private bool _pause;
        private readonly LinkedListCached<UIFormInfo> _uiFormInfos = new();
        private LinkedListNode<UIFormInfo> _cachedNode;

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string GroupName
        {
            get => _groupName;
            set
            {
                if (_groupName == value)
                {
                    return;
                }

                _groupName = value;
                gameObject.name = $"UI Group - {_groupName}";
            }
        }

        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        public int Depth
        {
            get => _depth;
            set
            {
                if (_depth == value)
                {
                    return;
                }

                _depth = value;
                SetDepth(_depth);
                Refresh();
            }
        }

        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        public bool Pause
        {
            get => _pause;
            set
            {
                if (_pause == value)
                {
                    return;
                }

                _pause = value;
                Refresh();
            }
        }

        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        public int UIFormCount => _uiFormInfos.Count;

        /// <summary>
        /// 获取当前界面。
        /// </summary>
        public IUIForm CurrentUIForm => _uiFormInfos.First?.Value.UIForm;
        
        /// <summary>
        /// 初始化界面组的新实例。
        /// </summary>
        /// <param name="groupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        public void Initialize(string groupName, int depth)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new Exception("UI group groupName is invalid.");
            }

            GroupName = groupName;
            Depth = depth;
        }

        /// <summary>
        /// 界面组轮询。
        /// </summary>
        public void InternalUpdate()
        {
            var current = _uiFormInfos.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }

                _cachedNode = current.Next;
                current.Value.UIForm.OnUpdate();
                current = _cachedNode;
                _cachedNode = null;
            }
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset groupName is invalid.");
            }

            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset groupName is invalid.");
            }

            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset groupName is invalid.");
            }

            var results = new List<IUIForm>();
            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset groupName is invalid.");
            }

            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }
        }

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        public IUIForm[] GetAllUIForms()
        {
            var results = new List<IUIForm>();
            foreach (var uiFormInfo in _uiFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <param name="results">界面组中的所有界面。</param>
        public void GetAllUIForms(List<IUIForm> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormInfo in _uiFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }
        }

        /// <summary>
        /// 往界面组增加界面。
        /// </summary>
        /// <param name="uiForm">要增加的界面。</param>
        public void AddUIForm(IUIForm uiForm)
        {
            _uiFormInfos.AddFirst(UIFormInfo.Create(uiForm));
        }

        /// <summary>
        /// 从界面组移除界面。
        /// </summary>
        /// <param name="uiForm">要移除的界面。</param>
        public void RemoveUIForm(IUIForm uiForm)
        {
            var uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                throw new Exception(
                    $"Can not find UI form info for serial id '{uiForm.SerialId}', UI form asset groupName is '{uiForm.UIFormAssetName}'.");
            }

            if (!uiFormInfo.Covered)
            {
                uiFormInfo.Covered = true;
                uiForm.OnCover();
            }

            if (!uiFormInfo.Paused)
            {
                uiFormInfo.Paused = true;
                uiForm.OnPause();
            }

            // 移除的界面从 Update 队列中移除
            if (_cachedNode != null && _cachedNode.Value.UIForm == uiForm)
            {
                _cachedNode = _cachedNode.Next;
            }

            if (!_uiFormInfos.Remove(uiFormInfo))
            {
                throw new Exception(
                    $"UI group '{_groupName}' not exists specified UI form '[{uiForm.SerialId}]{uiForm.UIFormAssetName}'.");
            }

            ReferencePool.Release(uiFormInfo);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIForm uiForm, object userData)
        {
            var uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                throw new Exception("Can not find UI form info.");
            }

            _uiFormInfos.Remove(uiFormInfo);
            _uiFormInfos.AddFirst(uiFormInfo);
        }

        /// <summary>
        /// 刷新界面组。
        /// </summary>
        public void Refresh()
        {
            var current = _uiFormInfos.First;
            var pause = _pause;
            var cover = false;
            var depth = UIFormCount;
            while (current != null && current.Value != null)
            {
                var next = current.Next;
                current.Value.UIForm.OnDepthChanged(Depth, depth--);
                if (current.Value == null)
                {
                    return;
                }

                if (pause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.UIForm.OnCover();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        current.Value.UIForm.OnPause();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        current.Value.UIForm.OnResume();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (current.Value.UIForm.PauseCoveredUIForm)
                    {
                        pause = true;
                    }

                    if (cover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIForm.OnCover();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            current.Value.UIForm.OnReveal();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }

                        cover = true;
                    }
                }

                current = next;
            }
        }

        internal void InternalGetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }
        }

        internal void InternalGetAllUIForms(List<IUIForm> results)
        {
            foreach (var uiFormInfo in _uiFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }
        }

        private UIFormInfo GetUIFormInfo(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            foreach (var uiFormInfo in _uiFormInfos)
            {
                if (uiFormInfo.UIForm == uiForm)
                {
                    return uiFormInfo;
                }
            }

            return null;
        }
    }
}