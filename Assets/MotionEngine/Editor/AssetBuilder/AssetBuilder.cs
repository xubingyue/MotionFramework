//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;
using UnityEditor;
using MotionEngine.Patch;
using MotionEngine.Utility;

public class AssetBuilder 
{
	/// <summary>
	/// AssetBundle压缩选项
	/// </summary>
	public enum ECompressOption
	{
		Uncompressed = 0,
		StandardCompressionLZMA,
		ChunkBasedCompressionLZ4,
	}

	/// <summary>
	/// 输出的根目录
	/// </summary>
	private string _outputRoot = string.Empty;

	// 构建相关
	public BuildTarget BuildTarget { private set; get; } = BuildTarget.NoTarget; //构建平台
	public int BuildVersion { set; get; } = -1; //构建版本
	public string PackPath { set; get; } = string.Empty; //打包目录
	public string OutputPath { private set; get; }  = string.Empty; //输出目录

	// 构建选项
	public ECompressOption CompressOption = ECompressOption.Uncompressed;
	public bool IsForceRebuild = false;
	public bool IsAppendHash = false;
	public bool IsDisableWriteTypeTree = false;
	public bool IsIgnoreTypeTreeChanges = false;


	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="buildTarget">构建平台</param>
	/// <param name="buildVersion">构建版本</param>
	/// <param name="packPath">构建路径</param>
	public void InitAssetBuilder(BuildTarget buildTarget, int buildVersion, string packPath)
	{
		_outputRoot = AssetHelper.MakeDefaultOutputRootPath();

		// 构建平台
		BuildTarget = buildTarget;
		// 构建版本
		BuildVersion = buildVersion;
		// 构建路径
		PackPath = packPath;
		// 输出路径
		OutputPath = $"{_outputRoot}/{buildTarget}/{PatchDefine.StrBuildManifestFileName}";
	}

	/// <summary>
	/// 准备构建
	/// </summary>
	public void PreAssetBuild()
	{
		Debug.Log("------------------------------OnPreAssetBuild------------------------------");

		// 检测构建平台是否合法
		if (BuildTarget == BuildTarget.NoTarget)
			throw new Exception("[BuildPackage] 请选择目标平台");

		// 检测构建版本是否合法
		if (EditorTools.IsNumber(BuildVersion.ToString()) == false)
			throw new Exception($"[BuildPackage] 版本号格式非法：{BuildVersion}");
		if (BuildVersion < 0)
			throw new Exception("[BuildPackage] 请先设置版本号");

		// 检测打包目录是否为空
		if (string.IsNullOrEmpty(PackPath))
			throw new Exception("[BuildPackage] 打包目录不能为空");

		// 检测输出目录是否为空
		if (string.IsNullOrEmpty(OutputPath))
			throw new Exception("[BuildPackage] 输出目录不能为空");

		// 检测补丁包是否已经存在
		string packagePath = GetPackagePath();
		if (Directory.Exists(packagePath))
			throw new Exception($"[BuildPackage] 补丁包已经存在：{packagePath}");

		// 如果是强制重建
		if (IsForceRebuild)
		{
			// 删除总目录
			string parentPath = $"{_outputRoot}/{BuildTarget}";
			if (Directory.Exists(parentPath))
			{
				Directory.Delete(parentPath, true);
				ShowBuildLog($"删除平台总目录：{parentPath}");
			}
		}

		// 如果输出目录不存在
		if (Directory.Exists(OutputPath) == false)
		{
			Directory.CreateDirectory(OutputPath);
			ShowBuildLog($"创建输出目录：{OutputPath}");
		}
	}

