//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine.Video;

namespace MotionEngine.Res
{
	public class AssetVideo : Asset
	{
		public VideoClip Clip { private set; get; }

		public AssetVideo()
			: base(EAssetType.Video)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Clip = asset as VideoClip;
			if (Clip == null)
				return false;

			return true;
		}
	}
}