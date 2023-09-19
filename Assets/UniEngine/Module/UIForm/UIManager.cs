using System;
using System.Collections.Generic;
using UniEngine.Base.Module;
using UniEngine.Base.ReferencePool;
using UniEngine.Module.Resource;
using UniEngine.Module.UIForm.UIForm;
using UnityEngine;

namespace UniEngine.Module.UIForm
{
    /// <summary>
    /// 界面管理器。
    /// </summary>
    public sealed class UIManager : IModule
    {
        private readonly Dictionary<string, UIGroup.UIGroup> _uiGroups;
        private readonly Dictionary<int, string> _uiFormsBeingLoaded;
        private readonly HashSet<int> _uiFormsToReleaseOnLoad;
        private readonly Queue<IUIForm> _recycleQueue;
        private readonly LoadAssetCallbacks _loadAssetCallbacks;
        private IUIFormHelper _uiFormHelper;
        private int _serial;
        private bool _isShutdown;
        private static UIManager _instance;

        /// <summary>
        /// 初始化界面管理器的新实例。
        /// </summary>
        public UIManager()
        {
            _uiGroups = new Dictionary<string, UIGroup.UIGroup>(StringComparer.Ordinal);
            _uiFormsBeingLoaded = new Dictionary<int, string>();
            _uiFormsToReleaseOnLoad = new HashSet<int>();
            _recycleQueue = new Queue<IUIForm>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccess, LoadAssetFailure);
            _uiFormHelper = null;
            _serial = 0;
            _isShutdown = false;
        }

        /// <summary>
        /// 游戏框架模块轮询优先级
        /// </summary>
        public int Priority => 0;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<UIManager>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount => _uiGroups.Count;

        /// <summary>
        /// UI 根节点
        /// </summary>
        public GameObject Root { get; private set; }

        /// <summary>
        /// UI 摄像机
        /// </summary>
        public Camera Camera { get; private set; }

        public void OnCreate()
        {
            _instance = this;
            Root = GameObject.Find("UI/UIRoot").gameObject;
            Camera = GameObject.Find("UI/UICamera").GetComponent<Camera>();
            // CreateUIFormHelper<UIFormHelperDefault>();
            // AddUIGroup<UIGroupHelperDefault>("Default");
        }

        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        public void Update()
        {
            // while (_recycleQueue.Count > 0)
            // {
            //     var uiForm = _recycleQueue.Dequeue();
            //     uiForm.OnRecycle();
            //     _uiFormHelper.ReleaseUIForm(uiForm.Handle);
            // }

            foreach (var uiGroup in _uiGroups)
            {
                uiGroup.Value.InternalUpdate();
            }
        }

        /// <summary>
        /// 关闭并清理界面管理器。
        /// </summary>
        public void Shutdown()
        {
            _isShutdown = true;
            CloseAllLoadedUIForms();
            _uiGroups.Clear();
            _uiFormsBeingLoaded.Clear();
            _uiFormsToReleaseOnLoad.Clear();
            _recycleQueue.Clear();
        }

