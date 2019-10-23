//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionEngine
{
	/// <summary>
	/// 模块接口
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// 当模块被注册的时候被调用，仅被执行一次
		/// </summary>
		void Awake();

		/// <summary>
		/// 当第一次Update之前被调用，仅被执行一次
		/// </summary>
		void Start();

		/// <summary>
		/// Update方法
		/// </summary>
		void Update();

		/// <summary>
		/// 在所有Update执行完毕后被调用
		/// </summary>
		void LateUpdate();

		/// <summary>
		/// GUI绘制
		/// </summary>
		void OnGUI();
	}
}