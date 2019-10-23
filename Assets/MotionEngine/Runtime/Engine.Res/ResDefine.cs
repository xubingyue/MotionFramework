//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Res
{
	public class ResDefine
	{
		public const int WebRequestTimeout = 30; //网络请求的超时时间（单位：秒）
	}

	/// <summary>
	/// 当资源文件加载完毕
	/// </summary>
	public delegate void OnAssetFileLoad(AssetFileLoader loader);

	/// <summary>
	/// 当资源对象加载完毕
	/// </summary>
	public delegate void OnAssetObjectLoad(UnityEngine.Object asset, bool result);

	/// <summary>
	/// 资源加载模式
	/// </summary>
	public enum EAssetLoadMode
	{
		ResourceMode, //Resource加载模式
		BundleMode, //AssetBundle加载模式
	}

	/// <summary>
	/// 资源类型
	/// </summary>
	public enum EAssetType
	{
		None,
		Scene, //场景
		Text, //配表（UnityEngine.TextAsset）
		Audio, //音频（UnityEngine.AudioClip）
		Texture, //纹理（UnityEngine.Texture）
		Sprite, //精灵（UnityEngine.Sprite）
		Atlas, //图集（UnityEngine.U2D.SpriteAtlas）
		Video, //视频 (UnityEngine.Video.VideoClip)
		Object, //预制体（UnityEngine.GameObject）
	}

	/// <summary>
	/// 资源加载结果
	/// </summary>
	public enum EAssetResult
	{
		None,
		Loading,
		Failed,
		OK,
	}

	/// <summary>
	/// 资源文件加载状态
	/// </summary>
	public enum EAssetFileLoadState
	{
		None = 0,
		LoadDepends,
		CheckDepends,
		LoadAssetFile,
		CheckAssetFile,
		LoadAssetFileOK,
		LoadAssetFileFailed,
	}

	/// <summary>
	/// 资源对象加载状态
	/// </summary>
	public enum EAssetObjectLoadState
	{
		None = 0,
		LoadAssetObject,
		CheckAssetObject,
		LoadAssetObjectOK,
		LoadAssetObjectFailed,
	}

	/// <summary>
	/// 网络加载状态
	/// </summary>
	public enum EWebLoadState
	{
		None = 0,
		Loading, //加载中
		LoadSucceed, //加载成功
		LoadFailed, //加载失败
	}
}