	/// <summary>
	/// 执行构建
	/// </summary>
	public void PostAssetBuild()
	{
		Debug.Log("------------------------------OnPostAssetBuild------------------------------");

		// 准备工作
		List<AssetBundleBuild> buildInfoList = new List<AssetBundleBuild>();
		List<AssetInfo> assetInfoList = PrepareWork();
		if (assetInfoList.Count == 0)
			throw new Exception("[BuildPackage] 构建列表不能为空");

		ShowBuildLog($"构建列表里总共有{assetInfoList.Count}个资源需要构建");
		for (int i = 0; i < assetInfoList.Count; i++)
		{
			AssetInfo assetInfo = assetInfoList[i];
			AssetBundleBuild buildInfo = new AssetBundleBuild();
			buildInfo.assetBundleName = assetInfo.AssetBundleName;
			buildInfo.assetBundleVariant = assetInfo.AssetBundleVariant;
			buildInfo.assetNames = new string[] { assetInfo.AssetPath };
			buildInfoList.Add(buildInfo);
		}

		// 开始构建
		ShowBuildLog($"开始构建......");
		BuildAssetBundleOptions opt = MakeBuildOptions();
		AssetBundleManifest buildManifest = BuildPipeline.BuildAssetBundles(OutputPath, buildInfoList.ToArray(), opt, BuildTarget);
		if (buildManifest == null)
			throw new Exception("[BuildPackage] 构建过程中发生错误！");

		// 清单列表
		string[] allAssetBundles = buildManifest.GetAllAssetBundles();
		ShowBuildLog($"资产清单里总共有{allAssetBundles.Length}个资产");

		// 视频单独打包
		PackageVideo(assetInfoList);
		// 加密资源文件
		EncryptFiles(allAssetBundles);
		
		// 创建补丁文件
		CreatePackageFile(allAssetBundles);
		// 创建说明文件
		CreateReadmeFile(allAssetBundles);

		// 复制更新文件到新的补丁文件夹
		CopyUpdateFiles();

		ShowBuildLog("构建完成");
	}

	/// <summary>
	/// 获取构建选项
	/// </summary>
	private BuildAssetBundleOptions MakeBuildOptions()
	{
		// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
		// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

		BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
		opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

		if (CompressOption == ECompressOption.Uncompressed)
			opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
		else if (CompressOption == ECompressOption.ChunkBasedCompressionLZ4)
			opt |= BuildAssetBundleOptions.ChunkBasedCompression;

		if (IsForceRebuild)
			opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
		if (IsAppendHash)
			opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
		if (IsDisableWriteTypeTree)
			opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
		if (IsIgnoreTypeTreeChanges)
			opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

		return opt;
	}

	/// <summary>
	/// 打印日志
	/// </summary>
	private void ShowBuildLog(string log)
	{
		Debug.Log($"[BuildPackage] {log}");
	}


	#region 准备工作
	/// <summary>
	/// 资源信息类
	/// </summary>
	private class AssetInfo
	{
		public string AssetPath { private set; get; }
		public bool IsPackAsset { private set; get; }
		public bool IsAtalsSpriteAsset { private set; get; }
		public bool IsSceneAsset { private set; get; }
		public bool IsVideoAsset { private set; get; }

		public int DependCount = 0; //被依赖次数
		public string AssetBundleName = null;
		public string AssetBundleVariant = null;

		public AssetInfo(string assetPath)
		{
			AssetPath = assetPath;

			if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
				IsSceneAsset = true;
			else
				IsSceneAsset = false;

			if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(UnityEngine.Video.VideoClip))
				IsVideoAsset = true;
			else
				IsVideoAsset = false;

			if (assetPath.Contains(PatchDefine.StrMyPackRootPath))
				IsPackAsset = true;
			else
				IsPackAsset = false;

