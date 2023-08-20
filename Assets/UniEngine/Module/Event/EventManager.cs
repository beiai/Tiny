using System;
using System.Collections.Generic;
using UniEngine.Base.Module;
using UniEngine.Base.ReferencePool;
using UnityEngine;

namespace UniEngine.Module.Event
{
    public class EventManager : IModule
    {
        private class EventInfo : IReference
        {
            public int PostFrame;
            public int EventID;
            public BaseEvent Message;

            public void Clear()
            {
                PostFrame = 0;
                EventID = 0;
                Message = null;
            }
        }

        private readonly Dictionary<int, LinkedList<Action<BaseEvent>>> _listeners = new(1000);
        private readonly List<EventInfo> _postingList = new(1000);
        private static EventManager _instance;

        public int Priority => 0;

        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ModuleManager.GetModule<EventManager>();
                }

                return _instance;
            }
        }

        public void OnCreate()
        {
            _instance = this;
        }

        /// <summary>
        /// 更新事件系统
        /// </summary>
        public void Update()
        {
            for (var i = _postingList.Count - 1; i >= 0; i--)
            {
                var eventInfo = _postingList[i];
                if (Time.frameCount > eventInfo.PostFrame)
                {
                    SendMessage(eventInfo.EventID, eventInfo.Message);
                    ReferencePool.Release(eventInfo);
                    _postingList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 清空所有监听
        /// </summary>
        public void Shutdown()
        {
            foreach (var eventId in _listeners.Keys)
            {
                _listeners[eventId].Clear();
            }

            _listeners.Clear();
            _postingList.Clear();
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener<TEvent>(Action<BaseEvent> listener) where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);
            var eventId = eventType.GetHashCode();
            AddListener(eventId, listener);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener(Type eventType, Action<BaseEvent> listener)
        {
            var eventId = eventType.GetHashCode();
            AddListener(eventId, listener);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener(int eventId, Action<BaseEvent> listener)
        {
            if (_listeners.ContainsKey(eventId) == false)
                _listeners.Add(eventId, new LinkedList<Action<BaseEvent>>());
            if (_listeners[eventId].Contains(listener) == false)
                _listeners[eventId].AddLast(listener);
        }


        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener<TEvent>(Action<BaseEvent> listener) where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);
            var eventId = eventType.GetHashCode();
            RemoveListener(eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(Type eventType, Action<BaseEvent> listener)
        {
            var eventId = eventType.GetHashCode();
            RemoveListener(eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(int eventId, Action<BaseEvent> listener)
        {
            if (_listeners.ContainsKey(eventId))
            {
                if (_listeners[eventId].Contains(listener))
                    _listeners[eventId].Remove(listener);
            }
        }


        /// <summary>
        /// 实时广播事件
        /// </summary>
        public void SendMessage(BaseEvent message)
        {
            var eventId = message.GetType().GetHashCode();
            SendMessage(eventId, message);
        }

        /// <summary>
        /// 实时广播事件
        /// </summary>
        public void SendMessage(int eventId, BaseEvent message)
        {
            if (_listeners.ContainsKey(eventId) == false)
            {
                ReferencePool.Release(message);
                return;
            }

            var listeners = _listeners[eventId];
            if (listeners.Count > 0)
            {
                var currentNode = listeners.Last;
                while (currentNode != null)
                {
                    currentNode.Value.Invoke(message);
                    currentNode = currentNode.Previous;
                }
            }
            ReferencePool.Release(message);
        }

        /// <summary>
        /// 延迟广播事件
        /// </summary>
        public void PostMessage(BaseEvent message)
        {
            var eventId = message.GetType().GetHashCode();
            PostMessage(eventId, message);
        }

        /// <summary>
        /// 延迟广播事件
        /// </summary>
        public void PostMessage(int eventId, BaseEvent message)
        {
            var eventInfo = ReferencePool.Acquire<EventInfo>();
            eventInfo.PostFrame = Time.frameCount;
            eventInfo.EventID = eventId;
            eventInfo.Message = message;
            _postingList.Add(eventInfo);
        }
    }
}