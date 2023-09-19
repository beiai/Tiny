using UnityEngine;
using UnityEngine.Audio;

namespace UniEngine.Module.Sound.SoundGroup
{
    /// <summary>
    /// 声音组辅助器基类。
    /// </summary>
    public abstract class SoundGroupHelperBase : MonoBehaviour, ISoundGroupHelper
    {
        [SerializeField]
        private AudioMixerGroup audioMixerGroup;

        /// <summary>
        /// 获取或设置声音组辅助器所在的混音组。
        /// </summary>
        public AudioMixerGroup AudioMixerGroup
        {
            get => audioMixerGroup;
            set => audioMixerGroup = value;
        }
    }
}
