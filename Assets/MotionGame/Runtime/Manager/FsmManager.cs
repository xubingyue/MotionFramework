using MotionEngine;
using MotionEngine.AI;

namespace MotionGame
{
	/// <summary>
	/// 状态机管理器
	/// </summary>
	public sealed class FsmManager : IModule
	{
		public static readonly FsmManager Instance = new FsmManager();

		/// <summary>
		/// 状态机系统
		/// </summary>
		public readonly FsmSystem System = new FsmSystem();

		/// <summary>
		/// 初始运行状态
		/// </summary>
		private int _runState;

		/// <summary>
		/// 全局状态
		/// </summary>
		private int _globalState;


		private FsmManager()
		{
		}
		public void Awake()
		{
		}
		public void Start()
		{
			System.Run(_runState, _globalState);
		}
		public void Update()
		{
			System.Update();
		}
		public void LateUpdate()
		{
		}
		public void OnGUI()
		{
			Engine.GUILable($"FSM : {System.RunStateType}");
		}

		/// <summary>
		/// 改变状态机状态
		/// </summary>
		public void ChangeState(int stateType)
		{
			System.ChangeState(stateType);
		}

		/// <summary>
		/// 设置初始状态和全局状态
		/// </summary>
		/// <param name="runStateType">初始状态</param>
		/// <param name="globalStateType">全局状态</param>
		public void SetDefaultState(int runStateType, int globalStateType)
		{
			_runState = runStateType;
			_globalState = globalStateType;
		}
	}
}