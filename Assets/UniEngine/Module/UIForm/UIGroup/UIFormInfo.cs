using System;
using UniEngine.Base.ReferencePool;
using UniEngine.Module.UIForm.UIForm;

namespace UniEngine.Module.UIForm.UIGroup
{
    /// <summary>
    /// 界面组界面信息。
    /// </summary>
    public sealed class UIFormInfo : IReference
    {
        private IUIForm _uiForm;
        private bool _paused;
        private bool _covered;

        public UIFormInfo()
        {
            _uiForm = null;
            _paused = false;
            _covered = false;
        }

        public IUIForm UIForm => _uiForm;

        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        public bool Covered
        {
            get => _covered;
            set => _covered = value;
        }

        public static UIFormInfo Create(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            var uiFormInfo = ReferencePool.Acquire<UIFormInfo>();
            uiFormInfo._uiForm = uiForm;
            uiFormInfo._paused = true;
            uiFormInfo._covered = true;
            return uiFormInfo;
        }

        public void Clear()
        {
            _uiForm = null;
            _paused = false;
            _covered = false;
        }
    }
}