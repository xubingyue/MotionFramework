//--------------------------------------------------
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Reflection;

namespace MotionEngine.Utility
{
	public static class UtilReflection
	{
		/// <summary>
		/// 获取私有字段的值
		/// </summary>
		public static T GetPrivateField<T>(this object instance, string fieldName)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			FieldInfo field = type.GetField(fieldName, flag);
			return (T)field.GetValue(instance);
		}

		/// <summary>
		/// 设置私有字段的值
		/// </summary>
		public static void SetPrivateField(this object instance, string fieldName, object value)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			FieldInfo field = type.GetField(fieldName, flag);
			field.SetValue(instance, value);
		}


		/// <summary>
		/// 获取私有属性的值
		/// </summary>
		public static T GetPrivateProperty<T>(this object instance, string propertyName)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			PropertyInfo field = type.GetProperty(propertyName, flag);
			return (T)field.GetValue(instance, null);
		}

		/// <summary>
		/// 设置私有属性的值
		/// </summary>
		public static void SetPrivateProperty(this object instance, string propertyName, object value)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			PropertyInfo field = type.GetProperty(propertyName, flag);
			field.SetValue(instance, value, null);
		}


		/// <summary>
		/// 调用私有方法（带返回值）
		/// </summary>
		public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			MethodInfo method = type.GetMethod(name, flag);
			return (T)method.Invoke(instance, param);
		}

		/// <summary>
		/// 调用私有方法
		/// </summary>
		public static  void CallPrivateMethod(this object instance, string name, params object[] param)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			MethodInfo method = type.GetMethod(name, flag);
			method.Invoke(instance, param);
		}
	}
}