//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionEngine.Res
{
	/// <summary>
	/// 用于读取.bytes格式的文件
	/// </summary>
	public class AssetByte : AssetText
	{
		protected byte[] _bytes;

		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			_bytes = Text.bytes;
			return true;
		}

		/// <summary>
		/// 获取字节数组
		/// </summary>
		public byte[] GetBytes()
		{
			return _bytes;
		}
	}
}