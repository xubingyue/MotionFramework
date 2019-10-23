//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using MotionEngine.Res;

/// <summary>
/// 运行时的资源列表查看器
/// </summary>
public class AssetBrowserWindow : EditorWindow
{
	static AssetBrowserWindow _thisInstance;

	[MenuItem("MotionTools/Asset - AssetBrowser")]
	static void ShowWindow()
	{
		if (_thisInstance == null)
		{
			_thisInstance = EditorWindow.GetWindow(typeof(AssetBrowserWindow), false, "资源浏览工具", true) as AssetBrowserWindow;
			_thisInstance.minSize = new Vector2(600, 600);
		}

		_thisInstance.Show();
	}

	/// <summary>
	/// 显示信息集合
	/// </summary>
	private List<string> _cacheInfos = new List<string>(1000);

	/// <summary>
	/// GUI滑动条位置
	/// </summary>
	private Vector2 _scrollPos = Vector2.zero;

	/// <summary>
	/// GUI搜索框
	/// </summary>
	private SearchField _searchField;

	/// <summary>
	/// 搜索的关键字
	/// </summary>
	private string _searchKey;

	private void OnGUI()
	{
		if (_searchField == null)
			_searchField = new SearchField();

		_searchKey = _searchField.OnGUI(_searchKey);
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

		EditorGUILayout.Space();
		string[]  showInfos = GetShowInfos();
		EditorGUILayout.LabelField($"加载器总数：{showInfos.Length}");
		for (int i=0; i< showInfos.Length; i++)
		{
			EditorGUILayout.SelectableLabel(showInfos[i], GUILayout.Height(18));
		}

		EditorGUILayout.EndScrollView();
	}

	/// <summary>
	/// 获取显示列表
	/// </summary>
	private string[] GetShowInfos()
	{
		// 清空列表
		_cacheInfos.Clear();

		var fileLoaders = AssetSystem.GetFileLoaders();
		foreach (var loader in fileLoaders)
		{
			// 只搜索关键字
			if (string.IsNullOrEmpty(_searchKey) == false)
			{
				if (loader.LoadPath.Contains(_searchKey) == false)
					continue;
			}

			string showInfo = EditorTools.Substring(loader.LoadPath, "/assets/", false);
			showInfo = showInfo.Replace(".unity3d", string.Empty);
			showInfo = $"{showInfo} = {loader.RefCount}";

			// 添加到显示列表
			_cacheInfos.Add(showInfo);
		}

		// 重新排序
		var array = _cacheInfos.ToArray();
		System.Array.Sort(array, string.CompareOrdinal);
		return array;
	}
}