using System.Collections;
using System.Collections.Generic;
using MotionEngine;
using MotionEngine.Res;

namespace MotionGame
{
	/// <summary>
	/// 配表管理器
	/// </summary>
	public sealed class CfgManager : IModule
	{
		public static readonly CfgManager Instance = new CfgManager();

		/// <summary>
		/// 配表集合
		/// </summary>
		private Dictionary<string, AssetConfig> _cfgs = new Dictionary<string, AssetConfig>();

		/// <summary>
		/// Resources文件夹下配表文件夹的相对路径
		/// </summary>
		public string BaseFolderPath = "Config/";


		private CfgManager()
		{
		}
		public void Awake()
		{
		}
		public void Start()
		{
		}
		public void Update()
		{
		}
		public void LateUpdate()
		{
		}
		public void OnGUI()
		{
		}

		/// <summary>
		/// 加载配表
		/// </summary>
		/// <param name="cfgName">配表文件名称</param>
		public void Load(string cfgName, System.Action<Asset, EAssetResult> prepareCallback)
		{
			// 防止重复加载
			if (_cfgs.ContainsKey(cfgName))
			{
				LogSystem.Log(ELogType.Error, $"Config {cfgName} is already exist.");
				return;
			}

			AssetConfig config = ConfigHandler.Handle(cfgName);
			if (config != null)
			{
				_cfgs.Add(cfgName, config);
				config.Load(BaseFolderPath + cfgName, prepareCallback);
			}
			else
			{
				LogSystem.Log(ELogType.Error, $"Config {cfgName} calss is invalid.");
			}
		}

		/// <summary>
		/// 加载结果
		/// </summary>
		/// <param name="cfgName">配表文件名称</param>
		public EAssetResult Result(string cfgName)
		{
			if (_cfgs.ContainsKey(cfgName))
			{
				return _cfgs[cfgName].Result;
			}
			return EAssetResult.None;
		}

		/// <summary>
		/// 获取配表
		/// </summary>
		/// <param name="cfgName">配表文件名称</param>
		public AssetConfig GetConfig(string cfgName)
		{
			if (_cfgs.ContainsKey(cfgName))
			{
				return _cfgs[cfgName];
			}

			LogSystem.Log(ELogType.Error, $"Not found config {cfgName}");
			return null;
		}
	}
}