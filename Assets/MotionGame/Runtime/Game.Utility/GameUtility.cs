using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MotionGame
{
	public static class GameUtility
	{
		/// <summary>
		/// 数值组比较
		/// </summary>
		public static bool ArraysEqual<T>(T[] a1, T[] a2)
		{
			if (ReferenceEquals(a1, a2))
				return true;

			if (a1 == null || a2 == null)
				return false;

			if (a1.Length != a2.Length)
				return false;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			for (int i = 0; i < a1.Length; i++)
			{
				if (!comparer.Equals(a1[i], a2[i])) return false;
			}
			return true;
		}

		#region Marshal
		/// <summary>
		/// 结构体序列化为字节数组
		/// </summary>
		public static byte[] StructToBytes(object structObject)
		{
			int structSize = Marshal.SizeOf(structObject);
			byte[] bytes = new byte[structSize];
			GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			IntPtr bytesPtr = bytesHandle.AddrOfPinnedObject();
			Marshal.StructureToPtr(structObject, bytesPtr, false);
			if (bytesHandle.IsAllocated)
				bytesHandle.Free();
			return bytes;
		}

		/// <summary>
		/// 字节数组序列化为结构体
		/// </summary>
		public static T BytesToStruct<T>(byte[] bytes, int offset = 0)
		{
			GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			IntPtr bytesPtr = (IntPtr)(bytesHandle.AddrOfPinnedObject().ToInt32() + offset);
			T structObject = (T)Marshal.PtrToStructure(bytesPtr, typeof(T));
			if (bytesHandle.IsAllocated)
				bytesHandle.Free();
			return structObject;
		}
		#endregion

		#region 类型检测
		/// <summary>
		/// 检测对象是否是可空类型
		/// </summary>
		public static bool IsNullable<T>(T t) { return false; }

		/// <summary>
		/// 检测对象是否是可空类型
		/// </summary>
		public static bool IsNullable<T>(T? t) where T : struct { return true; }
		#endregion

		#region 网络检测
		/// <summary>
		/// 检测当前网络状态
		/// </summary>
		/// <returns>返回TRUE网络已连接，返回FALSE网络不可用</returns>
		public static bool CheckNetworkState()
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}

		/// <summary>
		/// 检测当前网络是否为WIFI网络
		/// </summary>
		public static bool IsWIFI()
		{
			return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}

		/// <summary>
		/// 检测当前网络是否为移动网络
		/// </summary>
		public static bool IsMobileState()
		{
			return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
		}
		#endregion

		#region HTML
		/// <summary>
		/// 获取HTML中纯文本
		/// </summary>
		public static string GetSimpleText(string html)
		{
			html = Regex.Replace(html, @"<\/*[^<>]*>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			html = html.Replace("&nbsp;", "");
			return html;
		}

		/// <summary>
		/// 是否是HTML文本
		/// </summary>
		public static bool IsHtmlText(string text)
		{
			return Regex.Match(text, @"<\/*[^<>]*>").Success;
		}

		/// <summary>
		/// 是否含有HTML标签
		/// </summary>
		/// <param name="matchFlag">存储匹配到的内容[idx1,len1, idx2,len2, idx3,len3, ......]</param>
		/// <param name="matchCount">存储匹配到的文本个数</param>
		public static bool IsHtmlText(string text, ref List<int> matchFlag, ref int matchCount)
		{
			var matches = Regex.Matches(text, @"<\/*[^<>]*>");
			if (matches.Count > 0)
			{
				matchCount = 0;
				foreach (Match m in matches)
				{
					matchFlag.Add(m.Index);
					matchFlag.Add(m.Length);
					matchCount += m.Length;
				}
			}
			return matches.Count > 0;
		}
		#endregion
	}
}