using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

public class UIPanelMonitor : Editor
{
	[InitializeOnLoadMethod]
	static void StartInitializeOnLoadMethod()
	{
		// 监听Inspector的Apply事件
		PrefabUtility.prefabInstanceUpdated = delegate (GameObject go)
		{
			UIManifest manifest = go.GetComponent<UIManifest>();
			if (manifest != null)
				manifest.Refresh();
		};

		// 监听新的Prefab系统
		PrefabStage.prefabSaving += OnPrefabSaving;
	}

	static void OnPrefabSaving(GameObject go)
	{
		PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
		if(stage != null)
		{
			UIManifest manifest = go.GetComponent<UIManifest>();
			if (manifest != null)
				manifest.Refresh();
		}
	}
}

/*
public class TestAssets : UnityEditor.AssetModificationProcessor
{
	static string[] OnWillSaveAssets(string[] paths)
	{
		foreach(var path in paths)
		{
			Debug.Log(path);
		}

		return paths;
	}
}
*/