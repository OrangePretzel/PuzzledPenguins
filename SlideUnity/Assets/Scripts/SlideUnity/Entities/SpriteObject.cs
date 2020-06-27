using SlideCore;
using SlideCore.Entities;
using SlideUnity.Entities.Behaviours;
using UnityEngine;

namespace SlideUnity.Entities
{
	public abstract class SpriteObject : MonoBehaviour, IPoolableObject
	{
		public SpriteRenderer SpriteRenderer;
		public Animation Animation;

		public int ID;
		public Entity BackingEntity;
		public IEntityBehaviour EntityBehaviour;

		public ActionCallback OnEndOfUpdate = new ActionCallback();
		public ActionCallback OnEntityFinishedAnimation = new ActionCallback();

		public abstract bool HasSettled { get; }

		public void SetGraphic(Sprite sprite) => SpriteRenderer.sprite = sprite;
		public void PlayAnimation(AnimationClip animationClip)
		{
			if (Animation == null) return;

			if (animationClip == null)
			{
				Animation.Stop();
				if (Animation.clip != null) Animation.RemoveClip(Animation.clip);
				return;
			}

			// Stop animation
			Animation.Stop();

			// Swap animation clips
			if (Animation.clip != null) Animation.RemoveClip(Animation.clip);
			Animation.AddClip(animationClip, animationClip.name);

			// Start new animation
			Animation.Play(animationClip.name);
		}

		protected bool _isAnimating;

		public void HandleAnimationComplete()
		{
			_isAnimating = false;
			OnEntityFinishedAnimation.Invoke();
		}

		#region Entity Behaviour

		public void UpdateToEntity(Entity entity)
		{
			EntityBehaviour?.UpdateToEntity(this, entity);
		}

		public void UpdateToResult(UpdateResult updateResult)
		{
			EntityBehaviour?.UpdateToResult(this, updateResult);
		}

		public void UndoToResult(UpdateResult updateResult)
		{
			EntityBehaviour?.UndoToResult(this, updateResult);
		}

		public void CancelAction(PlayerActions playerAction)
		{
			EntityBehaviour?.CancelAction(this, playerAction);
		}

		public void TriggerAction(string actionID)
		{
			EntityBehaviour?.TriggerAction(this, actionID);
		}

		#endregion

		#region IPoolableObject

		protected ObjectPool MyObjectPool;

		public void SetObjectPool(ObjectPool objectPool) => MyObjectPool = objectPool;

		public virtual void ActivateFromPool()
		{
			gameObject.SetActive(true);
		}

		public virtual void ReturnToPool()
		{
			ID = 0;
			BackingEntity = null;
			EntityBehaviour = null;

			SpriteRenderer.color = Color.white;
			SpriteRenderer.transform.localPosition = Vector3.zero;
			PlayAnimation(null);

			OnEndOfUpdate.ClearCallback();
			OnEntityFinishedAnimation.ClearCallback();

			gameObject.SetActive(false);
			MyObjectPool?.ReturnObjectToPool(this);
		}

		#endregion
	}
}
