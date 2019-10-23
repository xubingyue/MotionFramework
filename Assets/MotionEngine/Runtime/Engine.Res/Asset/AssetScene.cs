//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MotionEngine.Res
{
	public class AssetScene : Asset
	{
		/// <summary>
		/// 场景加载进度（0-100）
		/// </summary>
		public int Progress { private set; get; }

		/// <summary>
		/// 场景加载完毕回调
		/// </summary>
		public System.Action<string> OnSceneLoad;


		public AssetScene()
			: base(EAssetType.Scene)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			// 开始异步加载场景
			string[] splits = ResName.Split('/');
			string sceneName = splits[splits.Length-1];
			Engine.Instance.StartCoroutine(StartLoading(sceneName));
			return true;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		private IEnumerator StartLoading(string name)
		{
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
			WaitForEndOfFrame waitForEnd = new WaitForEndOfFrame();

			// Wait until the asynchronous scene fully loads
			while (!asyncLoad.isDone)
			{
				int percent = (int)(asyncLoad.progress * 100);
				while (Progress < percent)
				{
					Progress++;
					yield return waitForEnd;
				}
				yield return null;
			}

			if (OnSceneLoad != null)
				OnSceneLoad.Invoke(ResName);
		}
	}
}