using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MotionEngine;
using MotionEngine.Net;

namespace MotionGame
{
	/// <summary>
	/// 网络管理器
	/// </summary>
	public sealed class NetManager : IModule
	{
		public static readonly NetManager Instance = new NetManager();

		/// <summary>
		/// 网络包解析器类型
		/// </summary>
		private Type _packageParseType;

		/// <summary>
		/// 服务端
		/// </summary>
		private TServer _server;

		/// <summary>
		/// 通信频道
		/// </summary>
		private TChannel _channel;

		/// <summary>
		/// 当前的网络状态
		/// </summary>
		public ENetworkState State { private set; get; } = ENetworkState.Disconnect;

		/// <summary>
		/// Mono层网络消息接收回调
		/// </summary>
		public Action<NetReceivePackage> MonoProtoCallback;

		/// <summary>
		/// 热更层网络消息接收回调
		/// </summary>
		public Action<NetReceivePackage> HotfixProtoCallback;


		private NetManager()
		{
		}
		public void Awake()
		{
			if (_packageParseType == null)
				throw new Exception("PackageParseType is null");
		}
		public void Start()
		{
			_server = new TServer();
			_server.Start(true, _packageParseType);
		}
		public void Update()
		{
			if (_server != null)
				_server.Update();

			UpdatePickMsg();
			UpdateNetworkState();
		}
		public void LateUpdate()
		{
		}
		public void OnGUI()
		{
			Engine.GUILable($"Network state : {State}");
		}

		private void UpdatePickMsg()
		{
			if (_channel != null)
			{
				NetReceivePackage package = (NetReceivePackage)_channel.PickMsg();
				if (package != null)
				{
					if (package.ProtoObj != null)
						MonoProtoCallback.Invoke(package);

					if (package.ProtoBodyData != null)
						HotfixProtoCallback.Invoke(package);
				}
			}
		}
		private void UpdateNetworkState()
		{
			if (State == ENetworkState.Connected)
			{
				if (_channel != null && _channel.IsConnected() == false)
				{
					State = ENetworkState.Disconnect;
					LogSystem.Log(ELogType.Warning, "Server disconnect.");
				}
			}
		}

		/// <summary>
		/// 设置解析器类型
		/// </summary>
		public void SetPackageParseType(Type parseType)
		{
			_packageParseType = parseType;
		}

		/// <summary>
		/// 连接服务器
		/// </summary>
		public void ConnectServer(string host, int port)
		{
			if (State == ENetworkState.Disconnect)
			{
				State = ENetworkState.Connecting;
				IPEndPoint remote = new IPEndPoint(IPAddress.Parse(host), port);
				_server.ConnectAsync(remote, OnConnectServer);
			}
		}
		private void OnConnectServer(TChannel channel, SocketError error)
		{
			LogSystem.Log(ELogType.Log, $"Server connect result : {error}");
			if (error == SocketError.Success)
			{
				_channel = channel;
				State = ENetworkState.Connected;
			}
			else
			{
				State = ENetworkState.Disconnect;
			}
		}

		/// <summary>
		/// 断开连接
		/// </summary>
		public void DisconnectServer()
		{
			State = ENetworkState.Disconnect;
			if (_channel != null)
			{
				_server.ReleaseChannel(_channel);
				_channel = null;
			}
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		public void SendMsg(ushort msgID, System.Object protoObj)
		{
			if (State != ENetworkState.Connected)
			{
				LogSystem.Log(ELogType.Warning, "Network is not connected.");
				return;
			}

			NetSendPackage package = new NetSendPackage();
			package.Type = msgID;
			package.ProtoObj = protoObj;

			if (_channel != null)
				_channel.SendMsg(package);
		}
	}
}