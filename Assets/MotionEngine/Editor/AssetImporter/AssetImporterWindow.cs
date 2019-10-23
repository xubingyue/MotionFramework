//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 资源导入管理窗口
/// </summary>
public class AssetImporterWindow : EditorWindow
{
	static AssetImporterWindow _thisInstance;

	[MenuItem("MotionTools/Asset - AssetImporter")]
	static void ShowWindow()
	{
		if (_thisInstance == null)
		{
			_thisInstance = EditorWindow.GetWindow(typeof(AssetImporterWindow), false, "资源导入工具", true) as AssetImporterWindow;
			_thisInstance.minSize = new Vector2(600, 600);
		}

		_thisInstance.Show();
	}

	/// <summary>
	/// 上次打开的文件夹路径
	/// </summary>
	private string _lastOpenFolderPath = "Assets/";

	/// <summary>
	/// 资源处理器类列表
	/// </summary>
	private string[] _processorClassArray = null;

	// 初始化相关
	private bool _isInit = false;
	private void Init()
	{
		// 字典KEY转换为数组
		List<string> keyList = new List<string>();
		foreach(var pair in AssetImporterProcessor.CacheTypes)
		{
			keyList.Add(pair.Key);
		}
		_processorClassArray = keyList.ToArray();
	}
	private int NameToIndex(string name)
	{
		for (int i = 0; i < _processorClassArray.Length; i++)
		{
			if (_processorClassArray[i] == name)
				return i;
		}
		return 0;
	}
	private string IndexToName(int index)
	{
		for (int i = 0; i < _processorClassArray.Length; i++)
		{
			if (i == index)
				return _processorClassArray[i];
		}
		return string.Empty;
	}

	private void OnGUI()
	{
		if (AssetImporterProcessor.Setting == null)
			AssetImporterProcessor.LoadSettingFile();

		if (_isInit == false)
		{
			_isInit = true;
			Init();
		}

		// 添加按钮
		if (GUILayout.Button("+"))
		{
			string resultPath = EditorTools.OpenFolderPanel("+", _lastOpenFolderPath);
			if (resultPath != null)
			{
				_lastOpenFolderPath = EditorTools.AbsolutePathToAssetPath(resultPath);
				AssetImporterProcessor.AddSettingElement(_lastOpenFolderPath);
			}
		}

		// 列表显示
		for (int i = 0; i < AssetImporterProcessor.Setting.Elements.Count; i++)
		{
			string folderPath = AssetImporterProcessor.Setting.Elements[i].FolderPath;
			string processorName = AssetImporterProcessor.Setting.Elements[i].ProcessorName;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(folderPath);

			int index = NameToIndex(processorName);
			int newIndex = EditorGUILayout.Popup(index, _processorClassArray, GUILayout.MaxWidth(150));
			if(newIndex != index)
			{
				string processClassName = IndexToName(newIndex);
				AssetImporterProcessor.ModifySettingElement(folderPath, processClassName);
			}

			if (GUILayout.Button("-", GUILayout.MaxWidth(80)))
			{
				AssetImporterProcessor.RemoveSettingElement(folderPath);
				break;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}