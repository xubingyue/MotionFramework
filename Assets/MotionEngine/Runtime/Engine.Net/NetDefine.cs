//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine.Net
{
	public class NetDefine
	{
		public const int PackageMaxSize = ushort.MaxValue; // 网络包最大长度
		public const int ByteBufferSize = PackageMaxSize * 4; // 缓冲区长度（注意：推荐4倍最大包体长度）
	}

	/// <summary>
	/// 网络状态
	/// </summary>
	public enum ENetworkState
	{
		Disconnect,
		Connecting,
		Connected,
	}
}