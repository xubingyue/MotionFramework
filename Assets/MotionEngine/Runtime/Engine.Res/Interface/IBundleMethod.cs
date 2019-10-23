//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Res
{
	public interface IBundleMethod
	{
		/// <summary>
		/// 获取AssetBundle的加载路径
		/// </summary>
		string GetAssetBundleLoadPath(string manifestPath);

		/// <summary>
		/// 获取AssetBundle的直接依赖列表
		/// </summary>
		string[] GetDirectDependencies(string assetBundleName);

		/// <summary>
		/// 获取AssetBundle的所有依赖列表
		/// </summary>
		string[] GetAllDependencies(string assetBundleName);
	}
}