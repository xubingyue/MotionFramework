using System;
using System.Collections;
using System.Collections.Generic;
using MotionEngine;
using MotionEngine.IO;
using MotionEngine.Res;

namespace MotionGame
{
	/// <summary>
	/// 配表数据类
	/// </summary>
	public abstract class ConfigTab
	{
		public int Id { get; protected set; }
		public abstract void ReadByte(ByteBuffer byteBuf);
	}

	/// <summary>
	/// 配表资源类
	/// </summary>
	public abstract class AssetConfig : AssetByte
	{
		/// <summary>
		/// 配表数据集合
		/// </summary>
		protected readonly Dictionary<int, ConfigTab> _tabs = new Dictionary<int, ConfigTab>();

		public AssetConfig()
		{
		}
		protected override bool OnPrepare(UnityEngine.Object asset, bool result)
		{
			if (base.OnPrepare(asset, result) == false)
				return false;

			try
			{
				// 解析数据
				ParseDataInternal();
			}
			catch (Exception ex)
			{
				LogSystem.Log(ELogType.Error, $"Failed to parse config {ResName}. Error : {ex.ToString()}");
				return false;
			}

			// 注意：为了节省内存这里立即释放了资源
			UnLoad();

			return true;
		}

		/// <summary>
		/// 序列化表格的接口
		/// </summary>
		protected abstract ConfigTab ReadTab(ByteBuffer byteBuffer);

		/// <summary>
		/// 解析数据
		/// </summary>
		private void ParseDataInternal()
		{
			ByteBuffer bb = new ByteBuffer(_bytes);

			int tabLine = 1;
			const int headMarkAndSize = 6;
			while (bb.IsReadable(headMarkAndSize))
			{
				// 检测行标记
				short tabHead = bb.ReadShort();
				if (tabHead != ConfigDefine.TabStreamHead)
				{
					throw new Exception($"Table stream head is invalid. File is {ResName} , tab line is {tabLine}");
				}

				// 检测行大小
				int tabSize = bb.ReadInt();
				if (!bb.IsReadable(tabSize) || tabSize > ConfigDefine.TabStreamMaxLen)
				{
					throw new Exception($"Table stream size is invalid. File is {ResName}, tab line {tabLine}");
				}

				// 读取行内容
				ConfigTab tab = null;
				try
				{
					tab = ReadTab(bb);
				}
				catch (Exception ex)
				{
					throw new Exception($"ReadTab falied. File is {ResName}, tab line {tabLine}. Error : {ex.ToString()}");
				}

				++tabLine;

				// 检测是否重复
				if (_tabs.ContainsKey(tab.Id))
				{
					throw new Exception($"The tab key is already exist. Type is {this.GetType()}, file is {ResName}, key is { tab.Id}");
				}
				else
				{
					_tabs.Add(tab.Id, tab);
				}
			}
		}

		/// <summary>
		/// 通过外部传进的数据来组织表
		/// </summary>
		public void ParseDataFromCustomData(byte[] bytes)
		{
			_bytes = bytes;
			_tabs.Clear();
			ParseDataInternal();
		}


		/// <summary>
		/// 获取数据，如果不存在报警告
		/// </summary>
		public ConfigTab GetTab(int key)
		{
			if (_tabs.ContainsKey(key))
			{
				return _tabs[key];
			}
			else
			{
				LogSystem.Log(ELogType.Warning, $"Faild to get tab. File is {ResName}, key is {key}");
				return null;
			}
		}

		/// <summary>
		/// 获取数据，如果不存在不会报警告
		/// </summary>
		public bool TryGetTab(int key, out ConfigTab tab)
		{
			return _tabs.TryGetValue(key, out tab);
		}

		/// <summary>
		/// 是否包含Key
		/// </summary>
		public bool ContainsKey(int key)
		{
			return _tabs.ContainsKey(key);
		}

		/// <summary>
		/// 获取所有Key
		/// </summary>
		public List<int> GetKeys()
		{
			List<int> keys = new List<int>();
			foreach (var tab in _tabs)
			{
				keys.Add(tab.Key);
			}
			return keys;
		}
	}
}