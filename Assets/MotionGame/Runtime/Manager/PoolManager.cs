using System;
using System.Collections;
using System.Collections.Generic;
using MotionEngine;
using UnityEngine;

namespace MotionGame
{
	/// <summary>
	/// 模型资源对象池管理器
	/// </summary>
	public sealed class PoolManager
	{
		public static readonly PoolManager Instance = new PoolManager();

		/// <summary>
		/// 资源池的ROOT
		/// </summary>
		private readonly GameObject _root;

		/// <summary>
		/// 资源池对象集合
		/// </summary>
		private readonly Dictionary<string, AssetObjectPool> _pools = new Dictionary<string, AssetObjectPool>();


		private PoolManager()
		{
			_root = new GameObject("[PoolManager]");
			_root.transform.position = Vector3.zero;
			_root.transform.eulerAngles = Vector3.zero;
			UnityEngine.Object.DontDestroyOnLoad(_root);
		}

		/// <summary>
		/// 创建一种模型资源的对象池
		/// </summary>
		public AssetObjectPool CreatePool(string resName, int capacity)
		{
			if (_pools.ContainsKey(resName))
				return _pools[resName];

			AssetObjectPool pool = new AssetObjectPool(_root.transform, resName, capacity);
			_pools.Add(resName, pool);
			return pool;
		}

		/// <summary>
		/// 是否都已经准备完毕
		/// </summary>
		public bool IsAllPrepare()
		{
			foreach (var pair in _pools)
			{
				if (pair.Value.IsPrepare == false)
					return false;
			}
			return true;
		}

		/// <summary>
		/// 销毁所有对象池及其资源
		/// </summary>
		public void DestroyAll()
		{
			foreach (var pair in _pools)
			{
				pair.Value.Destroy();
			}
			_pools.Clear();
		}

		/// <summary>
		/// 获取一个模型资源
		/// </summary>
		public void Spawn(string resName, Action<GameObject> callbcak)
		{
			if (_pools.ContainsKey(resName))
			{
				_pools[resName].Pop(callbcak);
			}
			else
			{
				// 如果不存在是否创建该资源的对象池
				AssetObjectPool pool = CreatePool(resName, 0);
				pool.Pop(callbcak);
			}
		}

		/// <summary>
		/// 回收一个模型资源
		/// </summary>
		public void Restore(string resName, GameObject obj)
		{
			if (obj == null)
				return;

			if (_pools.ContainsKey(resName))
			{
				_pools[resName].Push(obj);
			}
			else
			{
				LogSystem.Log(ELogType.Error, $"Should never get here. ResName is {resName}");
			}
		}
	}
}