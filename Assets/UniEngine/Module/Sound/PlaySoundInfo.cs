using UniEngine.Base.ReferencePool;
using YooAsset;

namespace UniEngine.Module.Sound
{
    public sealed class PlaySoundInfo : IReference
    {
        private int _serialId;
        private SoundGroup.SoundGroup _soundGroup;
        private PlaySoundParams _playSoundParams;
        private object _userData;

        public PlaySoundInfo()
        {
            _serialId = 0;
            _soundGroup = null;
            _playSoundParams = null;
            _userData = null;
        }

        public int SerialId => _serialId;

        public SoundGroup.SoundGroup SoundGroup => _soundGroup;

        public PlaySoundParams PlaySoundParams => _playSoundParams;

        public object UserData => _userData;
        
        public AssetOperationHandle AssetHandle { get; set; }

        public static PlaySoundInfo Create(int serialId, SoundGroup.SoundGroup soundGroup, PlaySoundParams playSoundParams,
            object userData)
        {
            var playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
            playSoundInfo._serialId = serialId;
            playSoundInfo._soundGroup = soundGroup;
            playSoundInfo._playSoundParams = playSoundParams;
            playSoundInfo._userData = userData;
            return playSoundInfo;
        }

        public void Clear()
        {
            _serialId = 0;
            _soundGroup = null;
            _playSoundParams = null;
            _userData = null;
        }
    }
}