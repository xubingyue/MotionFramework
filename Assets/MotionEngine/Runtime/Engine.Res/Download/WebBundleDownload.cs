//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MotionEngine.Res
{
	public class WebBundleDownload : WebDownload
	{
		/// <summary>
		/// 缓存的AssetBundle
		/// </summary>
		public AssetBundle CacheBundle = null;


		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (LoadState != EWebLoadState.None)
				throw new Exception($"Web bundle download state is not none state. {URL}");

			LoadState = EWebLoadState.Loading;

			// 下载文件
			CacheRequest = UnityWebRequestAssetBundle.GetAssetBundle(URL);
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = ResDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				LogSystem.Log(ELogType.Warning, $"Failed to download web bundle : {URL} Error : {CacheRequest.error}");
				LoadState = EWebLoadState.LoadFailed;
			}
			else
			{
				CacheBundle = DownloadHandlerAssetBundle.GetContent(CacheRequest);
				if (CacheBundle == null)
					LoadState = EWebLoadState.LoadFailed;
				else
					LoadState = EWebLoadState.LoadSucceed;
			}

			// Invoke callback
			LoadCallback?.Invoke(this);
		}
	}
}