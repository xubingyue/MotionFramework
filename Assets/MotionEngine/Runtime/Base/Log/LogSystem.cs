//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine
{
	public static class LogSystem
	{
		private static LogCallback _callback;

		/// <summary>
		/// 注册监听日志的委托
		/// </summary>
		public static void RegisterCallback(LogCallback callback)
		{
			_callback += callback;
		}

		/// <summary>
		/// 输出日志
		/// </summary>
		public static void Log(ELogType logType, string format, params object[] args)
		{
			if (_callback != null)
			{
				string message = string.Format(format, args);
				_callback.Invoke(logType, message);
			}
		}

		/// <summary>
		/// 输出日志
		/// </summary>
		public static void Log(ELogType logType, string message)
		{
			if (_callback != null)
			{
				_callback.Invoke(logType, message);
			}
		}
	}
}