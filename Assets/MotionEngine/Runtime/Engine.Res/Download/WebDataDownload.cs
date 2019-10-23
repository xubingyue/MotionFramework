//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MotionEngine.Res
{
	public class WebDataDownload : WebDownload
	{
		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (LoadState != EWebLoadState.None)
				throw new Exception($"Web data download state is not none state. {URL}");

			LoadState = EWebLoadState.Loading;

			// 下载文件
			CacheRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
			DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
			CacheRequest.downloadHandler = handler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = ResDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				LogSystem.Log(ELogType.Warning, $"Failed to download web data : {URL} Error : {CacheRequest.error}");
				LoadState = EWebLoadState.LoadFailed;
			}
			else
			{
				LoadState = EWebLoadState.LoadSucceed;
			}

			// Invoke callback
			LoadCallback?.Invoke(this);
		}

		public byte[] GetData()
		{
			if (LoadState == EWebLoadState.LoadSucceed)
				return CacheRequest.downloadHandler.data;
			else
				return null;
		}
		public string GetText()
		{
			if (LoadState == EWebLoadState.LoadSucceed)
				return CacheRequest.downloadHandler.text;
			else
				return null;
		}
	}
}