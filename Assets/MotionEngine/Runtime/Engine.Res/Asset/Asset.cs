//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Res
{
	public abstract class Asset
	{
		/// <summary>
		/// 资源名称
		/// 注意：相对于Resources文件夹的相对路径
		/// </summary>
		public string ResName { private set; get; }

		/// <summary>
		/// 最终结果
		/// </summary>
		public EAssetResult Result { private set; get; }

		/// <summary>
		/// 资源类型
		/// </summary>
		public EAssetType AssetType { private set; get; }

		/// <summary>
		/// 准备完毕回调
		/// </summary>
		private System.Action<Asset, EAssetResult> _prepareCallback;

		/// <summary>
		/// 缓存的加载器
		/// </summary>
		private AssetFileLoader _cacheLoader;


		public Asset(EAssetType assetType)
		{
			AssetType = assetType;
		}

		/// <summary>
		/// 异步加载
		/// </summary>
		public void Load(string resName, System.Action<Asset, EAssetResult> prepareCallbcak)
		{
			// 防止重复加载
			if (Result != EAssetResult.None)
			{
				LogSystem.Log(ELogType.Warning, $"Asset {ResName} is already load.");
				return;
			}

			if (_cacheLoader != null)
			{
				LogSystem.Log(ELogType.Warning, $"Asset  {ResName}  loader must null.");
				return;
			}

			ResName = resName;
			Result = EAssetResult.Loading;
			_prepareCallback = prepareCallbcak;
			_cacheLoader = AssetSystem.LoadAssetFile(ResName, AssetType, OnAssetFileLoad);
		}

		/// <summary>
		/// 卸载
		/// </summary>
		public virtual void UnLoad()
		{
			Result = EAssetResult.None;

			if (_cacheLoader != null)
			{
				_cacheLoader.Release();
				_cacheLoader = null;
			}
		}

		/// <summary>
		/// 是否加载完毕（无论成功失败）
		/// </summary>
		public bool IsLoadDone()
		{
			return Result == EAssetResult.Failed || Result == EAssetResult.OK;
		}

		private void OnAssetFileLoad(AssetFileLoader loader)
		{
			// 注意 : 如果在加载过程中调用UnLoad，等资源文件加载完毕时不再执行后续准备工作。
			if (Result != EAssetResult.Loading)
				return;

			if (loader.LoadState == EAssetFileLoadState.LoadAssetFileOK)
			{
				loader.LoadMainAsset(AssetType, OnAssetObjectLoad);
			}
			else
			{
				Result = EAssetResult.Failed;

				// 回调接口
				if (_prepareCallback != null)
					_prepareCallback.Invoke(this, Result);
			}
		}
		private void OnAssetObjectLoad(UnityEngine.Object asset, bool result)
		{
			// 注意 : 如果在加载过程中调用UnLoad，等资源对象加载完毕时不再执行后续准备工作。
			if (Result != EAssetResult.Loading)
				return;

			// 准备数据
			if (OnPrepare(asset, result))
			{
				Result = EAssetResult.OK;
			}
			else
			{
				Result = EAssetResult.Failed;
				LogSystem.Log(ELogType.Warning, $"Failed to prepare asset : {ResName}");
			}

			// 回调接口
			if (_prepareCallback != null)
				_prepareCallback.Invoke(this, Result);
		}
		protected virtual bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			return result;
		}
	}
}