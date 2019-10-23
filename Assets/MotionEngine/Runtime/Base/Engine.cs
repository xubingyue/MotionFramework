//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionEngine
{
	public class Engine
	{
		/// <summary>
		/// 模块封装类
		/// </summary>
		private class ModuleWrapper
		{
			internal bool IsStart = false;
			public int Priority { get; private set; }
			public IModule Module { get; private set; }

			public ModuleWrapper(IModule module, int priority)
			{
				Module = module;
				Priority = priority;
			}

			public void SetPriority(int priority)
			{
				Priority = priority;
			}
		}


		public static readonly Engine Instance = new Engine();

		/// <summary>
		/// 模块集合
		/// </summary>
		private readonly List<ModuleWrapper> _coms = new List<ModuleWrapper>(100);

		/// <summary>
		/// 临时集合
		/// </summary>
		private readonly List<ModuleWrapper> _temps = new List<ModuleWrapper>(100);

		/// <summary>
		/// 协程脚本
		/// </summary>
		private MonoBehaviour _bhvCoroutine;


		private Engine()
		{
		}

		/// <summary>
		/// 注册模块
		/// </summary>
		/// <param name="module">要注册的模块</param>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public void RegisterModule(IModule module, int priority = 0)
		{
			if (module == null)
				throw new ArgumentNullException();

			ModuleWrapper wrapper = new ModuleWrapper(module, priority);		
			_temps.Add(wrapper);

			// 执行Awake
			module.Awake();
		}

		/// <summary>
		/// 该方法需要外部主动调用执行
		/// </summary>
		public void Update()
		{
			// 如果有新模块需要加入
			if (_temps.Count > 0)
			{
				for (int i = 0; i < _temps.Count; i++)
				{
					ModuleWrapper wrapper = _temps[i];

					// 如果没有设置优先级
					if (wrapper.Priority == 0)
					{
						int minPriority = GetMinPriority();
						wrapper.SetPriority(--minPriority);
					}

					_coms.Add(wrapper);
				}

				// 清空临时列表
				_temps.Clear();

				// 最后重新排序
				_coms.Sort((left, right) =>
				{
					if (left.Priority > right.Priority)
						return -1;
					else if (left.Priority == right.Priority)
						return 0;
					else
						return 1;
				});
			}

			// 更新所有模块
			for (int i = 0; i < _coms.Count; i++)
			{
				ModuleWrapper wrapper = _coms[i];
				if (wrapper.IsStart == false)
				{
					LogSystem.Log(ELogType.Log, $"{wrapper.Module.GetType()} is start.");
					wrapper.IsStart = true;
					wrapper.Module.Start();
				}
				wrapper.Module.Update();
			}
		}

		/// <summary>
		/// 该方法需要外部主动调用执行
		/// </summary>
		public void LateUpdate()
		{
			for (int i = 0; i < _coms.Count; i++)
			{
				_coms[i].Module.LateUpdate();
			}
		}

		/// <summary>
		/// 该方法需要外部主动调用执行
		/// </summary>
		public void OnGUI()
		{
			for (int i = 0; i < _coms.Count; i++)
			{
				_coms[i].Module.OnGUI();
			}
		}

		/// <summary>
		/// 获取当前模块里最小的优先级
		/// </summary>
		private int GetMinPriority()
		{
			int minPriority = 0;
			for (int i = 0; i < _coms.Count; i++)
			{
				if (_coms[i].Priority < minPriority)
					minPriority = _coms[i].Priority;
			}
			return minPriority; //小于等于零
		}

		#region 协程相关
		/// <summary>
		/// 初始化协程脚本
		/// </summary>
		public void InitCoroutineBehaviour(MonoBehaviour bhvCoroutine)
		{
			_bhvCoroutine = bhvCoroutine;
		}

		/// <summary>
		/// 开启一个协程
		/// </summary>
		public Coroutine StartCoroutine(IEnumerator coroutine)
		{
			if (_bhvCoroutine == null)
				throw new Exception("Coroutine mono behaviour is null.");
			return _bhvCoroutine.StartCoroutine(coroutine);
		}

		/// <summary>
		/// 停止一个协程
		/// </summary>
		/// <param name="coroutine"></param>
		public void StopCoroutine(Coroutine coroutine)
		{
			if (_bhvCoroutine == null)
				throw new Exception("Coroutine mono behaviour is null.");
			_bhvCoroutine.StopCoroutine(coroutine);
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public void StopAllCoroutines()
		{
			if (_bhvCoroutine == null)
				throw new Exception("Coroutine mono behaviour is null.");
			_bhvCoroutine.StopAllCoroutines();
		}
		#endregion

		#region GUI辅助方法
		public static void GUILable(string text)
		{
			GUIStyle style = GUIStyle.none;
			style.richText = true;
			GUILayout.Label("<size=18><color=white>" + text + "</color></size>", style);
		}
		public static void GUIRedLable(string text)
		{
			GUIStyle style = GUIStyle.none;
			style.richText = true;
			GUILayout.Label("<size=18><color=red>" + text + "</color></size>", style);
		}
		public static void GUIYellowLable(string text)
		{
			GUIStyle style = GUIStyle.none;
			style.richText = true;
			GUILayout.Label("<size=18><color=yellow>" + text + "</color></size>", style);
		}
		#endregion
	}
}