//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

public class ClearPlayerSetting
{
	[MenuItem("MotionTools/Misc - Clear PlayerSetting")]
	static void ClearPlayerSettingFun()
	{
		PlayerPrefs.DeleteAll();
	}
}