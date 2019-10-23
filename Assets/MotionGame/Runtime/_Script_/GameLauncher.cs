using System;
using System.Collections.Generic;
using UnityEngine;
using MotionEngine;
using MotionEngine.Res;
using MotionGame;

public class GameLauncher : MonoBehaviour
{
	public static GameLauncher Instance = null;

	[Tooltip("运行时是否在屏幕上显示日志")]
	public bool ShowScreenLog = false;

	[Tooltip("是否启用脚本热更模式")]
	public bool EnableILRuntime = true;

	[Tooltip("资源系统的加载模式")]
	public EAssetLoadMode AssetLoadMode = EAssetLoadMode.ResourceMode;


	void Awake()
	{
		Instance = this;

		// 不销毁游戏对象
		DontDestroyOnLoad(gameObject);

		// 设置协程脚本
		Engine.Instance.InitCoroutineBehaviour(this);

		// 初始化日志
		InitLog();

		// 初始化应用
		InitAppliaction();
	}
	void Start()
	{
		RegisterAndRunAllGameModule();
	}
	void Update()
	{
		Engine.Instance.Update();
	}
	void LateUpdate()
	{
		Engine.Instance.LateUpdate();
	}
	void OnApplicationQuit()
	{
	}
	void OnApplicationFocus(bool focusStatus)
	{
	}
	void OnApplicationPause(bool pauseStatus)
	{
	}
	void OnGUI()
	{
		if (ShowScreenLog)
			DrawScreenLog();
	}

	private void InitLog()
	{
		// 加载背景纹理
		_bgTexture = Resources.Load<Texture>("buildinBackground");
		if (_bgTexture == null)
			UnityEngine.Debug.LogWarning("Not found buildinBackground texture.");

		// 注册MotionEngine日志系统
		LogSystem.RegisterCallback(MotionEngineLogCallback);

		// 注册UnityEngine日志系统
		Application.logMessageReceived += HandleUnityEngineLog;
	}
	private void InitAppliaction()
	{
		UnityEngine.Debug.Log($"Game run platform : {Application.platform}");
		UnityEngine.Debug.Log($"Version of the runtime : {Application.unityVersion}");

		Application.runInBackground = true;
		Application.backgroundLoadingPriority = ThreadPriority.High;

		// 设置最大帧数
		Application.targetFrameRate = 60;

		// 屏幕不休眠
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	/// <summary>
	/// 注册所有游戏模块
	/// </summary>
	private void RegisterAndRunAllGameModule()
	{
		// 设置资源系统加载模式
		AssetSystem.SetAssetLoadMode(AssetLoadMode);

		// 设置网络包解析器
		NetManager.Instance.SetPackageParseType(typeof(NetProtoPackageParser));

		// 设置ILRuntime开关
		ILRManager.Instance.EnableILRuntime = EnableILRuntime;

		// 注册所有游戏模块
		Engine.Instance.RegisterModule(EventManager.Instance);
		Engine.Instance.RegisterModule(ResManager.Instance);
		Engine.Instance.RegisterModule(CfgManager.Instance);
		Engine.Instance.RegisterModule(AudioManager.Instance);
		Engine.Instance.RegisterModule(NetManager.Instance);
		Engine.Instance.RegisterModule(ILRManager.Instance);
		Engine.Instance.RegisterModule(GameTest.Instance);
	}

	#region 日志系统
	private struct LogWrapper
	{
		public LogType Type;
		public string Log;
	}

	/// <summary>
	/// 日志最大显示数量
	/// </summary>
	private const int LOG_MAX_COUNT = 500;

	/// <summary>
	/// 显示开关
	/// </summary>
	private bool _visibleToggle = true;

	/// <summary>
	/// 背景纹理
	/// </summary>
	private Texture _bgTexture = null;

	/// <summary>
	/// GUI滑动条位置
	/// </summary>
	private Vector2 _scrollPos = new Vector2(0, 0);

	/// <summary>
	/// 日志集合
	/// </summary>
	private List<LogWrapper> _logs = new List<LogWrapper>();

	private void HandleUnityEngineLog(string logString, string stackTrace, LogType type)
	{
		LogWrapper wrapper = new LogWrapper();
		wrapper.Type = type;
		wrapper.Log = logString;

		_logs.Add(wrapper);
		if (_logs.Count > LOG_MAX_COUNT)
			_logs.RemoveAt(0);
	}
	private void MotionEngineLogCallback(ELogType logType, string log)
	{
		if (logType == ELogType.Log)
		{
			UnityEngine.Debug.Log(log);
		}
		else if (logType == ELogType.Error)
		{
			UnityEngine.Debug.LogError(log);
		}
		else if (logType == ELogType.Warning)
		{
			UnityEngine.Debug.LogWarning(log);
		}
		else if (logType == ELogType.Exception)
		{
			UnityEngine.Debug.LogError(log);
		}
		else
		{
			throw new NotImplementedException($"{logType}");
		}
	}
	private void DrawScreenLog()
	{
		// 显示开关
		if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(30)))
			_visibleToggle = !_visibleToggle;

		if (_visibleToggle == false)
			return;

		// 绘制背景纹理
		if (_bgTexture != null)
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTexture, ScaleMode.StretchToFill, true);

		_scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));

		// 显示游戏模块数据
		Engine.Instance.OnGUI();

		// 显示输出日志
		GUILayout.Space(20);
		for (int i = 0; i < _logs.Count; i++)
		{
			LogWrapper wrapper = _logs[i];
			if (wrapper.Type == LogType.Log)
				Engine.GUILable(wrapper.Log);
			else if (wrapper.Type == LogType.Warning)
				Engine.GUIYellowLable(wrapper.Log);
			else
				Engine.GUIRedLable(wrapper.Log);
		}

		GUILayout.EndScrollView();
	}
	#endregion
}