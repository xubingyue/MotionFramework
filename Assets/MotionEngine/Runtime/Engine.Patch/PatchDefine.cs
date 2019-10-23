//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Patch
{
	public class PatchDefine
	{
		public const string StrBundleSuffixName = ".unity3d"; // AssetBundle文件后缀名称
		public const string StrBuildManifestFileName = "Manifest"; //构建输出的清单文件名称
		public const string StrBuildPackageFileName = "package.bytes"; //构建输出的补丁文件名称


		/// <summary>
		/// 我们设定的打包根路径
		/// </summary>
		public const string StrMyPackRootPath = "Assets/Works/Resources";

		/// <summary>
		/// 我们设定的图集根路径
		/// </summary>
		public const string StrMyUIAtlasFolderPath = "Assets/Works/Resources/UIAtlas";

		/// <summary>
		/// 我们设定的精灵根路径（图集关联的精灵）
		/// </summary>
		public const string StrMyUISpriteFolderPath = "Assets/WorksArt/Panel/UISprite";
	}
}