using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using MotionEngine;

namespace MotionGame
{
	public class ILRManager : IModule
	{
		public static readonly ILRManager Instance = new ILRManager();

		public ILRuntime.Runtime.Enviorment.AppDomain ILRDomain { private set; get; }
		private MemoryStream _dllStream;
		private MemoryStream _pdbStream;
		private Assembly _monoAssembly;

		// 热更新层相关函数
		private IStaticMethod _startFun;
		private IStaticMethod _updateFun;
		private IStaticMethod _lateUpdateFun;

		/// <summary>
		/// 热更新所有类型集合
		/// </summary>
		public List<Type> HotfixAssemblyTypes { private set; get; }

		/// <summary>
		/// 是否启用ILRuntime
		/// </summary>
		public bool EnableILRuntime { set; get; } = true;


		private ILRManager()
		{
		}
		public void Awake()
		{
		}
		public void Start()
		{
			LoadHotfixAssembly();
			InitHotfixProgram();

			_startFun.Invoke();
		}
		public void Update()
		{
			_updateFun.Invoke();
		}
		public void LateUpdate()
		{
			_lateUpdateFun.Invoke();
		}
		public void OnGUI()
		{
			Engine.GUILable($"EnableILRuntime : {EnableILRuntime}");
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void ReleaseILRuntime()
		{
			if (_dllStream != null)
			{
				_dllStream.Close();
				_dllStream = null;
			}
			if (_pdbStream != null)
			{
				_pdbStream.Close();
				_pdbStream = null;
			}
		}

		/// <summary>
		/// 加载热更的动态库文件
		/// </summary>
		private void LoadHotfixAssembly()
		{
			// TODO：正式发布时去掉PDB，下面LoadAssembly的PDB参数设置为null
			byte[] dllBytes = Resources.Load<TextAsset>($"Assembly/{ILRDefine.StrMyHotfixDLLFileName}").bytes;
			byte[] pdbBytes = Resources.Load<TextAsset>($"Assembly/{ILRDefine.StrMyHotfixPDBFileName}").bytes;

			if (EnableILRuntime)
			{
				LogSystem.Log(ELogType.Log, "ILRuntime模式");
				var symbolReader = new Mono.Cecil.Pdb.PdbReaderProvider();
				_dllStream = new MemoryStream(dllBytes);
				_pdbStream = new MemoryStream(pdbBytes);
				ILRDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
				ILRDomain.LoadAssembly(_dllStream, _pdbStream, symbolReader);
			}
			else
			{
				LogSystem.Log(ELogType.Log, "Mono模式");
				_monoAssembly = Assembly.Load(dllBytes, pdbBytes);
			}
		}

		/// <summary>
		/// 初始化热更程序
		/// </summary>
		private void InitHotfixProgram()
		{
			string typeName = "Hotfix.HotfixMain";
			string startFunName = "Start";
			string updateFunName = "Update";
			string lateUpdateFunName = "LateUpdate";

			if (EnableILRuntime)
			{
				ILRHelper.Init(ILRDomain);
				_startFun = new ILRStaticMethod(ILRDomain, typeName, startFunName, 0);
				_updateFun = new ILRStaticMethod(ILRDomain, typeName, updateFunName, 0);
				_lateUpdateFun = new ILRStaticMethod(ILRDomain, typeName, lateUpdateFunName, 0);
				HotfixAssemblyTypes = ILRDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
			}
			else
			{
				Type type = _monoAssembly.GetType(typeName);
				_startFun = new MonoStaticMethod(type, startFunName);
				_updateFun = new MonoStaticMethod(type, updateFunName);
				_lateUpdateFun = new MonoStaticMethod(type, lateUpdateFunName);
				HotfixAssemblyTypes = _monoAssembly.GetTypes().ToList<Type>();
			}
		}
	}
}