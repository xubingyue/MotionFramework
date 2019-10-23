//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine
{
	/// <summary>
	/// 日志类型
	/// </summary>
	public enum ELogType
	{
		Log,
		Error,
		Warning,
		Exception,
	}

	/// <summary>
	/// 日志消息回调
	/// </summary>
	/// <param name="logType">日志类型</param>
	/// <param name="message">日志内容</param>
	public delegate void LogCallback(ELogType logType, string message);
}