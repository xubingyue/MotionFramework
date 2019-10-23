//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	public class AssetObject : Asset
	{
		/// <summary>
		/// 实例化的游戏对象
		/// </summary>
		public GameObject GameObj { get; protected set; }

		public AssetObject()
			: base(EAssetType.Object)
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			// 实例化
			GameObj = Object.Instantiate(asset) as GameObject;
			if (GameObj == null)
			{
				LogSystem.Log(ELogType.Error, $"Failed to instantiate GameObject : {ResName}");
				return false;
			}

			return true;
		}

		public override void UnLoad()
		{
			if (GameObj != null)
			{
				UnityEngine.Object.Destroy(GameObj);
				GameObj = null;
			}

			base.UnLoad();
		}
	}
}