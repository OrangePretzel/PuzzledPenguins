using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class LevelCompleteAnimation : MonoBehaviour
	{
		public AnimationClip LevelCompleteClip;

		private Image _image;
		private Animation _animation;

		private void OnEnable()
		{
			_image = GetComponent<Image>();
			_animation = GetComponent<Animation>();

			_image.enabled = false;
			_animation.AddClip(LevelCompleteClip, LevelCompleteClip.name);
		}

		public void OnAnimationComplete()
		{
			GameManager.GotoState(GameState.LevelCompleteMenu);
			_image.enabled = false;
		}

		public void PlayAnimation()
		{
			_image.enabled = true;
			_animation.Play(LevelCompleteClip.name);
		}
	}
}
