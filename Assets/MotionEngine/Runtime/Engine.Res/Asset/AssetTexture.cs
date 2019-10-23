//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	public class AssetTexture : Asset
	{
		public Texture Image { private set; get; }

		public AssetTexture()
			: base(EAssetType.Texture)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Image = asset as Texture;
			if (Image == null)
				return false;

			return true;
		}
	}
}