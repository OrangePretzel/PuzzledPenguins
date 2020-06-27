using System.Threading.Tasks;
using UnityEngine;

namespace SlideUnity
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleObject : MonoBehaviour, IPoolableObject
	{
		public ParticleSystem ParticleSystem { get; private set; }

		private void OnEnable()
		{
			ParticleSystem = GetComponent<ParticleSystem>();
		}

		public async void Play()
		{
			ParticleSystem.Play();
			await Task.Delay((int)(ParticleSystem.main.duration * 1000));
			ReturnToPool();
		}

		#region IPoolableObject

		protected ObjectPool MyObjectPool;

		public void SetObjectPool(ObjectPool objectPool) => MyObjectPool = objectPool;

		public virtual void ActivateFromPool()
		{
			gameObject.SetActive(true);
		}

		public virtual void ReturnToPool()
		{
			gameObject.SetActive(false);
			MyObjectPool?.ReturnObjectToPool(this);
		}

		#endregion
	}
}