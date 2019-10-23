using System;
using System.Collections;
using System.Collections.Generic;
using MotionEngine.Net;

namespace MotionGame
{
	/// <summary>
	/// 客户端到服务器的消息类
	/// </summary>
	public class NetSendPackage
	{
		//public ushort SIZE;
		public ushort Type;

		public System.Object ProtoObj; //Proto消息对象
	}

	/// <summary>
	/// 服务器到客户端的消息类
	/// </summary>
	public class NetReceivePackage
	{
		//public ushort SIZE;
		public ushort Type;

		public System.Object ProtoObj; //Proto消息对象
		public byte[] ProtoBodyData; //Proto包体数据
	}


	/// <summary>
	/// Protobuf网络消息解析器
	/// </summary>
	public class NetProtoPackageParser : NetPackageParser
	{
		public const int SIZE_BYTE_COUNT = 2; //SIZE字段所占字节数
		public const int SEND_PACKAGE_HEAD_SIZE = 2; //发送消息的包头长度
		public const int RECEIVE_PACKAGE_HEAD_SIZE = 2; //接收消息的包头长度

		public NetProtoPackageParser()
		{
		}
		public override void Dispose()
		{
			base.Dispose();
		}
		public override void Encode(System.Object msg)
		{
			NetSendPackage packet = (NetSendPackage)msg;
			if (packet.ProtoObj == null)
			{
				Channel.HandleError(false, $"NetProtoPackageParser encode fatal. Msg type is {packet.Type}");
				return;
			}

			// 获取包体数据
			byte[] bodyData = ProtobufHelper.Encode(packet.ProtoObj);

			// 写入长度
			int packetSize = SEND_PACKAGE_HEAD_SIZE + bodyData.Length;
			_sendBuffer.WriteUShort((ushort)packetSize);

			// 写入包头
			_sendBuffer.WriteUShort(packet.Type);

			// 写入包体
			_sendBuffer.WriteBytes(bodyData, 0, bodyData.Length);
		}
		public override void Decode(List<System.Object> msgList)
		{
			// 循环解包
			while (true)
			{
				// 如果数据不够一个SIZE
				if (_receiveBuffer.ReadableBytes() < SIZE_BYTE_COUNT)
					break;

				_receiveBuffer.MarkReaderIndex();

				// 读取Package长度
				ushort packageSize = _receiveBuffer.ReadUShort();

				// 如果剩余可读数据小于Package长度
				if (_receiveBuffer.ReadableBytes() < packageSize)
				{
					_receiveBuffer.ResetReaderIndex();
					break; //需要退出读够数据再解包
				}

				// 获取包头信息
				NetReceivePackage package = new NetReceivePackage();
				package.Type = _receiveBuffer.ReadUShort();

				// 如果包体超过最大长度
				int bodySize = packageSize - RECEIVE_PACKAGE_HEAD_SIZE;
				if (bodySize > NetDefine.PackageMaxSize)
				{
					Channel.HandleError(true, $"The package {package.Type} size is exceeds max size.");
					break;
				}

				// 获取包体数据
				byte[] bodyData = _receiveBuffer.ReadBytes(bodySize);

				// 正常解包
				try
				{
					Type classType = NetProtoHandler.TryHandle(package.Type);
					if (classType != null)
					{
						// 非热更协议
						package.ProtoObj = ProtobufHelper.Decode(classType, bodyData);
						if (package.ProtoObj != null)
							msgList.Add(package);
					}
					else
					{
						// 热更协议
						package.ProtoBodyData = bodyData;
						msgList.Add(package);
					}
				}
				catch (Exception ex)
				{
					// 解包异常后继续解包
					Channel.HandleError(false, $"The package {package.Type} decode error : {ex.ToString()}");
				}
			} //while end

			// 注意：将剩余数据移至起始
			_receiveBuffer.DiscardReadBytes();
		}
	}
}