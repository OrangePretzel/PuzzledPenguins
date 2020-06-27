using UnityEngine;

namespace SlideUnity.Entities
{
	public class BumpableObject : MonoBehaviour
	{
		// TODO: Consider making these static
		[SerializeField]
		private AnimationCurve BumpCurve = null;
		[SerializeField]
		private float BumpDuration = 0.25f;

		[SerializeField]
		private SpriteRenderer SpriteRendererToBump = null;

		private bool _isBumping = false;
		private float _bumpTime = 0;
		private Vector3 _bumpDir;

		public void StartBump(Vector3 dir, float time = 0)
		{
			if (dir.sqrMagnitude == 0) return;

			_isBumping = true;
			_bumpTime = time * BumpDuration;
			_bumpDir = dir;
		}

		public void StopBump()
		{
			if (!_isBumping) return;

			_isBumping = false;
			_bumpTime = 0;
			_bumpDir = Vector3.zero;
			SpriteRendererToBump.transform.localPosition = Vector3.zero;
		}

		private void LateUpdate()
		{
			if (GameManager.Paused || !_isBumping) return;

			_bumpTime += Time.deltaTime;
			if (_bumpTime >= BumpDuration)
				StopBump();
			else
				SpriteRendererToBump.transform.localPosition = _bumpDir * BumpCurve.Evaluate(_bumpTime / BumpDuration);
		}
	}
}
