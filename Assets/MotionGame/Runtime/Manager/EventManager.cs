using MotionEngine;
using MotionEngine.Event;

namespace MotionGame
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public sealed class EventManager : IModule
	{
		public static readonly EventManager Instance = new EventManager();

		/// <summary>
		/// 事件系统
		/// </summary>
		public readonly EventSystem System = new EventSystem();


		private EventManager()
		{
		}
		public void Awake()
		{
		}
		public void Start()
		{
		}
		public void Update()
		{
		}
		public void LateUpdate()
		{
		}
		public void OnGUI()
		{
			Engine.GUILable($"Listener total count : {System.GetAllListenerCount()}");
		}

		/// <summary>
		/// 添加监听
		/// </summary>
		public void AddListener(string eventTag, System.Action<IEventMessage> listener)
		{
			System.AddListener(eventTag, listener);
		}

		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener(string eventTag, System.Action<IEventMessage> listener)
		{
			System.RemoveListener(eventTag, listener);
		}

		/// <summary>
		/// 发送事件
		/// </summary>
		public void Send(string eventTag, IEventMessage message)
		{
			System.Broadcast(eventTag, message);
		}
	}
}