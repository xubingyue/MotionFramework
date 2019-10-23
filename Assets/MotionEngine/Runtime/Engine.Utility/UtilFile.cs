//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace MotionEngine.Utility
{
	public static class UtilFile
	{
		/// <summary>
		/// 获取规范的路径
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 创建文件
		/// 注意：如果存在则删除旧文件
		/// </summary>
		public static void CreateFile(string filePath, byte[] data)
		{
			// 删除旧文件
			if (File.Exists(filePath))
				File.Delete(filePath);

			// 创建目录
			CreateFileDirectory(filePath);

			// 创建新文件
			using (FileStream fs = File.Create(filePath))
			{
				fs.Write(data, 0, data.Length);
				fs.Flush();
				fs.Close();
			}
		}

		/// <summary>
		/// 创建文件
		/// 注意：如果存在则删除旧文件
		/// </summary>
		public static void CreateFile(string filePath, string info)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(info);
			CreateFile(filePath, bytes);
		}

		/// <summary>
		/// 创建文件所在的目录
		/// </summary>
		/// <param name="filePath">文件路径</param>
		public static void CreateFileDirectory(string filePath)
		{
			// If the destination directory doesn't exist, create it.
			string destDirectory = Path.GetDirectoryName(filePath);
			if (Directory.Exists(destDirectory) == false)
				Directory.CreateDirectory(destDirectory);
		}
	}
}