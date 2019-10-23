//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using MotionEngine.Patch;
using MotionEngine.Utility;

public class AssetBuilderWindow : EditorWindow
{
	static AssetBuilderWindow _thisInstance;

	[MenuItem("MotionTools/Asset - AssetBuilder")]
	static void ShowWindow()
	{
		if (_thisInstance == null)
		{
			_thisInstance = EditorWindow.GetWindow(typeof(AssetBuilderWindow), false, "资源打包工具", true) as AssetBuilderWindow;
			_thisInstance.minSize = new Vector2(600, 600);
		}
		_thisInstance.Show();
	}

	// GUI相关
	private GUIStyle _centerStyle;
	private GUIStyle _leftStyle;
	private bool _showSettingFoldout = true;
	private bool _showToolsFoldout = false;

	/// <summary>
	/// 构建器
	/// </summary>
	private AssetBuilder _assetBuilder = null;


	private void InitInternal()
	{
		if (_assetBuilder != null)
			return;

		_centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		_centerStyle.alignment = TextAnchor.UpperCenter;
		_leftStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		_leftStyle.alignment = TextAnchor.MiddleLeft;

		// 创建构建器类
		Version appVersion = new Version(Application.version);
		int buildVersion = appVersion.Revision;
		BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
		string packPath = EditorPrefs.GetString(StrEditorPackingPath, PatchDefine.StrMyPackRootPath);
		_assetBuilder = new AssetBuilder();
		_assetBuilder.InitAssetBuilder(buildTarget, buildVersion, packPath);

		// 读取配置
		LoadSettingsFromPlayerPrefs(_assetBuilder);
	}
	private void OnGUI()
	{
		// 初始化
		InitInternal();

		// 标题
		EditorGUILayout.LabelField("Build setup", _centerStyle);
		EditorGUILayout.Space();

		// 构建版本
		_assetBuilder.BuildVersion = EditorGUILayout.IntField("Build Version", _assetBuilder.BuildVersion, GUILayout.MaxWidth(250));

		// 打包路径
		EditorGUILayout.LabelField("Build Pack Path", _assetBuilder.PackPath);

		// 输出路径
		EditorGUILayout.LabelField("Build Output Path", _assetBuilder.OutputPath);

		// 构建选项
		EditorGUILayout.Space();
		_assetBuilder.IsForceRebuild = GUILayout.Toggle(_assetBuilder.IsForceRebuild, "Froce Rebuild", GUILayout.MaxWidth(120));

		// 高级选项
		using (new EditorGUI.DisabledScope(false))
		{
			EditorGUILayout.Space();
			_showSettingFoldout = EditorGUILayout.Foldout(_showSettingFoldout, "Advanced Settings");
			if (_showSettingFoldout)
			{
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 1;
				_assetBuilder.CompressOption = (AssetBuilder.ECompressOption)EditorGUILayout.EnumPopup("Compression", _assetBuilder.CompressOption);
				_assetBuilder.IsAppendHash = EditorGUILayout.ToggleLeft("Append Hash", _assetBuilder.IsAppendHash, GUILayout.MaxWidth(120));
				_assetBuilder.IsDisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable Write Type Tree", _assetBuilder.IsDisableWriteTypeTree, GUILayout.MaxWidth(200));
				_assetBuilder.IsIgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore Type Tree Changes", _assetBuilder.IsIgnoreTypeTreeChanges, GUILayout.MaxWidth(200));
				EditorGUI.indentLevel = indent;
			}
		}

		// 构建按钮
		EditorGUILayout.Space();
		if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
		{
			string title;
			string content;
			if (_assetBuilder.IsForceRebuild)
			{
				title = "警告";
				content = "确定开始强制构建吗，这样会删除所有已有构建的文件";
			}
			else
			{
				title = "提示";
				content = "确定开始增量构建吗";
			}
			if (EditorUtility.DisplayDialog(title, content, "Yes", "No"))
			{
				// 清空控制台
				EditorTools.ClearUnityConsole();

				// 存储配置
				SaveSettingsToPlayerPrefs(_assetBuilder);

				EditorApplication.delayCall += ExecuteBuild;
			}
			else
			{
				Debug.LogWarning("[Build] 打包已经取消");
			}
		}

		// 绘制工具栏部分
		OnDrawGUITools();
	}
	private void OnDrawGUITools()
	{
		GUILayout.Space(50);
		using (new EditorGUI.DisabledScope(false))
		{
			_showToolsFoldout = EditorGUILayout.Foldout(_showToolsFoldout, "工具");
			if (_showToolsFoldout)
			{
				EditorGUILayout.Space();

				if (GUILayout.Button("检测损坏预制件", GUILayout.MaxWidth(120), GUILayout.MaxHeight(40)))
				{
					EditorApplication.delayCall += CheckAllPrefabValid;
				}

				if (GUILayout.Button("设置打包路径", GUILayout.MaxWidth(120), GUILayout.MaxHeight(40)))
				{
					string resultPath = EditorTools.OpenFolderPanel("Open Folder Dialog", _assetBuilder.PackPath);
					if (resultPath != null)
					{
						string newPackPath = EditorTools.AbsolutePathToAssetPath(resultPath);
						_assetBuilder.PackPath = newPackPath;
						EditorPrefs.SetString(StrEditorPackingPath, _assetBuilder.PackPath);
					}
				}

				if (GUILayout.Button("刷新流目录（清空后拷贝所有补丁包到流目录）", GUILayout.MaxWidth(300), GUILayout.MaxHeight(40)))
				{
					EditorApplication.delayCall += RefreshStreammingFolder;
				}

				if (GUILayout.Button("刷新输出主目录（清空后拷贝所有补丁包到输出主目录）", GUILayout.MaxWidth(300), GUILayout.MaxHeight(40)))
				{
					EditorApplication.delayCall += RefreshOutputMainFolder;
				}
			}
		}
	}

