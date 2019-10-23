using System;
using System.ComponentModel;
using Google.Protobuf;

namespace MotionGame
{
	/// <summary>
	/// Protobuf帮助类
	/// NOTE：这里可以灵活的替换为任何Protobuf库
	/// </summary>
	public static class ProtobufHelper
	{
		public static byte[] Encode(System.Object protoObj)
		{
			Google.Protobuf.IMessage message = (Google.Protobuf.IMessage)protoObj;
			return message.ToByteArray();
		}

		public static System.Object Decode(System.Type msgType, byte[] bodyData)
		{
			object message = Activator.CreateInstance(msgType);
			((Google.Protobuf.IMessage)message).MergeFrom(bodyData, 0, bodyData.Length);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize != null)
				iSupportInitialize.EndInit();
			return message;
		}

		public static System.Object Decode(object hotfixMsg, byte[] bodyData)
		{
			((Google.Protobuf.IMessage)hotfixMsg).MergeFrom(bodyData, 0, bodyData.Length);
			ISupportInitialize iSupportInitialize = hotfixMsg as ISupportInitialize;
			if (iSupportInitialize != null)
				iSupportInitialize.EndInit();
			return hotfixMsg;
		}
	}
}