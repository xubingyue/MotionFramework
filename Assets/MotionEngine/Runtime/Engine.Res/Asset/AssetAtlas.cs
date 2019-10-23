//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine.U2D;

namespace MotionEngine.Res
{
	public class AssetAtlas : Asset
	{
		public SpriteAtlas Atlas { private set; get; }

		public AssetAtlas()
			: base(EAssetType.Atlas)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Atlas = asset as SpriteAtlas;
			if (Atlas == null)
				return false;

			return true;
		}
	}
}