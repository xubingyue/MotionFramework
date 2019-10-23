using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionGame
{
	internal class NetProtoHandler
	{
		private static Dictionary<ushort, Type> _msgTypes = new Dictionary<ushort, Type>();

		public static void RegisterMsgType(ushort msgType, Type protoType)
		{
			// 判断是否重复
			if (_msgTypes.ContainsKey(msgType))
			{
				throw new Exception($"Msg {msgType} class {nameof(protoType)} already exist.");
			}

			_msgTypes.Add(msgType, protoType);
		}

		public static Type Handle(ushort msgType)
		{
			Type type;
			if (_msgTypes.TryGetValue(msgType, out type))
			{
				return type;
			}
			else
			{
				throw new KeyNotFoundException($"Package {msgType} is not define.");
			}
		}
		public static Type TryHandle(ushort msgType)
		{
			Type type;
			_msgTypes.TryGetValue(msgType, out type);
			return type;
		}
	}
}