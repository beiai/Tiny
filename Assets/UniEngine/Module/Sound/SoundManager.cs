using System;
using System.Collections.Generic;
using UniEngine.Base.Module;
using UniEngine.Base.ReferencePool;
using UniEngine.Module.Resource;
using UniEngine.Module.Sound.SoundAgent;
using UniEngine.Module.Sound.SoundGroup;
using UniEngine.Module.Sound.SoundHelper;
using UnityEngine;

namespace UniEngine.Module.Sound
{
    /// <summary>
    /// 声音管理器。
    /// </summary>
    public sealed class SoundManager : IModule, ISoundManager
    {
        private readonly Dictionary<string, SoundGroup.SoundGroup> _soundGroups;
        private readonly List<int> _soundsBeingLoaded;
        private readonly HashSet<int> _soundsToReleaseOnLoad;
        private readonly LoadAssetCallbacks _loadAssetCallbacks;
        private ISoundHelper _soundHelper;
        private int _serial;
        private static SoundManager _instance;

        public int Priority => 0;
        
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<SoundManager>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount => _soundGroups.Count;

        /// <summary>
        /// Sound 根节点
        /// </summary>
        public GameObject Root { get; private set; }

        /// <summary>
        /// 初始化声音管理器的新实例。
        /// </summary>
        public SoundManager()
        {
            _soundGroups = new Dictionary<string, SoundGroup.SoundGroup>(StringComparer.Ordinal);
            _soundsBeingLoaded = new List<int>();
            _soundsToReleaseOnLoad = new HashSet<int>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccess, LoadAssetFailure);
            _soundHelper = null;
            _serial = 0;
        }

        public void OnCreate()
        {
            _instance = this;
            Root = GameObject.Find("Launcher/Sound").gameObject;
            CreateSoundHelper<SoundHelperDefault>();
            AddSoundGroup<SoundGroupHelperDefault>("Music");
            for (var i = 0; i < 2; i++)
            {
                AddSoundAgentHelper<SoundAgentHelperDefault>("Music");
            }

            AddSoundGroup<SoundGroupHelperDefault>("Effect");
            for (var i = 0; i < 2; i++)
            {
                AddSoundAgentHelper<SoundAgentHelperDefault>("Effect");
            }

            AddSoundGroup<SoundGroupHelperDefault>("Voice");
            for (var i = 0; i < 2; i++)
            {
                AddSoundAgentHelper<SoundAgentHelperDefault>("Voice");
            }

            AddSoundGroup<SoundGroupHelperDefault>("UI");
            for (var i = 0; i < 4; i++)
            {
                AddSoundAgentHelper<SoundAgentHelperDefault>("UI");
            }
        }

        /// <summary>
        /// 声音管理器轮询。
        /// </summary>
        public void Update()
        {
        }

        /// <summary>
        /// 关闭并清理声音管理器。
        /// </summary>
        public void Shutdown()
        {
            StopAllLoadedSounds();
            _soundGroups.Clear();
            _soundsBeingLoaded.Clear();
            _soundsToReleaseOnLoad.Clear();
        }

        /// <summary>
        /// 设置声音辅助器。
        /// </summary>
        public void CreateSoundHelper<T>() where T : SoundHelperBase
        {
            var soundHelper = (T)new GameObject().AddComponent(typeof(T));
            if (soundHelper == null)
            {
                Debug.LogError("Can not create sound helper.");
                return;
            }

            soundHelper.name = "Sound Helper";
            soundHelper.transform.SetParent(Root.transform);
            soundHelper.transform.localScale = Vector3.one;
            _soundHelper = soundHelper;
        }

