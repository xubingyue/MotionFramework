//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Res
{
	/// <summary>
	/// 资源加载器基类
	/// </summary>
	public abstract class AssetFileLoader
	{
		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { get; private set; }

		/// <summary>
		/// 资源类型
		/// </summary>
		public EAssetType AssetType { get; private set; }

		/// <summary>
		/// 加载路径
		/// </summary>
		public string LoadPath { get; private set; }

		/// <summary>
		/// 完成回调
		/// </summary>
		public OnAssetFileLoad LoadCallback { get; set; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public EAssetFileLoadState LoadState { get; protected set; }


		public AssetFileLoader(EAssetType assetType, string loadPath)
		{
			RefCount = 0;	
			AssetType = assetType;
			LoadPath = loadPath;
			LoadState = EAssetFileLoadState.None;
		}

		/// <summary>
		/// 轮询更新方法
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// 加载主资源对象
		/// 注意：在加载主资源对象的时候，需要传入资源类型，因为依赖Bundle被加载的时候，AssetType为空。
		/// </summary>
		public abstract void LoadMainAsset(EAssetType mainAssetType, OnAssetObjectLoad callback);

		/// <summary>
		/// 引用接口（引用计数递加）
		/// </summary>
		public virtual void Reference()
		{
			RefCount++;
		}

		/// <summary>
		/// 释放接口（引用计数递减）
		/// </summary>
		public virtual void Release()
		{
			RefCount--;
		}

		/// <summary>
		/// 卸载接口
		/// </summary>
		public virtual void UnLoad(bool force)
		{
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			return LoadState == EAssetFileLoadState.LoadAssetFileOK || LoadState == EAssetFileLoadState.LoadAssetFileFailed;
		}
	}
}