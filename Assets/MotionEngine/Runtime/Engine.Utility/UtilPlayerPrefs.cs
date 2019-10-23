//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionEngine.IO;

namespace MotionEngine.Utility
{
	public static class UtilPlayerPrefs
	{
		// BOOL
		public static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}
		public static bool GetBool(string key, bool defaultValue)
		{
			int result = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
			return result != 0;
		}

		// 枚举
		public static void SetEnum<T>(string key, T value)
		{
			string enumName = value.ToString();
			PlayerPrefs.SetString(key, enumName);
		}
		public static T GetEnum<T>(string key, T defaultValue)
		{
			string enumName = PlayerPrefs.GetString(key, defaultValue.ToString());
			return StringConvert.NameToEnum<T>(enumName);
		}
	}
}