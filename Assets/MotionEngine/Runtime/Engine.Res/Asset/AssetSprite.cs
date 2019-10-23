//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	public class AssetSprite : Asset
	{
		public Sprite Image { private set; get; }

		public AssetSprite()
			: base(EAssetType.Sprite)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Image = asset as Sprite;
			if (Image == null)		
				return false;		

			return true;
		}
	}
}