        // /// <summary>
        // /// 设置界面辅助器。
        // /// </summary>
        // public void CreateUIFormHelper<T>() where T : UIFormHelperBase
        // {
        //     var uiFormHelper = (T)new GameObject().AddComponent(typeof(T));
        //     if (uiFormHelper == null)
        //     {
        //         Debug.LogError("Can not create UI form helper.");
        //         return;
        //     }
        //
        //     uiFormHelper.name = "UI Form Helper";
        //     var transform = uiFormHelper.transform;
        //     transform.SetParent(Root.transform.parent);
        //     transform.localScale = Vector3.one;
        //
        //     _uiFormHelper = uiFormHelper;
        // }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            return _uiGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public UIGroup.UIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            if (_uiGroups.TryGetValue(uiGroupName, out var uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public UIGroup.UIGroup[] GetAllUIGroups()
        {
            var index = 0;
            var results = new UIGroup.UIGroup[_uiGroups.Count];
            foreach (var uiGroup in _uiGroups)
            {
                results[index++] = uiGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <param name="results">所有界面组。</param>
        public void GetAllUIGroups(List<UIGroup.UIGroup> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in _uiGroups)
            {
                results.Add(uiGroup.Value);
            }
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName)
        {
            return AddUIGroup(uiGroupName, 0);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            if (HasUIGroup(uiGroupName))
            {
                return false;
            }

            var uiGroup = new GameObject().AddComponent<UIGroup.UIGroup>();
            if (uiGroup == null)
            {
                return false;
            }
            var transform = uiGroup.transform;
            transform.SetParent(Root.transform);
            transform.localScale = Vector3.one;
            
            uiGroup.Initialize(uiGroupName, uiGroupDepth);
            _uiGroups.Add(uiGroupName, uiGroup);

            return true;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (var uiGroup in _uiGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (var uiGroup in _uiGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (var uiGroup in _uiGroups)
            {
                var uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (var uiGroup in _uiGroups)
            {
                var uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            var results = new List<IUIForm>();
            foreach (var uiGroup in _uiGroups)
            {
                results.AddRange(uiGroup.Value.GetUIForms(uiFormAssetName));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in _uiGroups)
            {
                uiGroup.Value.InternalGetUIForms(uiFormAssetName, results);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IUIForm[] GetAllLoadedUIForms()
        {
            var results = new List<IUIForm>();
            foreach (var uiGroup in _uiGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIForms());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<IUIForm> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in _uiGroups)
            {
                uiGroup.Value.InternalGetAllUIForms(results);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            var index = 0;
            var results = new int[_uiFormsBeingLoaded.Count];
            foreach (var uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return _uiFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            return _uiFormsBeingLoaded.ContainsValue(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, Constant.DefaultPriority, false, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, Constant.DefaultPriority, pauseCoveredUIForm, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, Constant.DefaultPriority, false, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, Constant.DefaultPriority, pauseCoveredUIForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIForm(int serialId)
        {
            CloseUIForm(serialId, null);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData)
        {
            if (IsLoadingUIForm(serialId))
            {
                _uiFormsToReleaseOnLoad.Add(serialId);
                _uiFormsBeingLoaded.Remove(serialId);
                return;
            }

            var uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                throw new Exception($"Can not find UI form '{serialId}'.");
            }

            CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIForm(IUIForm uiForm)
        {
            CloseUIForm(uiForm, null);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(IUIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            var uiGroup = (UIGroup.UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(_isShutdown, userData);
            uiGroup.Refresh();

            _recycleQueue.Enqueue(uiForm);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
        {
            CloseAllLoadedUIForms(null);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIForms(object userData)
        {
            var uiForms = GetAllLoadedUIForms();
            foreach (var uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (var uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                _uiFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            _uiFormsBeingLoaded.Clear();
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(IUIForm uiForm)
        {
            RefocusUIForm(uiForm, null);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            var uiGroup = (UIGroup.UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm, userData);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm,
            object userData)
        {
            if (_uiFormHelper == null)
            {
                throw new Exception("You must set UI form helper first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            var uiGroup = (UIGroup.UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new Exception($"UI group '{uiGroupName}' is not exist.");
            }

            var serialId = ++_serial;
            var uiFormInstanceObject = _uiFormHelper.GetUIFormAsset(uiFormAssetName);
            if (uiFormInstanceObject == null)
            {
                var openUIFormInfo = OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData);
                _uiFormsBeingLoaded.Add(serialId, uiFormAssetName);
                // _uiFormHelper.LoadUIFormAsset(uiFormAssetName, priority, openUIFormInfo, _loadAssetCallbacks);
            }
            else
            {
                // InternalOpenUIForm(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm,
                //     false, userData);
            }

            return serialId;
        }

        private void InternalOpenUIForm(int serialId, string uiFormAssetName, UIGroup.UIGroup uiGroup, object uiFormInstance,
            bool pauseCoveredUIForm, bool isNewInstance, object userData)
        {
            var uiForm = _uiFormHelper.CreateUIForm(uiFormInstance, uiGroup, userData);
            if (uiForm == null)
            {
                throw new Exception("Can not create UI form in UI form helper.");
            }

            uiForm.OnInit(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
            uiGroup.AddUIForm(uiForm);
            uiForm.OnOpen(userData);
            uiGroup.Refresh();
        }

        private void LoadAssetSuccess(string uiFormAssetName, object uiFormAsset, object userData)
        {
            var openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new Exception("Open UI form info is invalid.");
            }

            if (_uiFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uiFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                openUIFormInfo.AssetHandle.Release();
                ReferencePool.Release(openUIFormInfo);
                return;
            }

            _uiFormsBeingLoaded.Remove(openUIFormInfo.SerialId);

            var uiFormInstance = _uiFormHelper.AddUIFormAsset(uiFormAssetName, openUIFormInfo);
            InternalOpenUIForm(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup, uiFormInstance,
                openUIFormInfo.PauseCoveredUIForm, true, userData);

            ReferencePool.Release(openUIFormInfo);
        }

        private void LoadAssetFailure(string uiFormAssetName, string errorMessage, object userData)
        {
            var openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new Exception("Open UI form info is invalid.");
            }

            if (_uiFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uiFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                openUIFormInfo.AssetHandle.Release();
                ReferencePool.Release(openUIFormInfo);
                return;
            }

            _uiFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            
            var appendErrorMessage =
                $"Load UI form failure, asset name '{uiFormAssetName}', error message '{errorMessage}'.";

            throw new Exception(appendErrorMessage);
        }
    }
}