using SlideCore;
using SlideUnity.UI;
using UnityEngine;

namespace SlideUnity.Game
{
	public class TutorialController : GameController
	{
		[SerializeField]
		private TutorialScreen TutorialScreen;

		public override void EnableController()
		{
			base.EnableController();
			EnableTutorialOverlay();
		}

		public override void DisableController()
		{
			DisableTutorialOverlay();
			base.DisableController();
		}

		protected override void HandlePlayerInput(PlayerActions action)
		{
			if (GameManager.CurrentLevel?.Move == 0 && TutorialScreen.IsVisible)
			{
				if (action != PlayerActions.MoveDown) return; // The only accepted first move is down
				DisableTutorialOverlay();
			}

			base.HandlePlayerInput(action);
		}

		private void EnableTutorialOverlay()
		{
			if (GameManager.CurrentLevel?.Move == 0)
				TutorialScreen.ShowScreen();
		}

		private void DisableTutorialOverlay()
		{
			TutorialScreen.HideScreen();
		}
	}
}
