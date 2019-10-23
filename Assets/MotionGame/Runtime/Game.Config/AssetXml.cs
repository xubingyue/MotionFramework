using System;
using System.Security;
using Mono.Xml;
using MotionEngine;
using MotionEngine.Res;

namespace MotionGame
{
	public abstract class AssetXml : AssetText
	{
		protected SecurityElement _xml;

		public AssetXml()
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			SecurityParser sp = new SecurityParser();
			sp.LoadXml(Text.text);
			_xml = sp.ToXml();

			if (_xml == null)
			{
				LogSystem.Log(ELogType.Error, $"SecurityParser.LoadXml failed. {ResName}");
				return false;
			}

			try
			{
				// 解析数据
				ParseData();
			}
			catch (Exception ex)
			{
				LogSystem.Log(ELogType.Error, $"Failed to parse xml {ResName}. Exception : {ex.ToString()}");
				return false;
			}

			// 注意：为了节省内存这里立即释放了资源
			UnLoad();

			return true;
		}

		/// <summary>
		/// 序列化数据的接口
		/// </summary>
		protected abstract void ParseData();
	}
}