        /// <summary>
        /// 创建界面组辅助器
        /// </summary>
        public T CreateSoundGroupHelper<T>(string soundGroupName) where T : SoundGroupHelperBase
        {
            if (HasSoundGroup(soundGroupName))
            {
                return null;
            }

            var soundGroupHelper = (T)new GameObject().AddComponent(typeof(T));
            if (soundGroupHelper == null)
            {
                Debug.LogError("Can not create sound group helper.");
                return null;
            }

            soundGroupHelper.name = $"Sound Group - {soundGroupName}";
            soundGroupHelper.transform.SetParent(Root.transform);
            soundGroupHelper.transform.localScale = Vector3.one;
            return soundGroupHelper;
        }

        /// <summary>
        /// 创建声音代理辅助器
        /// </summary>
        /// <param name="soundGroupName">声音组名称</param>
        /// <param name="soundGroupHelper">声音组辅助器</param>
        /// <param name="index">声音代理编号</param>
        public T CreateSoundAgentHelper<T>(string soundGroupName, SoundGroupHelperBase soundGroupHelper, int index)
            where T : SoundAgentHelperBase
        {
            var soundAgentHelper = (T)new GameObject().AddComponent(typeof(T));
            if (soundAgentHelper == null)
            {
                Debug.LogError("Can not create sound agent helper.");
                return null;
            }

            soundAgentHelper.name = $"Sound Agent Helper - {soundGroupName} - {index}";
            soundAgentHelper.transform.SetParent(soundGroupHelper.transform);
            soundAgentHelper.transform.localScale = Vector3.one;
            return soundAgentHelper;
        }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new Exception("Sound group name is invalid.");
            }

            return _soundGroups.ContainsKey(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new Exception("Sound group name is invalid.");
            }

            if (_soundGroups.TryGetValue(soundGroupName, out var soundGroup))
            {
                return soundGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public ISoundGroup[] GetAllSoundGroups()
        {
            var index = 0;
            var results = new ISoundGroup[_soundGroups.Count];
            foreach (var soundGroup in _soundGroups)
            {
                results[index++] = soundGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (var soundGroup in _soundGroups)
            {
                results.Add(soundGroup.Value);
            }
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup<T>(string soundGroupName) where T : SoundGroupHelperBase
        {
            return AddSoundGroup<T>(soundGroupName, false, Constant.DefaultMute, Constant.DefaultVolume);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup<T>(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority,
            bool soundGroupMute, float soundGroupVolume) where T : SoundGroupHelperBase
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new Exception("Sound group name is invalid.");
            }

            if (HasSoundGroup(soundGroupName))
            {
                return false;
            }

            var soundGroupHelper = CreateSoundGroupHelper<T>(soundGroupName);
            if (soundGroupHelper == null)
            {
                return false;
            }

            var soundGroup = new SoundGroup.SoundGroup(soundGroupName, soundGroupHelper)
            {
                AvoidBeingReplacedBySamePriority = soundGroupAvoidBeingReplacedBySamePriority,
                Mute = soundGroupMute,
                Volume = soundGroupVolume
            };

            _soundGroups.Add(soundGroupName, soundGroup);

            return true;
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        public void AddSoundAgentHelper<T>(string soundGroupName) where T : SoundAgentHelperBase
        {
            if (_soundHelper == null)
            {
                throw new Exception("You must set sound helper first.");
            }

            var soundGroup = (SoundGroup.SoundGroup)GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                throw new Exception($"Sound group '{soundGroupName}' is not exist.");
            }

            var soundAgentHelper = CreateSoundAgentHelper<T>(soundGroupName, (SoundGroupHelperBase)soundGroup.Helper,
                soundGroup.SoundAgentCount);

            soundGroup.AddSoundAgentHelper(_soundHelper, soundAgentHelper);
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return _soundsBeingLoaded.ToArray();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_soundsBeingLoaded);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return _soundsBeingLoaded.Contains(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName)
        {
            return PlaySound(soundAssetName, soundGroupName, 0, null, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, 0, playSoundParams, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, 0, null, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams,
            object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, 0, playSoundParams, userData);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return StopSound(serialId, Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingSound(serialId))
            {
                _soundsToReleaseOnLoad.Add(serialId);
                _soundsBeingLoaded.Remove(serialId);
                return true;
            }

            foreach (var soundGroup in _soundGroups)
            {
                if (soundGroup.Value.StopSound(serialId, fadeOutSeconds))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            StopAllLoadedSounds(Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (var soundGroup in _soundGroups)
            {
                soundGroup.Value.StopAllLoadedSounds(fadeOutSeconds);
            }
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            foreach (var serialId in _soundsBeingLoaded)
            {
                _soundsToReleaseOnLoad.Add(serialId);
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            PauseSound(serialId, Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (var soundGroup in _soundGroups)
            {
                if (soundGroup.Value.PauseSound(serialId, fadeOutSeconds))
                {
                    return;
                }
            }

            throw new Exception($"Can not find sound '{serialId}'.");
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            ResumeSound(serialId, Constant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (var soundGroup in _soundGroups)
            {
                if (soundGroup.Value.ResumeSound(serialId, fadeInSeconds))
                {
                    return;
                }
            }

            throw new Exception($"Can not find sound '{serialId}'.");
        }
        
        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, object userData)
        {
            if (_soundHelper == null)
            {
                throw new Exception("You must set sound helper first.");
            }

            if (playSoundParams == null)
            {
                playSoundParams = PlaySoundParams.Create();
            }

            var serialId = ++_serial;
            PlaySoundErrorCode? errorCode = null;
            string errorMessage = null;
            var soundGroup = (SoundGroup.SoundGroup)GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                errorCode = PlaySoundErrorCode.SoundGroupNotExist;
                errorMessage = $"Sound group '{soundGroupName}' is not exist.";
            }
            else if (soundGroup.SoundAgentCount <= 0)
            {
                errorCode = PlaySoundErrorCode.SoundGroupHasNoAgent;
                errorMessage = $"Sound group '{soundGroupName}' is have no sound agent.";
            }

            if (errorCode.HasValue)
            {
                if (playSoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundParams);
                }

                throw new Exception(errorMessage);
            }

            _soundsBeingLoaded.Add(serialId);

            var playSoundInfo = PlaySoundInfo.Create(serialId, soundGroup, playSoundParams, userData);
            _soundHelper.LoadAudioAsset(soundAssetName, priority, playSoundInfo, _loadAssetCallbacks);
            return serialId;
        }

        private void LoadAssetSuccess(string soundAssetName, object soundAsset, object userData)
        {
            var playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new Exception("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                playSoundInfo.AssetHandle.Release();
                ReferencePool.Release(playSoundInfo);
                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);

            var soundAgent = playSoundInfo.SoundGroup.PlaySound(playSoundInfo.SerialId, soundAsset,
                playSoundInfo.PlaySoundParams, out var errorCode);
            if (soundAgent != null)
            {
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                playSoundInfo.AssetHandle.Release();
                ReferencePool.Release(playSoundInfo);
                return;
            }

            _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
            var errorMessage =
                $"Sound group '{playSoundInfo.SoundGroup.Name}' play sound '{soundAssetName}' failure.";

            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferencePool.Release(playSoundInfo.PlaySoundParams);
            }

            playSoundInfo.AssetHandle.Release();
            ReferencePool.Release(playSoundInfo);
            throw new Exception(errorMessage);
        }

        private void LoadAssetFailure(string soundAssetName, string errorMessage, object userData)
        {
            var playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new Exception("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                playSoundInfo.AssetHandle.Release();
                ReferencePool.Release(playSoundInfo);
                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);
            var appendErrorMessage =
                $"Load sound failure, asset name '{soundAssetName}', error message '{errorMessage}'.";

            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferencePool.Release(playSoundInfo.PlaySoundParams);
            }

            playSoundInfo.AssetHandle.Release();
            ReferencePool.Release(playSoundInfo);
            throw new Exception(appendErrorMessage);
        }
    }
}