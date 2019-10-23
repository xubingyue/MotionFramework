//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;

/// <summary>
/// 默认的资源处理器
/// </summary>
public class DefaultProcessor : IAssetProcessor
{
	private string GetTemplateAssetPath(string importAssetPath)
	{
		// 获取导入资源所在文件夹内的所有文件的GUID
		string folderPath = Path.GetDirectoryName(importAssetPath);
		string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
		if (guids.Length == 0)
			return string.Empty;

		// 我们以Project视图里文件夹内首个资源做为模板
		return AssetDatabase.GUIDToAssetPath(guids[0]);
	}
	private AssetImporter GetTemplateAssetImporter(string importAssetPath)
	{
		// 获取模板资源路径
		string templateAssetPath = GetTemplateAssetPath(importAssetPath);
		if (string.IsNullOrEmpty(templateAssetPath))
			return null;

		var templateImporter = AssetImporter.GetAtPath(templateAssetPath);
		if (templateImporter == null)
			Debug.LogError($"[DefaultProcessor] 模板资源导入器获取失败 : {templateAssetPath}");

		return templateImporter;
	}

	private void ProcessAllModel(ModelImporter templateImporter)
	{
		string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
		string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Model}", new[] { folderPath });
		for (int i = 0; i < guids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
				continue;
			AssetDatabase.ImportAsset(assetPath);
		}
	}
	private void ProcessAllTexture(TextureImporter templateImporter)
	{
		string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
		string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Texture}", new[] { folderPath });
		for (int i = 0; i < guids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
				continue;
			AssetDatabase.ImportAsset(assetPath);
		}
	}
	private void ProcessAllAudio(AudioImporter templateImporter)
	{
		string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
		string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.AudioClip}", new[] { folderPath });
		for (int i = 0; i < guids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
				continue;
			AssetDatabase.ImportAsset(assetPath);
		}
	}

	#region 接口方法
	public void OnPreprocessModel(string importAssetPath, AssetImporter assetImporter)
	{
		ModelImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as ModelImporter;
		if (templateImporter == null)
			return;

		// 如果模板被更改，那么更新全部资源
		if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
		{
			ProcessAllModel(templateImporter);
			return;
		}

		ModelImporter targetImporter = assetImporter as ModelImporter;
		CopyModelImporter(targetImporter, templateImporter);
		Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
	}
	public void OnPreprocessTexture(string importAssetPath, AssetImporter assetImporter)
	{
		TextureImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as TextureImporter;
		if (templateImporter == null)
			return;

		// 如果模板被更改，那么更新全部资源
		if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
		{
			ProcessAllTexture(templateImporter);
			return;
		}

		TextureImporter targetImporter = assetImporter as TextureImporter;
		CopyTextureImporter(targetImporter, templateImporter);
		Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
	}
	public void OnPreprocessAudio(string importAssetPath, AssetImporter assetImporter)
	{
		AudioImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as AudioImporter;
		if (templateImporter == null)
			return;

		// 如果模板被更改，那么更新全部资源
		if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
		{
			ProcessAllAudio(templateImporter);
			return;
		}

		AudioImporter targetImporter = assetImporter as AudioImporter;
		CopyAudioImporter(targetImporter, templateImporter);
		Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
	}
	public void OnPreprocessSpriteAtlas(string importAssetPath, AssetImporter assetImporter)
	{
		string templateAssetPath = GetTemplateAssetPath(importAssetPath);
		if (string.IsNullOrEmpty(templateAssetPath))
			throw new System.Exception($"图集资源模板获取失败：{importAssetPath}");

		SpriteAtlas template = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(templateAssetPath);
		SpriteAtlas target = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(importAssetPath);
		CopySpriteAtlasSetting(target, template);
		Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
	}
	#endregion

	#region 复制导入器的静态全局方法
	public static void CopyModelImporter(ModelImporter targetImporter, ModelImporter templateImporter)
	{
		//NOTE：Unity没有提供模型导入器的拷贝接口

		// Scene
		targetImporter.globalScale = templateImporter.globalScale;
		targetImporter.importBlendShapes = templateImporter.importBlendShapes;
		targetImporter.importVisibility = templateImporter.importVisibility;
		targetImporter.importCameras = templateImporter.importCameras;
		targetImporter.importLights = templateImporter.importLights;
		targetImporter.preserveHierarchy = templateImporter.preserveHierarchy;

		// Meshes
		targetImporter.meshCompression = templateImporter.meshCompression;
		targetImporter.isReadable = templateImporter.isReadable;
		targetImporter.optimizeGameObjects = templateImporter.optimizeGameObjects;
		targetImporter.addCollider = templateImporter.addCollider;

		// Geometry
		targetImporter.keepQuads = templateImporter.keepQuads;
		targetImporter.weldVertices = templateImporter.weldVertices;
		targetImporter.indexFormat = templateImporter.indexFormat;
		targetImporter.importBlendShapes = templateImporter.importBlendShapes;
		targetImporter.importBlendShapeNormals = templateImporter.importBlendShapeNormals;
		targetImporter.normalSmoothingAngle = templateImporter.normalSmoothingAngle;
		targetImporter.normalSmoothingSource = templateImporter.normalSmoothingSource;
		targetImporter.importTangents = templateImporter.importTangents;
		targetImporter.swapUVChannels = templateImporter.swapUVChannels;
		targetImporter.generateSecondaryUV = templateImporter.generateSecondaryUV;
		targetImporter.secondaryUVAngleDistortion = templateImporter.secondaryUVAngleDistortion;
		targetImporter.secondaryUVAreaDistortion = templateImporter.secondaryUVAreaDistortion;
		targetImporter.secondaryUVHardAngle = templateImporter.secondaryUVHardAngle;
		targetImporter.secondaryUVPackMargin = templateImporter.secondaryUVPackMargin;

		// Animation
		targetImporter.animationType = templateImporter.animationType;
	}
	public static void CopyTextureImporter(TextureImporter targetImporter, TextureImporter templateImporter)
	{
		// 通用属性
		TextureImporterSettings temper = new TextureImporterSettings();
		templateImporter.ReadTextureSettings(temper);
		targetImporter.SetTextureSettings(temper);

		// 平台设置
		TextureImporterPlatformSettings platformSettingPC = templateImporter.GetPlatformTextureSettings("Standalone");
		TextureImporterPlatformSettings platformSettingIOS = templateImporter.GetPlatformTextureSettings("iPhone");
		TextureImporterPlatformSettings platformSettingAndroid = templateImporter.GetPlatformTextureSettings("Android");
		targetImporter.SetPlatformTextureSettings(platformSettingPC);
		targetImporter.SetPlatformTextureSettings(platformSettingIOS);
		targetImporter.SetPlatformTextureSettings(platformSettingAndroid);
	}
	public static void CopyAudioImporter(AudioImporter targetImporter, AudioImporter templateImporter)
	{
		// 通用属性
		targetImporter.forceToMono = templateImporter.forceToMono;
		targetImporter.loadInBackground = templateImporter.loadInBackground;
		targetImporter.ambisonic = templateImporter.ambisonic;
		targetImporter.defaultSampleSettings = templateImporter.defaultSampleSettings;

		// 平台设置
		AudioImporterSampleSettings sampleSettingsPC = templateImporter.GetOverrideSampleSettings("Standalone");
		AudioImporterSampleSettings sampleSettingsIOS = templateImporter.GetOverrideSampleSettings("iOS");
		AudioImporterSampleSettings sampleSettingsAndroid = templateImporter.GetOverrideSampleSettings("Android");
		targetImporter.SetOverrideSampleSettings("Standalone", sampleSettingsPC);
		targetImporter.SetOverrideSampleSettings("iOS", sampleSettingsIOS);
		targetImporter.SetOverrideSampleSettings("Android", sampleSettingsAndroid);
	}
	public static void CopySpriteAtlasSetting(SpriteAtlas target, SpriteAtlas template)
	{
		// 注意：默认设置为False
		target.SetIncludeInBuild(false);

		// 通用属性
		target.SetPackingSettings(template.GetPackingSettings());
		target.SetTextureSettings(template.GetTextureSettings());

		// 平台设置
		target.SetPlatformSettings(template.GetPlatformSettings("Standalone"));
		target.SetPlatformSettings(template.GetPlatformSettings("iPhone"));
		target.SetPlatformSettings(template.GetPlatformSettings("Android"));
	}
	#endregion
}