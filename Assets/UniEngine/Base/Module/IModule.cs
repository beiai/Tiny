namespace UniEngine.Base.Module
{
	/// <summary>
	/// 游戏框架模块抽象类。
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// 获取游戏框架模块优先级。
		/// </summary>
		/// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
		int Priority { get; }

		/// <summary>
		/// 框架创建时调用
		/// </summary>
		void OnCreate();

		/// <summary>
		/// 游戏框架模块轮询。
		/// </summary>
		void Update();

		/// <summary>
		/// 关闭并清理游戏框架模块。
		/// </summary>
		void Shutdown();
	}
}