	/// <summary>
	/// 执行构建
	/// </summary>
	private void ExecuteBuild()
	{
		_assetBuilder.PreAssetBuild();
		_assetBuilder.PostAssetBuild();
	}

	/// <summary>
	/// 检测预制件是否损坏
	/// </summary>
	private void CheckAllPrefabValid()
	{
		// 获取打包目录下所有文件
		string packPath = _assetBuilder.PackPath;
		DirectoryInfo dirInfo = new DirectoryInfo(packPath);
		FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

		// 进度条相关
		int checkCount = 0;
		int invalidCount = 0;
		EditorUtility.DisplayProgressBar("进度", $"检测预制件文件是否损坏：{checkCount}/{files.Length}", (float)checkCount / files.Length);

		// 获取所有资源列表
		foreach (FileInfo fileInfo in files)
		{
			string assetPath = EditorTools.AbsolutePathToAssetPath(fileInfo.FullName);
			string ext = System.IO.Path.GetExtension(assetPath);
			if (ext == ".prefab")
			{
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				if (prefab == null)
				{
					invalidCount++;
					Debug.LogError($"[Build] 发现损坏预制件：{assetPath}");
				}
			}

			// 进度条相关
			checkCount++;
			EditorUtility.DisplayProgressBar("进度", $"检测预制件文件是否损坏：{checkCount}/{files.Length}", (float)checkCount / files.Length);
		}

		EditorUtility.ClearProgressBar();
		if (invalidCount == 0)
			UnityEngine.Debug.Log($"没有发现损坏预制件");
	}

	/// <summary>
	/// 刷新流目录（清空后拷贝所有补丁包到流目录）
	/// </summary>
	private void RefreshStreammingFolder()
	{
		string streamingPath = Application.dataPath + "/StreamingAssets";
		EditorTools.ClearFolder(streamingPath);

		string outputRoot = AssetHelper.MakeDefaultOutputRootPath();
		AssetHelper.CopyPackageToStreamingFolder(_assetBuilder.BuildTarget, outputRoot);
	}

	/// <summary>
	/// 刷新输出主目录（清空后拷贝所有补丁包到输出主目录）
	/// </summary>
	private void RefreshOutputMainFolder()
	{
		string outputPath = _assetBuilder.OutputPath;
		EditorTools.ClearFolder(outputPath);

		string outputRoot = AssetHelper.MakeDefaultOutputRootPath();
		AssetHelper.CopyPackageToManifestFolder(_assetBuilder.BuildTarget, outputRoot);
	}

	#region 设置相关
	private const string StrEditorPackingPath = "StrEditorPackingPath";
	private const string StrEditorCompressOption = "StrEditorCompressOption";
	private const string StrEditorIsForceRebuild = "StrEditorIsForceRebuild";
	private const string StrEditorIsAppendHash = "StrEditorIsAppendHash";
	private const string StrEditorIsDisableWriteTypeTree = "StrEditorIsDisableWriteTypeTree";
	private const string StrEditorIsIgnoreTypeTreeChanges = "StrEditorIsIgnoreTypeTreeChanges";
	private const string StrEditorIsUsePlayerSettingVersion = "StrEditorIsUsePlayerSettingVersion";

	/// <summary>
	/// 存储配置
	/// </summary>
	private static void SaveSettingsToPlayerPrefs(AssetBuilder builder)
	{
		UtilPlayerPrefs.SetEnum<AssetBuilder.ECompressOption>(StrEditorCompressOption, builder.CompressOption);
		UtilPlayerPrefs.SetBool(StrEditorIsForceRebuild, builder.IsForceRebuild);
		UtilPlayerPrefs.SetBool(StrEditorIsAppendHash, builder.IsAppendHash);
		UtilPlayerPrefs.SetBool(StrEditorIsDisableWriteTypeTree, builder.IsDisableWriteTypeTree);
		UtilPlayerPrefs.SetBool(StrEditorIsIgnoreTypeTreeChanges, builder.IsIgnoreTypeTreeChanges);
	}
	
	/// <summary>
	/// 读取配置
	/// </summary>
	private static void LoadSettingsFromPlayerPrefs(AssetBuilder builder)
	{
		builder.CompressOption = UtilPlayerPrefs.GetEnum<AssetBuilder.ECompressOption>(StrEditorCompressOption, AssetBuilder.ECompressOption.Uncompressed);
		builder.IsForceRebuild = UtilPlayerPrefs.GetBool(StrEditorIsForceRebuild, false);
		builder.IsAppendHash = UtilPlayerPrefs.GetBool(StrEditorIsAppendHash, false);
		builder.IsDisableWriteTypeTree = UtilPlayerPrefs.GetBool(StrEditorIsDisableWriteTypeTree, false);
		builder.IsIgnoreTypeTreeChanges = UtilPlayerPrefs.GetBool(StrEditorIsIgnoreTypeTreeChanges, false);
	}
	#endregion
}