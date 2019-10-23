//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	public class AssetAudio : Asset
	{
		/// <summary>
		/// 标签
		/// </summary>
		public int AudioTag { private set; get; }

		/// <summary>
		/// 资源对象
		/// </summary>
		public AudioClip Clip { private set; get; }

		public AssetAudio(int audioTag)
			: base(EAssetType.Audio)
		{
			AudioTag = audioTag;
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Clip = asset as AudioClip;
			if (Clip == null)
				return false;

			return true;
		}
	}
}