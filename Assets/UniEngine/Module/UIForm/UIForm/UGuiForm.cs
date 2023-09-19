using System.Collections.Generic;
using UniEngine.Module.UIForm.UIGroup;
using UnityEngine;

namespace UniEngine.Module.UIForm.UIForm
{
    public class UGuiForm : UIFormLogic
    {
        private Canvas _cachedCanvas;
        private CanvasGroup _canvasGroup;
        private readonly List<Canvas> _cachedCanvasList = new List<Canvas>();

        public int Depth => _cachedCanvas.sortingOrder;

        public void Close()
        {
            StopAllCoroutines();
            UIManager.Instance.CloseUIForm(UIForm);
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            _cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            _cachedCanvas.overrideSorting = true;

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        protected internal override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            _cachedCanvas.sortingOrder = uiGroupDepth * UIGroup.UIGroup.DepthFactor + depthInUIGroup;
        }
    }
}