
namespace UnityEngine
{
	public static class UnityEngineObjectExtention
	{
		public static bool IsDestroyed(this UnityEngine.Object o)
		{
			return o == null;
		}
	}
}