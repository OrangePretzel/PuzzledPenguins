using SlideCore.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class MainGameScreen : UIScreen
	{
		private static Color STAR_NOT_EARNED_COLOR = new Color(1, 1, 1, 0.25f);

		[SerializeField]
		private Text LevelTitleText = null;

		[SerializeField]
		private Button UndoButton = null;

		[SerializeField]
		private Button PreviousLevelButton = null;
		[SerializeField]
		private Button NextLevelButton = null;

		[SerializeField]
		private Image SushiStarImage = null;
		[SerializeField]
		private Image MovesStarImage = null;

		[SerializeField]
		private Text SushiCounter = null;
		[SerializeField]
		private Text MoveCounter = null;

		public override void ShowScreen()
		{
			UpdateUI();

			bool isNextLevelUnlocked = false;
			bool hasNextLevel = LevelManager.HasNextLevel(GameManager.CurrentLevelPack, GameManager.CurrentLevel.Info, ref isNextLevelUnlocked);
			bool hasPrevLevel = LevelManager.HasPreviousLevel(GameManager.CurrentLevelPack, GameManager.CurrentLevel.Info);
			PreviousLevelButton.interactable = hasPrevLevel;
			NextLevelButton.interactable = isNextLevelUnlocked;

			base.ShowScreen();
		}

		private void FixedUpdate()
		{
			// TODO: Find a way to only update UI on changes
			UpdateUI();
		}

		private void UpdateUI()
		{
			var currentLevel = GameManager.CurrentLevel;
			if (currentLevel != null)
			{
				UndoButton.interactable = currentLevel.CanUndo;
				MoveCounter.text = $"{currentLevel.Move.ToString()} / {currentLevel.TargetMoves}";
				SushiCounter.text = $"{currentLevel.CollectedSushiCount} / {currentLevel.TotalSushiCount}";

				SushiStarImage.color = currentLevel.HasCollectedAllSushi ? Color.white : STAR_NOT_EARNED_COLOR;
				MovesStarImage.color = currentLevel.IsWithinTargetMoves ? Color.white : STAR_NOT_EARNED_COLOR;

				LevelTitleText.text = currentLevel.Info.DisplayName;
			}
		}
	}
}
