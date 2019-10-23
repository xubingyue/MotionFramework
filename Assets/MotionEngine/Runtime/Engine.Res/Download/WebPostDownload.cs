//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MotionEngine.Res
{
	public class WebPostDownload : WebDownload
	{
		public string PostContent = null;

		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (string.IsNullOrEmpty(PostContent))
				throw new Exception($"Web post content is null or empty. {URL}");

			// Check fatal
			if (LoadState != EWebLoadState.None)
				throw new Exception($"Web post download state is not none state. {URL}");

			LoadState = EWebLoadState.Loading;

			// 投递数据
			byte[] bodyRaw = Encoding.UTF8.GetBytes(PostContent);

			// 下载文件
			CacheRequest =  new UnityWebRequest(URL, UnityWebRequest.kHttpVerbPOST);
			UploadHandlerRaw uploadHandler = new UploadHandlerRaw(bodyRaw);
			DownloadHandlerBuffer downloadhandler = new DownloadHandlerBuffer();
			CacheRequest.uploadHandler = uploadHandler;
			CacheRequest.downloadHandler = downloadhandler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = ResDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				LogSystem.Log(ELogType.Warning, $"Failed to request web post : {URL} Error : {CacheRequest.error}");
				LoadState = EWebLoadState.LoadFailed;
			}
			else
			{
				LoadState = EWebLoadState.LoadSucceed;
			}

			// Invoke callback
			LoadCallback?.Invoke(this);
		}

		public string GetResponse()
		{
			if (LoadState == EWebLoadState.LoadSucceed)
				return CacheRequest.downloadHandler.text;
			else
				return null;
		}
	}
}