using UniEngine.Base.ReferencePool;

namespace UniEngine.Module.Event
{
    public abstract class BaseEvent : IReference
    {
        public BaseEvent()
        {
        }

        public abstract void Clear();
    }
}