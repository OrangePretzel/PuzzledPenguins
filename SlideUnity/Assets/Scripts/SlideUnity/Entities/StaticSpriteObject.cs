using System.Threading.Tasks;
using UnityEngine;

namespace SlideUnity.Entities
{
	public class StaticSpriteObject : SpriteObject
	{
		[SerializeField]
		private Sprite[] Sprites;

		public override bool HasSettled => true;

		private bool _isAnimated = false;
		private int _spriteIndex = 0;
		private float _animationRate = 0;
		private float _timeSinceLastAnimationUpdate = 0;

		private void Update()
		{
			if (!_isAnimated) return;
			_timeSinceLastAnimationUpdate += Time.deltaTime;
			if (_timeSinceLastAnimationUpdate >= _animationRate)
			{
				UpdateSpriteObject();
				_timeSinceLastAnimationUpdate = 0;
			}
		}

		public void SetSprite(Sprite sprite)
		{
			_isAnimated = false;
			_spriteIndex = 0;
			_animationRate = 0;
			_timeSinceLastAnimationUpdate = 0;

			Sprites = null;
			SpriteRenderer.sprite = sprite;
		}

		public void SetSprite(Sprite[] sprites, float animationRate = 0.1f)
		{
			_isAnimated = sprites.Length > 1;
			_spriteIndex = 0;
			_animationRate = animationRate;
			_timeSinceLastAnimationUpdate = 0;

			Sprites = sprites;
			SpriteRenderer.sprite = Sprites[_spriteIndex];
		}

		public void UpdateSpriteObject()
		{
			if (!_isAnimated) return;

			_spriteIndex = (_spriteIndex + 1) % Sprites.Length;
			SpriteRenderer.sprite = Sprites[_spriteIndex];
		}

		#region IPoolable

		public override void ReturnToPool()
		{
			_isAnimated = false;
			_spriteIndex = 0;
			_animationRate = 0;
			_timeSinceLastAnimationUpdate = 0;

			Sprites = null;

			base.ReturnToPool();
		}

		#endregion
	}
}
