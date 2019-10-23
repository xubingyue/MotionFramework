//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionEngine.IO;

namespace MotionEngine.Net
{
	/// <summary>
	/// 网络消息解析器基类
	/// </summary>
	public abstract class NetPackageParser
	{
		protected readonly ByteBuffer _sendBuffer = new ByteBuffer(NetDefine.ByteBufferSize);
		protected readonly ByteBuffer _receiveBuffer = new ByteBuffer(NetDefine.ByteBufferSize);
		public TChannel Channel { private set; get; }


		public NetPackageParser()
		{	
		}

		/// <summary>
		/// 初始化频道
		/// </summary>
		public void InitChannel(TChannel channel)
		{
			Channel = channel;
		}

		/// <summary>
		/// 释放解析器
		/// </summary>
		public virtual void Dispose()
		{
			_sendBuffer.Clear();
			_receiveBuffer.Clear();
		}

		/// <summary>
		/// 消息编码
		/// </summary>
		/// <param name="msg">需要编码的消息对象</param>
		public abstract void Encode(System.Object msg);

		/// <summary>
		/// 消息解码
		/// </summary>
		/// <param name="msgList">解码成功后的消息对象列表</param>
		public abstract void Decode(List<System.Object> msgList);


		#region 字节缓冲区处理接口
		public void SetReceiveDataSize(int size)
		{
			_receiveBuffer.WriterIndex += size;
		}

		public void ClearReceiveBuffer()
		{
			_receiveBuffer.Clear();
		}
		public byte[] GetReceiveBuffer()
		{
			return _receiveBuffer.Buf;
		}
		public int GetReceiveBufferCapacity()
		{
			return _receiveBuffer.Capacity;
		}
		public int GetReceiveBufferWriterIndex()
		{
			return _receiveBuffer.WriterIndex;
		}
		public int GetReceiveBufferWriteableBytes()
		{
			return _receiveBuffer.WriteableBytes();
		}
		public int GetReceiveBufferReadableBytes()
		{
			return _receiveBuffer.ReadableBytes();
		}

		public void ClearSendBuffer()
		{
			_sendBuffer.Clear();
		}
		public byte[] GetSendBuffer()
		{
			return _sendBuffer.Buf;
		}
		public int GetSendBufferCapacity()
		{
			return _sendBuffer.Capacity;
		}
		public int GetSendBufferWriterIndex()
		{
			return _sendBuffer.WriterIndex;
		}
		public int GetSendBufferWriteableBytes()
		{
			return _sendBuffer.WriteableBytes();
		}
		public int GetSendBufferReadableBytes()
		{
			return _sendBuffer.ReadableBytes();
		}
		#endregion
	}
}