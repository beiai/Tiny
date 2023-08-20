using System;
using System.Collections.Generic;
using UniEngine.Base.ReferencePool;
using UnityEngine;

namespace UniEngine.Module.Event
{
	public class EventGroup: IReference
	{
		private readonly Dictionary<Type, List<Action<BaseEvent>>> _cachedListener = new();

		/// <summary>
		/// 添加一个监听
		/// </summary>
		public void AddListener<TEvent>(Action<BaseEvent> listener) where TEvent : BaseEvent
		{
			var eventType = typeof(TEvent);
			if (_cachedListener.ContainsKey(eventType) == false)
				_cachedListener.Add(eventType, new List<Action<BaseEvent>>());

			if (_cachedListener[eventType].Contains(listener) == false)
			{
				_cachedListener[eventType].Add(listener);
				EventManager.Instance.AddListener(eventType, listener);
			}
			else
			{
				Debug.LogWarning($"Event listener is exist : {eventType}");
			}
		}

		/// <summary>
		/// 移除所有缓存的监听
		/// </summary>
		public void RemoveAllListener()
		{
			foreach (var (eventType, list) in _cachedListener)
			{
				foreach (var listener in list)
				{
					EventManager.Instance.RemoveListener(eventType, listener);
				}
				list.Clear();
			}
			ReferencePool.Release(this);
		}

		public void Clear()
		{
			_cachedListener.Clear();
		}
	}
}