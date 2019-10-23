using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionEngine.Res;

namespace MotionGame
{
	/// <summary>
	/// GameObject对象池
	/// </summary>
	public class AssetObjectPool
	{
		/// <summary>
		/// 池子
		/// </summary>
		private readonly Stack<GameObject> _pool;

		/// <summary>
		/// 对象资源类
		/// </summary>
		private AssetObject _asset;

		/// <summary>
		/// 对象池Root
		/// </summary>
		private Transform _root;

		/// <summary>
		/// 资源加载完毕回调
		/// </summary>
		private Action<GameObject> _callbacks;


		/// <summary>
		/// 模型资源名称
		/// </summary>
		public string ResName { private set; get; }

		/// <summary>
		/// 对象池容量
		/// </summary>
		public int Capacity { private set; get; }

		/// <summary>
		/// 是否准备完毕
		/// </summary>
		public bool IsPrepare { private set; get; } = false;


		public AssetObjectPool(Transform root, string resName, int capacity)
		{
			_root = root;
			ResName = resName;
			Capacity = capacity;

			// 创建缓存池
			_pool = new Stack<GameObject>(capacity);

			// 创建并加载资源
			_asset = new AssetObject();
			_asset.Load(resName, OnAssetQuey);
		}

		/// <summary>
		/// 当资源加载完毕
		/// </summary>
		private void OnAssetQuey(object assetClass, EAssetResult result)
		{
			if (result == EAssetResult.Failed)
				return;

			_asset.GameObj.SetActive(false);
			_asset.GameObj.transform.SetParent(_root);
			_asset.GameObj.transform.localPosition = Vector3.zero;

			// 创建初始对象
			for (int i = 0; i < Capacity; i++)
			{
				GameObject obj = GameObject.Instantiate(_asset.GameObj) as GameObject;
				Push(obj);
			}

			// 准备完毕
			IsPrepare = true;

			// 最后返回结果
			if (_callbacks != null)
			{
				Delegate[] actions = _callbacks.GetInvocationList();
				for (int i = 0; i < actions.Length; i++)
				{
					var action = (Action<GameObject>)actions[i];
					Pop(action);
				}
				_callbacks = null;
			}
		}

		/// <summary>
		/// 存储一个对象
		/// </summary>
		public void Push(GameObject obj)
		{
			if (obj == null)
				return;

			obj.SetActive(false);
			obj.transform.SetParent(_root);
			obj.transform.localPosition = Vector3.zero;
			_pool.Push(obj);
		}

		/// <summary>
		/// 获取一个对象
		/// </summary>
		public void Pop(Action<GameObject> callback)
		{
			// 如果对象池还没有准备完毕
			if (IsPrepare == false)
			{
				_callbacks += callback;
				return;
			}

			if (_pool.Count > 0)
			{
				GameObject obj = _pool.Pop();
				obj.SetActive(true);
				obj.transform.parent = null;
				callback.Invoke(obj);
			}
			else
			{
				GameObject obj = GameObject.Instantiate(_asset.GameObj);
				obj.SetActive(true);
				callback.Invoke(obj);
			}
		}

		/// <summary>
		/// 销毁对象池
		/// </summary>
		public void Destroy()
		{
			// 卸载资源对象
			if (_asset != null)
			{
				_asset.UnLoad();
				_asset = null;
			}

			// 销毁游戏对象
			foreach (var item in _pool)
			{
				GameObject.Destroy(item);
			}
			_pool.Clear();

			// 清空回调
			_callbacks = null;
		}
	}
}