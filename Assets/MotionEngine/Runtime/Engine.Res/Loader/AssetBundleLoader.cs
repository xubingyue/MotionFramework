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
	/// AssetBundle接口加载器
	/// </summary>
	public class AssetBundleLoader : AssetFileLoader
	{
		/// <summary>
		/// 依赖加载器列表
		/// </summary>
		private readonly List<AssetFileLoader> _depends = new List<AssetFileLoader>(10);

		/// <summary>
		/// Manifest路径
		/// </summary>
		private string _manifestPath = string.Empty;

		/// <summary>
		/// AssetBundle对象
		/// </summary>
		private AssetBundle _cacheBundle;

		/// <summary>
		/// Request对象
		/// </summary>
		private AssetBundleCreateRequest _cacheRequest;


		public AssetBundleLoader(EAssetType assetType, string loadPath, string manifestPath)
			: base(assetType, loadPath)
		{
			_manifestPath = manifestPath;
		}
		public override void Update()
		{
			// 轮询更新主资源对象加载
			if (LoadState == EAssetFileLoadState.LoadAssetFileOK)
			{
				if (_mainAssetLoader != null)
					_mainAssetLoader.Update();
			}

			if (IsDone())
				return;

			if (LoadState == EAssetFileLoadState.None)
			{
				LoadState = EAssetFileLoadState.LoadDepends;
			}

			// 1. 加载所有依赖项
			if (LoadState == EAssetFileLoadState.LoadDepends)
			{
				string[] dependencies = AssetSystem.BundleMethod.GetDirectDependencies(_manifestPath);
				if (dependencies.Length > 0)
				{
					foreach (string dpManifestPath in dependencies)
					{
						string dpLoadPath = AssetSystem.BundleMethod.GetAssetBundleLoadPath(dpManifestPath);
						AssetFileLoader dpLoader = AssetSystem.GetBundleLoader(EAssetType.None, dpLoadPath, null, dpManifestPath);
						_depends.Add(dpLoader);
					}
				}
				LoadState = EAssetFileLoadState.CheckDepends;
			}

			// 2. 检测所有依赖完成状态
			if (LoadState == EAssetFileLoadState.CheckDepends)
			{
				foreach (var dpLoader in _depends)
				{
					if (dpLoader.IsDone() == false)
						return;
				}
				LoadState = EAssetFileLoadState.LoadAssetFile;
			}

			// 3. 加载AssetBundle
			if (LoadState == EAssetFileLoadState.LoadAssetFile)
			{
#if UNITY_EDITOR
				// TODO：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(LoadPath) == false)
				{
					LogSystem.Log(ELogType.Warning, $"Not found assetBundle file : {LoadPath}");
					LoadState = EAssetFileLoadState.LoadAssetFileFailed;
					LoadCallback?.Invoke(this);
					return;
				}
#endif

				// Load assetBundle file
				_cacheRequest = AssetBundle.LoadFromFileAsync(LoadPath);
				LoadState = EAssetFileLoadState.CheckAssetFile;
			}

			// 4. 检测AssetBundle加载结果
			if (LoadState == EAssetFileLoadState.CheckAssetFile)
			{
				if (_cacheRequest.isDone == false)
					return;
				_cacheBundle = _cacheRequest.assetBundle;

				// Check scene
				if (AssetType == EAssetType.Scene)
				{
					//_cacheBundle.isStreamedSceneAssetBundle; //TODO 验证接口
					LoadState = EAssetFileLoadState.LoadAssetFileOK;
					LoadCallback?.Invoke(this);
					return;
				}

				// Check error
				if (_cacheBundle == null)
				{
					LogSystem.Log(ELogType.Warning, $"Failed to load assetBundle file : {LoadPath}");
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
		public override void Reference()
		{
			base.Reference();

			// 同时引用一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Reference();
			}
		}
		public override void Release()
		{
			base.Release();

			// 同时释放一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Release();
			}
		}
		public override void UnLoad(bool force)
		{
			// Check fatal
			if (RefCount > 0)
				throw new Exception($"Bundle file loader ref is not zero : {LoadPath}");
			if (IsDone() == false)
				throw new Exception($"Bundle file loader is not done : {LoadPath}");
			if (CheckMainAssetObjectLoaderIsDone() == false)
				throw new Exception($"Main asset object loader is not done : {LoadPath}");

			// 卸载AssetBundle
			if (_cacheBundle != null)
			{
				_cacheBundle.Unload(force);
				_cacheBundle = null;
			}

			_depends.Clear();
		}

		#region 主资源对象
		private AssetObjectLoader _mainAssetLoader = null;
		public override void LoadMainAsset(EAssetType assetType, OnAssetObjectLoad callback)
		{
			// Check error
			if (LoadState != EAssetFileLoadState.LoadAssetFileOK)
			{
				LogSystem.Log(ELogType.Error, $"Can not load asset object, {nameof(AssetBundleLoader)} is not ok : {LoadPath}");
				callback?.Invoke(null, false);
				return;
			}

			// Check secne
			// 注意：场景文件在获取资源对象的时候直接返回成功
			if (assetType == EAssetType.Scene)
			{
				callback?.Invoke(null, true);
				return;
			}

			// 如果加载器不存在
			if (_mainAssetLoader == null)
			{
				string assetName = AssetSystem.GetCacheFileName(LoadPath);
				_mainAssetLoader = new AssetObjectLoader(_cacheBundle, assetName, assetType);
				_mainAssetLoader.LoadCallback = callback;
				_mainAssetLoader.Update(); //立刻轮询
			}
			else
			{
				if (_mainAssetLoader.IsDone())
					callback?.Invoke(_mainAssetLoader.AssetObject, _mainAssetLoader.LoadState == EAssetObjectLoadState.LoadAssetObjectOK);
				else
					_mainAssetLoader.LoadCallback += callback;
			}
		}

		/// <summary>
		/// 检测主资源加载器是否完毕
		/// </summary>
		public bool CheckMainAssetObjectLoaderIsDone()
		{
			if (_mainAssetLoader == null)
				return true;
			return _mainAssetLoader.IsDone();
		}
		#endregion
	}
}