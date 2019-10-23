//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionEngine.Res
{
	/// <summary>
	/// Resources接口加载器
	/// </summary>
	public class AssetResourceLoader : AssetFileLoader
	{
		/// <summary>
		/// 主资源对象
		/// </summary>
		private UnityEngine.Object _mainAsset;

		/// <summary>
		/// Request对象
		/// </summary>
		private ResourceRequest _cacheRequest;


		public AssetResourceLoader(EAssetType assetType, string loadPath)
			: base(assetType, loadPath)
		{
		}
		public override void Update()
		{
			if (IsDone())
				return;

			if (LoadState == EAssetFileLoadState.None)
			{
				LoadState = EAssetFileLoadState.LoadAssetFile;
			}

			// 1. 加载主资源对象
			if (LoadState == EAssetFileLoadState.LoadAssetFile)
			{
				// Load resource folder file		
				System.Type systemType = AssetSystem.MakeSystemType(AssetType);
				if (systemType == null)
					_cacheRequest = Resources.LoadAsync(LoadPath);
				else
					_cacheRequest = Resources.LoadAsync(LoadPath, systemType);

				LoadState = EAssetFileLoadState.CheckAssetFile;
			}

			// 2. 检测AssetObject加载结果
			if (LoadState == EAssetFileLoadState.CheckAssetFile)
			{
				if (_cacheRequest.isDone == false)
					return;
				_mainAsset = _cacheRequest.asset;

				// Check scene
				if (AssetType == EAssetType.Scene)
				{
					LoadState = EAssetFileLoadState.LoadAssetFileOK;
					LoadCallback?.Invoke(this);
					return;
				}

				// Check error
				if (_mainAsset == null)
				{
					LogSystem.Log(ELogType.Warning, $"Failed to load resource file : {LoadPath}");
					LoadState = EAssetFileLoadState.LoadAssetFileFailed;
					LoadCallback?.Invoke(this);
				}
				else
				{
					LoadState = EAssetFileLoadState.LoadAssetFileOK;
					LoadCallback?.Invoke(this);
				}
			}
		}

		public override void LoadMainAsset(EAssetType mainAssetType, OnAssetObjectLoad callback)
		{
			// Check error
			if (LoadState != EAssetFileLoadState.LoadAssetFileOK)
			{
				LogSystem.Log(ELogType.Error, $"Can not load asset object, {nameof(AssetResourceLoader)} is not ok : {LoadPath}");
				callback?.Invoke(null, false);
				return;
			}

			callback?.Invoke(_mainAsset, LoadState == EAssetFileLoadState.LoadAssetFileOK);
		}
	}
}