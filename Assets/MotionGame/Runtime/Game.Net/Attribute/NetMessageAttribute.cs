using System;

namespace MotionGame
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NetMessageAttribute : Attribute
	{
		public ushort MsgType;

		public NetMessageAttribute(ushort msgType)
		{
			MsgType = msgType;
		}
	}
}