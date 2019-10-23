using System;
using MotionEngine;
using MotionEngine.Res;

namespace MotionGame
{
	/// <summary>
	/// 资源管理器
	/// </summary>
	public sealed class ResManager : IModule
	{
		public static readonly ResManager Instance = new ResManager();


		private ResManager()
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
			AssetSystem.UpdatePoll();
		}
		public void LateUpdate()
		{
			AssetSystem.Release();
		}
		public void OnGUI()
		{
			int totalCount = 0;
			int failedCount = 0;

			if (AssetSystem.AssetLoadMode == EAssetLoadMode.ResourceMode)
			{
				totalCount = AssetSystem.GetResourceFileLoaderCount();
				failedCount = AssetSystem.GetResourceFileLoaderFailedCount();
			}
			else if (AssetSystem.AssetLoadMode == EAssetLoadMode.BundleMode)
			{
				totalCount = AssetSystem.GetBundleFileLoaderCount();
				failedCount = AssetSystem.GetBundleFileLoaderFailedCount();
			}
			else
			{
				throw new NotImplementedException($"{AssetSystem.AssetLoadMode}");
			}

			Engine.GUILable($"AssetLoadMode : {AssetSystem.AssetLoadMode}");
			Engine.GUILable($"Asset loader total count : {totalCount}");
			Engine.GUILable($"Asset loader failed count : {failedCount}");
		}
	}
}