			if (assetPath.Contains(PatchDefine.StrMyUISpriteFolderPath))
				IsAtalsSpriteAsset = true;
			else
				IsAtalsSpriteAsset = false;
		}
	}

	/// <summary>
	/// 准备工作
	/// </summary>
	private List<AssetInfo> PrepareWork()
	{
		int progressBarCount = 0;
		Dictionary<string, AssetInfo> allAsset = new Dictionary<string, AssetInfo>();

		// 获取打包目录下所有文件
		DirectoryInfo dirInfo = new DirectoryInfo(PackPath);
		FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

		// 获取所有资源列表
		foreach (FileInfo fileInfo in files)
		{
			string assetPath = EditorTools.AbsolutePathToAssetPath(fileInfo.FullName);
			if (ValidateAsset(assetPath))
			{
				List<AssetInfo> depends = GetDependencies(assetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (allAsset.ContainsKey(assetInfo.AssetPath))
					{
						AssetInfo cacheInfo = allAsset[assetInfo.AssetPath];
						cacheInfo.DependCount++;
					}
					else
					{
						allAsset.Add(assetInfo.AssetPath, assetInfo);
					}
				}
			}
			// 进度条
			progressBarCount++;
			EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{files.Length}", (float)progressBarCount / files.Length);
		}
		EditorUtility.ClearProgressBar();
		progressBarCount = 0;

		// 移除零依赖的资源
		List<string> removeList = new List<string>();
		foreach (KeyValuePair<string, AssetInfo> pair in allAsset)
		{
			if (pair.Value.IsPackAsset)
				continue;
			if (pair.Value.IsAtalsSpriteAsset)
				continue;
			if (pair.Value.DependCount == 0)
				removeList.Add(pair.Value.AssetPath);
		}
		for (int i = 0; i < removeList.Count; i++)
		{
			allAsset.Remove(removeList[i]);
		}

		// 设置资源的打包标签
		foreach (KeyValuePair<string, AssetInfo> pair in allAsset)
		{
			SetAssetPackingTag(pair.Value);
			// 进度条
			progressBarCount++;
			EditorUtility.DisplayProgressBar("进度", $"设置打包标签：{progressBarCount}/{allAsset.Count}", (float)progressBarCount / allAsset.Count);
		}
		EditorUtility.ClearProgressBar();
		progressBarCount = 0;

		// 构建资源列表
		List<AssetInfo> result = new List<AssetInfo>();
		foreach (KeyValuePair<string, AssetInfo> pair in allAsset)
		{
			result.Add(pair.Value);
		}

		return result;
	}
	
	/// <summary>
	/// 获取依赖列表
	/// </summary>
	private List<AssetInfo> GetDependencies(string assetPath)
	{
		List<AssetInfo> depends = new List<AssetInfo>();
		string[] dependArray = AssetDatabase.GetDependencies(assetPath, true);
		foreach (string dependPath in dependArray)
		{
			if (ValidateAsset(dependPath))
			{
				AssetInfo assetInfo = new AssetInfo(dependPath);
				depends.Add(assetInfo);
			}
		}
		return depends;
	}

	/// <summary>
	/// 检测资源是否有效
	/// </summary>
	private bool ValidateAsset(string assetPath)
	{
		if (!assetPath.StartsWith("Assets/"))
			return false;

		string ext = System.IO.Path.GetExtension(assetPath);
		if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta")
			return false;

		return true;
	}
	
	/// <summary>
	/// 设置资源的打包标签
	/// </summary>
	private void SetAssetPackingTag(AssetInfo assetInfo)
	{
		string suffixName = PatchDefine.StrBundleSuffixName;
		if (assetInfo.IsAtalsSpriteAsset)
		{
			string tagName = assetInfo.AssetPath.Remove(assetInfo.AssetPath.LastIndexOf("."));
			tagName = tagName.Remove(assetInfo.AssetPath.LastIndexOf("/"));
			//tagName = tagName.Replace("/Resources/", $"/res/");
			assetInfo.AssetBundleName = $"{tagName}{suffixName}";
		}
		else
		{
			string tagName = assetInfo.AssetPath.Remove(assetInfo.AssetPath.LastIndexOf("."));
			//tagName = tagName.Replace("/Resources/", $"/res/");
			assetInfo.AssetBundleName = $"{tagName}{suffixName}";
		}
	}
	#endregion

	#region 视频相关
	private void PackageVideo(List<AssetInfo> assetInfoList)
	{
		// 注意：在Unity2018.4截止的版本里，安卓还不支持压缩的视频Bundle
		if (BuildTarget == BuildTarget.Android)
		{
			ShowBuildLog($"开始视频单独打包（安卓平台）");
			for (int i = 0; i < assetInfoList.Count; i++)
			{
				AssetInfo assetInfo = assetInfoList[i];
				if (assetInfo.IsVideoAsset)
				{
					BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
					opt |= BuildAssetBundleOptions.DeterministicAssetBundle;
					opt |= BuildAssetBundleOptions.StrictMode;
					opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
					var videoObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(assetInfo.AssetPath);
					string outPath = OutputPath + "/" + assetInfo.AssetBundleName.ToLower();
					bool result = BuildPipeline.BuildAssetBundle(videoObj, new[] { videoObj }, outPath, opt, BuildTarget);
					if (result == false)
						throw new Exception($"视频单独打包失败：{assetInfo.AssetPath}");
				}
			}
		}
	}
	#endregion

	#region 文件加密
	private const string StrEncryptFolderName = "/Assembly/";
	
	private void EncryptFiles(string[] allAssetBundles)
	{
		ShowBuildLog($"开始加密资源文件");

		int progressBarCount = 0;
		foreach (string assetName in allAssetBundles)
		{
			string path = $"{OutputPath}/{assetName}";
			if (path.Contains(StrEncryptFolderName))
			{
				byte[] fileData = File.ReadAllBytes(path);

				// 通过判断文件合法性，规避重复加密一个文件。
				if (EditorTools.CheckBundleFileValid(fileData))
				{
					byte[] bytes = EncryptInternal(fileData);
					File.WriteAllBytes(path, bytes);
					ShowBuildLog($"文件加密完成：{path}");
				}
			}

			// 进度条
			progressBarCount++;
			EditorUtility.DisplayProgressBar("进度", $"加密资源包：{progressBarCount}/{allAssetBundles.Length}", (float)progressBarCount / allAssetBundles.Length);
		}
		EditorUtility.ClearProgressBar();
		progressBarCount = 0;
	}
	private byte[] EncryptInternal(byte[] data)
	{
		// TODO 这里实现你自己的加密算法
		return data;
	}
	#endregion

	#region 文件相关
	private readonly List<string> _updateFiles = new List<string>();

	/// <summary>
	/// 1. 创建补丁文件到输出目录
	/// </summary>
	private void CreatePackageFile(string[] allAssetBundles)
	{
		// 加载旧文件
		PatchFile patchFile = LoadPatchFile();

		// 删除旧文件
		string filePath = OutputPath + $"/{PatchDefine.StrBuildPackageFileName}";
		if (File.Exists(filePath))
			File.Delete(filePath);

		// 创建新文件
		ShowBuildLog($"创建补丁文件：{filePath}");
		using (FileStream fs = File.Create(filePath))
		{
			StreamWriter sw = new StreamWriter(fs);

			// 写入版本信息
			sw.Write(BuildVersion);
			sw.Write("\n");
			sw.Flush();

			// 写入Manifest文件的信息
			{
				string assetName = PatchDefine.StrBuildManifestFileName;
				string path = $"{OutputPath}/{assetName}";
				string md5 = UtilHash.FileMD5(path);
				long sizeKB = EditorTools.GetFileSize(path) / 1024;
				int version = BuildVersion;

				sw.Write($"{assetName}={md5}={sizeKB}={version}");
				sw.Write("\n");
				sw.Flush();
			}

			// 写入所有AssetBundle文件的信息
			foreach (string assetName in allAssetBundles)
			{
				string path = $"{OutputPath}/{assetName}";
				string md5 = UtilHash.FileMD5(path);
				long sizeKB = EditorTools.GetFileSize(path) / 1024;
				int version = BuildVersion;

				// 注意：如果文件没有变化使用旧版本号
				PatchElement element;
				if (patchFile.Elements.TryGetValue(assetName, out element))
				{
					if (element.MD5 == md5)
						version = element.Version;
				}

				sw.Write($"{assetName}={md5}={sizeKB}={version}");
				sw.Write("\n");
				sw.Flush();
			}

			// 关闭文件流
			sw.Close();
			fs.Close();
		}
	}

	/// <summary>
	/// 2. 创建Readme文件到输出目录
	/// </summary>
	private void CreateReadmeFile(string[] allAssetBundles)
	{
		// 删除旧文件
		string filePath = OutputPath + "/readme.txt";
		if (File.Exists(filePath))
			File.Delete(filePath);

		ShowBuildLog($"创建说明文件：{filePath}");

		StringBuilder content = new StringBuilder();
		AppendData(content, $"构建平台：{BuildTarget}");
		AppendData(content, $"构建版本：{BuildVersion}");
		AppendData(content, $"构建目录：{PackPath}");
		AppendData(content, $"构建时间：{DateTime.Now}");

		AppendData(content, "");
		AppendData(content, $"--构建参数--");
		AppendData(content, $"CompressOption：{CompressOption}");
		AppendData(content, $"ForceRebuild：{IsForceRebuild}");
		AppendData(content, $"DisableWriteTypeTree：{IsDisableWriteTypeTree}");
		AppendData(content, $"IgnoreTypeTreeChanges：{IsIgnoreTypeTreeChanges}");

		AppendData(content, "");
		AppendData(content, $"--构建清单--");
		for (int i = 0; i < allAssetBundles.Length; i++)
		{
			AppendData(content, allAssetBundles[i]);
		}

		AppendData(content, "");
		AppendData(content, $"--更新清单--");
		PatchFile patchFile = LoadPatchFile();
		foreach (var pair in patchFile.Elements)
		{
			if (pair.Value.Version == BuildVersion)
			{
				AppendData(content, pair.Key);
			}
		}

		// 创建新文件
		File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
	}
	private void AppendData(StringBuilder sb, string data)
	{
		sb.Append(data);
		sb.Append("\r\n");
	}

	/// <summary>
	/// 3. 复制更新文件到补丁目录
	/// </summary>
	private void CopyUpdateFiles()
	{
		string packagePath = GetPackagePath();
		ShowBuildLog($"开始复制更新文件到版本目录：{packagePath}");

		// 复制Readme文件
		{
			string sourcePath = $"{OutputPath}/readme.txt";
			string destPath = $"{packagePath}/readme.txt";
			EditorTools.CopyFile(sourcePath, destPath, true);
			ShowBuildLog($"复制Readme文件到：{destPath}");
		}

		// 复制补丁文件
		{
			string sourcePath = $"{OutputPath}/{PatchDefine.StrBuildPackageFileName}";
			string destPath = $"{packagePath}/{PatchDefine.StrBuildPackageFileName}";
			EditorTools.CopyFile(sourcePath, destPath, true);
			ShowBuildLog($"复制Package文件到：{destPath}");
		}

		// 复制Manifest文件
		{
			string sourcePath = $"{OutputPath}/{PatchDefine.StrBuildManifestFileName}";
			string destPath = $"{packagePath}/{PatchDefine.StrBuildManifestFileName}";
			EditorTools.CopyFile(sourcePath, destPath, true);
			ShowBuildLog($"复制Manifest文件到：{destPath}");
		}

		// 复制Manifest文件
		{
			string sourcePath = $"{OutputPath}/{PatchDefine.StrBuildManifestFileName}.manifest";
			string destPath = $"{packagePath}/{PatchDefine.StrBuildManifestFileName}.manifest";
			EditorTools.CopyFile(sourcePath, destPath, true);
		}

		// 复制所有更新文件
		PatchFile patchFile = LoadPatchFile();
		foreach (var pair in patchFile.Elements)
		{
			if (pair.Value.Version == BuildVersion)
			{
				string sourcePath = $"{OutputPath}/{pair.Key}";
				string destPath = $"{packagePath}/{pair.Key}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				ShowBuildLog($"复制更新文件：{destPath}");
			}
		}
	}
	private string GetPackagePath()
	{
		return $"{_outputRoot}/{BuildTarget}/{BuildVersion}";
	}

	/// <summary>
	/// 加载补丁文件从输出目录
	/// </summary>
	private PatchFile LoadPatchFile()
	{
		string filePath = $"{OutputPath}/{PatchDefine.StrBuildPackageFileName}";

		PatchFile patchFile = new PatchFile();

		// 如果文件不存在
		if (File.Exists(filePath) == false)
			return patchFile;

		using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			StreamReader sr = new StreamReader(fs);
			patchFile.Parse(sr);
			sr.Close();
			fs.Close();
		}

		return patchFile;
	}
	#endregion
}