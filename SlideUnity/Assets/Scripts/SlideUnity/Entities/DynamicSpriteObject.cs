using SlideCore;
using SlideCore.Entities;
using SlideCore.Math;
using SlideUnity.Entities.Behaviours;
using UnityEngine;

namespace SlideUnity.Entities
{
	[RequireComponent(typeof(BumpableObject))]
	public class DynamicSpriteObject : SpriteObject
	{
		[SerializeField]
		protected float EntitySpeed = 10f;

		protected Animator _animator;

		protected BumpableObject _bumpableObject;

		public override bool HasSettled => _hasReachedTarget && !_isAnimating;

		public ActionCallback OnEntityReachedTarget = new ActionCallback();

		protected Vector3 _targetPosition;
		protected float _totalDistance = 0;
		protected bool _hasReachedTarget;

		protected virtual void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_bumpableObject = GetComponent<BumpableObject>();
			_targetPosition = transform.position;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_targetPosition, 0.1f);
		}

		private void Update()
		{
			if (GameManager.Paused)
				return;

			if (!_hasReachedTarget)
			{
				// Handle move while bumping
				_bumpableObject?.StopBump();

				// Move towards target
				MoveTowardsTarget(Time.deltaTime);
				return;
			}
		}

		public void SetTargetPosition(Vector3 position) => SetTargetPosition(position, false, false);
		public void SetTargetPositionInstant(Vector3 position, bool useParticles) => SetTargetPosition(position, true, useParticles);
		private void SetTargetPosition(Vector3 position, bool instant, bool useParticles)
		{
			_targetPosition = position;
			if (useParticles && GameManager.Settings.ParticlesEnabled)
			{
				var poofParticlesStart = GameManager.GetSnowParticleSystem();
				var poofParticlesTarget = GameManager.GetSnowParticleSystem();

				poofParticlesStart.transform.position = transform.position;
				poofParticlesStart.Play();

				poofParticlesTarget.transform.position = _targetPosition;
				poofParticlesTarget.Play();
			}
			if (instant) transform.position = _targetPosition;

			_hasReachedTarget = _targetPosition == transform.position;
			if (!_hasReachedTarget)
				_totalDistance = (_targetPosition - transform.position).magnitude;
			else
				OnEntityReachedTarget.Invoke();
		}

		public void SetDirection(Vector3 moveDir)
		{
			if (_animator == null) return;

			var dir = (
				moveDir.x != 0 ? (moveDir.x > 0 ? Directions.Right : Directions.Left) :
				moveDir.y != 0 ? (moveDir.y > 0 ? Directions.Up : Directions.Down) :
				Directions.None);
			_animator.SetInteger("MoveDir", (int)dir);
		}

		public virtual void MoveTowardsTarget(float deltaTime)
		{
			const float MOVEMENT_THRESHOLD = 0.2f * 0.2f;
			const float MOVEMENT_LIMIT = 0.25f;
			float movementLimit = Mathf.Max(MOVEMENT_LIMIT * _totalDistance, 1.0f);

			var posDiff = _targetPosition - transform.position;
			if (posDiff.sqrMagnitude < MOVEMENT_THRESHOLD)
			{
				transform.position = _targetPosition;
				SetDirection(Vector3.zero);
				_hasReachedTarget = true;
				OnEntityReachedTarget.Invoke();
				return;
			}

			if (posDiff.sqrMagnitude > movementLimit)
				posDiff = posDiff.normalized * movementLimit;
			var posDelta = posDiff * deltaTime;
			transform.position += posDelta * EntitySpeed;
			SetDirection(posDelta);
		}

		public override void ReturnToPool()
		{
			HandleAnimationComplete();

			OnEntityReachedTarget.ClearCallback();

			base.ReturnToPool();
		}

		public void StartBump(Vector2 dir, float time = 0) => _bumpableObject?.StartBump(dir, time);

		public void ResetAnimator(params string[] animationsNames)
		{
			if (_animator == null) return;
			foreach (var animationName in animationsNames)
				_animator.SetBool(animationName, false);
		}

		public void AnimateOnReachTarget(string animationName)
		{
			if (_animator == null) return;

			_isAnimating = true;

			// Handle animation start
			OnEntityReachedTarget.RegisterCallback(() =>
			{
				_animator.SetBool(animationName, true);
			});

			// Handle animation finish
			OnEntityFinishedAnimation.RegisterCallback(() =>
			{
				_isAnimating = false;
				_animator.SetBool(animationName, false);
			});
		}

		public void AnimateToReachTarget(string animationName)
		{
			if (_animator == null) return;

			_isAnimating = true;

			// Handle animation start
			_animator.SetBool(animationName, true);

			// Handle animation finish
			OnEntityFinishedAnimation.RegisterCallback(() =>
			{
				_isAnimating = false;
				_animator.SetBool(animationName, false);
			});
		}
	}
}
