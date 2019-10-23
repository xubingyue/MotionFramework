//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionEngine.Event
{
	public class EventSystem
	{
		/// <summary>
		/// 监听集合
		/// </summary>
		private readonly Dictionary<string, List<Action<IEventMessage>>> _handlers = new Dictionary<string, List<Action<IEventMessage>>>();


		/// <summary>
		/// 清空所有监听
		/// </summary>
		public void ClearListeners()
		{
			foreach (string type in _handlers.Keys)
			{
				_handlers[type].Clear();
			}
			_handlers.Clear();
		}

		/// <summary>
		/// 注册监听
		/// </summary>
		public void AddListener(string eventTag, Action<IEventMessage> listener)
		{
			if (_handlers.ContainsKey(eventTag) == false)
				_handlers.Add(eventTag, new List<Action<IEventMessage>>());

			if (_handlers[eventTag].Contains(listener) == false)
				_handlers[eventTag].Add(listener);
		}

		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener(string eventTag, Action<IEventMessage> listener)
		{
			if (_handlers.ContainsKey(eventTag))
			{
				if (_handlers[eventTag].Contains(listener))
					_handlers[eventTag].Remove(listener);
			}
		}

		/// <summary>
		/// 广播事件
		/// </summary>
		/// <param name="eventTag">事件标签</param>
		/// <param name="msg">消息类</param>
		public void Broadcast(string eventTag, IEventMessage msg)
		{
			if (_handlers.ContainsKey(eventTag) == false)
			{
				LogSystem.Log(ELogType.Warning, $"Not found message eventTag : {eventTag}");
				return;
			}

			List<Action<IEventMessage>> listeners = _handlers[eventTag];
			for(int i=0; i< listeners.Count; i++)
			{
				listeners[i].Invoke(msg);
			}
		}

		/// <summary>
		/// 获取所有监听器的总数
		/// </summary>
		public int GetAllListenerCount()
		{
			int count = 0;
			foreach(var list in _handlers)
			{
				count += list.Value.Count;
			}
			return count;
		}
	}
}