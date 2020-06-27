using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlideUnity
{
	public interface IPoolableObject
	{
		void SetObjectPool(ObjectPool objectPool);
		void ActivateFromPool();
		void ReturnToPool();
	}

	public class ObjectPool : MonoBehaviour
	{
		private const int ALLOCATION_INCREMENT = 5;

		public string PoolName;

		private Queue<IPoolableObject> _objectPool = new Queue<IPoolableObject>();

		private Func<IPoolableObject> _createPoolableFunc;

		public void SetAllocationFunction(Func<IPoolableObject> allocationFunction)
		{
			_createPoolableFunc = allocationFunction;
		}

		public void AllocateObjects(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var obj = _createPoolableFunc?.Invoke();
				if (obj == null)
				{
					throw new Exception($"Allocation function for [{gameObject.name}] failed to return a valid object");
				}

				obj.SetObjectPool(this);
				obj.ReturnToPool();
			}
		}

		public IPoolableObject GetObjectFromPool()
		{
			if (_objectPool.Count < 1)
				AllocateObjects(ALLOCATION_INCREMENT);

			var obj = _objectPool.Dequeue();
			obj.ActivateFromPool();
			return obj;
		}

		public void ReturnObjectToPool(IPoolableObject poolableObject)
		{
			_objectPool.Enqueue(poolableObject);
		}

#if UNITY_EDITOR

		private void OnEnable()
		{
			PoolName = gameObject.name;
		}

		private void FixedUpdate()
		{
			gameObject.name = $"{PoolName} ({_objectPool.Count})";
		}

#endif
	}
}