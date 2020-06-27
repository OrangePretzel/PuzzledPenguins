using UnityEngine;

namespace SlideUnity.UI
{
	public class TutorialScreen : UIScreen
	{
		[SerializeField]
		private GameObject TutorialHand;
		[SerializeField]
		private Animation TutorialAnimation;

		private void FixedUpdate()
		{
			if (!TutorialAnimation.isPlaying) TutorialAnimation.Play();
		}

		public override void ShowScreen()
		{
			base.ShowScreen();
			TutorialHand.SetActive(true);
		}

		public override void HideScreen()
		{
			base.HideScreen();
			TutorialHand.SetActive(false);
		}
	}
}