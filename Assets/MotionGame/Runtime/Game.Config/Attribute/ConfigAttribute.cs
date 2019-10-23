using System;

namespace MotionGame
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute : Attribute
	{
		public string CfgType;

		public ConfigAttribute(string cfgType)
		{
			CfgType = cfgType;
		}
	}
}