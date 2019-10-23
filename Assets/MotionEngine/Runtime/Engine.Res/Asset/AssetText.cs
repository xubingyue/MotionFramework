//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	public class AssetText : Asset
	{
		public TextAsset Text { private set; get; }

		public AssetText()
			: base(EAssetType.Text)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			Text = asset as TextAsset;
			if (Text == null)
				return false;

			return true;
		}
	}
}