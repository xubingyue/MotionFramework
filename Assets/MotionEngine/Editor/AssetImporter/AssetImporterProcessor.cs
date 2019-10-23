//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using MotionEngine.Utility;

/// <summary>
/// 资源导入管理类
/// </summary>
public class AssetImporterProcessor : AssetPostprocessor
{
	/// <summary>
	/// 配置文件
	/// </summary>
	public static ImportSetting Setting;

	/// <summary>
	/// 导入器类型集合
	/// </summary>
	public static readonly Dictionary<string, System.Type> CacheTypes = new Dictionary<string, System.Type>();

	/// <summary>
	/// 导入器集合
	/// </summary>
	public static readonly Dictionary<string, IAssetProcessor> CacheProcessor = new Dictionary<string, IAssetProcessor>();


	public static void LoadSettingFile()
	{
		// 加载配置文件
		Setting = AssetDatabase.LoadAssetAtPath<ImportSetting>(EditorDefine.ImporterSettingFilePath);
		if (Setting == null)
		{
			Debug.LogWarning($"Create new ImportSetting.asset : {EditorDefine.ImporterSettingFilePath}");
			Setting = ScriptableObject.CreateInstance<ImportSetting>();
			EditorTools.CreateFileDirectory(EditorDefine.ImporterSettingFilePath);
			AssetDatabase.CreateAsset(Setting, EditorDefine.ImporterSettingFilePath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		else
		{
			Debug.Log("Load ImportSetting.asset ok");
		}

		// Clear
		CacheTypes.Clear();
		CacheProcessor.Clear();

		// 获取所有资源处理器类型
		List<Type> result = UtilAssembly.GetAssignableTypes(typeof(IAssetProcessor));
		for(int i=0; i<result.Count; i++)
		{
			Type type = result[i];
			if (CacheTypes.ContainsKey(type.Name) == false)
				CacheTypes.Add(type.Name, type);
		}
	}
	public static void SaveSettingFile()
	{
		if (Setting != null)
		{
			EditorUtility.SetDirty(Setting);
			AssetDatabase.SaveAssets();
		}
	}
	public static void AddSettingElement(string folderPath)
	{
		if (IsContainsSetting(folderPath) == false)
		{
			ImportSetting.Wrapper element = new ImportSetting.Wrapper();
			element.FolderPath = folderPath;
			element.ProcessorName = nameof(DefaultProcessor);
			Setting.Elements.Add(element);
			SaveSettingFile();
		}
	}
	public static void RemoveSettingElement(string folderPath)
	{
		for (int i = 0; i < Setting.Elements.Count; i++)
		{
			if (Setting.Elements[i].FolderPath == folderPath)
			{
				Setting.Elements.RemoveAt(i);
				break;
			}
		}
		SaveSettingFile();
	}
	public static void ModifySettingElement(string folderPath, string processorName)
	{
		for (int i = 0; i < Setting.Elements.Count; i++)
		{
			if (Setting.Elements[i].FolderPath == folderPath)
			{
				Setting.Elements[i].ProcessorName = processorName;
				break;
			}
		}
		SaveSettingFile();
	}
	public static bool IsContainsSetting(string folderPath)
	{
		for (int i = 0; i < Setting.Elements.Count; i++)
		{
			if (Setting.Elements[i].FolderPath == folderPath)
				return true;
		}
		return false;
	}

	/// <summary>
	/// 获取资源处理器
	/// </summary>
	/// <param name="importAssetPath">导入的资源路径</param>
	/// <returns>如果该资源的路径没包含在列表里返回NULL</returns>
	private static IAssetProcessor GetCustomProcessor(string importAssetPath)
	{
		// 如果是过滤文件
		string fileName = Path.GetFileNameWithoutExtension(importAssetPath);
		if (fileName.EndsWith("@"))
			return null;

		// 获取处理器类名
		string className = null;
		for (int i = 0; i < Setting.Elements.Count; i++)
		{
			var element = Setting.Elements[i];
			if (importAssetPath.Contains(element.FolderPath))
			{
				className = element.ProcessorName;
				break;
			}
		}
		if (string.IsNullOrEmpty(className))
			return null;

		// 先从缓存里获取
		IAssetProcessor processor = null;
		if (CacheProcessor.TryGetValue(className, out processor))
			return processor;

		// 如果不存在创建处理器
		System.Type type;
		if (CacheTypes.TryGetValue(className, out type))
		{
			processor = (IAssetProcessor)Activator.CreateInstance(type);
			return processor;
		}
		else
		{
			Debug.LogError($"资源处理器类型无效：{className}");
			return null;
		}
	}


	#region 模型处理
	public void OnPreprocessModel()
	{
		if (Setting == null)
			LoadSettingFile();
		if (Setting == null || Setting.Toggle == false)
			return;

		string importAssetPath = this.assetPath;
		IAssetProcessor processor = GetCustomProcessor(importAssetPath);
		if (processor != null)
			processor.OnPreprocessModel(importAssetPath, this.assetImporter);
	}
	public void OnPostprocessModel(GameObject go)
	{
	}
	#endregion

	#region 纹理处理
	public void OnPreprocessTexture()
	{
		if (Setting == null)
			LoadSettingFile();
		if (Setting == null || Setting.Toggle == false)
			return;

		string importAssetPath = this.assetPath;
		IAssetProcessor processor = GetCustomProcessor(importAssetPath);
		if (processor != null)
			processor.OnPreprocessTexture(importAssetPath, this.assetImporter);
	}
	public void OnPostprocessTexture(Texture2D texture)
	{
	}
	public void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
	{
	}
	#endregion

	#region 音频处理
	public void OnPreprocessAudio()
	{
		if (Setting == null)
			LoadSettingFile();
		if (Setting == null || Setting.Toggle == false)
			return;

		string importAssetPath = this.assetPath;
		IAssetProcessor processor = GetCustomProcessor(importAssetPath);
		if (processor != null)
			processor.OnPreprocessAudio(importAssetPath, this.assetImporter);
	}
	public void OnPostprocessAudio(AudioClip clip)
	{
	}
	#endregion

	#region 其他处理
	/// <summary>
	/// 注意：该方法从Unity2018版本开始才起效
	/// </summary>
	public void OnPreprocessAsset()
	{
		string extension = Path.GetExtension(this.assetPath);

		if(extension == ".spriteatlas")
		{
			OnPreprocessSpriteAtlas();
		}
	}

	/// <summary>
	/// 处理图集
	/// </summary>
	private void OnPreprocessSpriteAtlas()
	{
		if (Setting == null)
			LoadSettingFile();
		if (Setting == null || Setting.Toggle == false)
			return;

		string importAssetPath = this.assetPath;
		IAssetProcessor processor = GetCustomProcessor(importAssetPath);
		if (processor != null)
			processor.OnPreprocessSpriteAtlas(importAssetPath, this.assetImporter);
	}
	#endregion
}