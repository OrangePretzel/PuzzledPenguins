using SlideCore.Levels;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class LevelCompleteScreen : UIScreen
	{
		[SerializeField]
		private Image SolvedStar;
		[SerializeField]
		private Image SushiStar;
		[SerializeField]
		private Image MovesStar;

		[SerializeField]
		private Button NextLevelButton;

		[SerializeField]
		private AnimationCurve StarPopCurve;

		private bool _shouldAnimateSushi = false;
		private bool _shouldAnimateMoves = true;

		public override void ShowScreen()
		{
			UpdateStars();

			base.ShowScreen();

			PlayAnimations();
		}

		public void SetStarAnimations(bool shouldAnimateSushi, bool shouldAnimateMoves)
		{
			_shouldAnimateSushi = shouldAnimateSushi;
			_shouldAnimateMoves = shouldAnimateMoves;
		}

		private void UpdateStars()
		{
			var level = GameManager.CurrentLevel;
			if (level == null) return;

			SolvedStar.sprite = level.Info.IsSolved ?
				GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_StarOn
				: GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_StarOff;
			SushiStar.sprite = level.Info.IsSolvedWithSushi ?
				GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_SushiOn
				: GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_SushiOff;
			MovesStar.sprite = level.Info.IsSolvedWithinMinMoves ?
				GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_MinMovesOn
				: GameManager.GraphicsDictionary.UISprites.LevelCompleteSprites.LevelComplete_MinMovesOff;

			bool isNextLevelUnlocked = false; // Don't actually need this
			NextLevelButton.gameObject.SetActive(LevelManager.HasNextLevel(GameManager.CurrentLevelPack, GameManager.CurrentLevel.Info, ref isNextLevelUnlocked));
		}

		private void PlayAnimations()
		{
			// Animate Solved
			AnimateStar(SolvedStar);

			if (_shouldAnimateSushi)
			{
				// Animate Sushi
				AnimateStar(SushiStar);

				_shouldAnimateSushi = false;
			}

			if (_shouldAnimateMoves)
			{
				// Animate Moves
				AnimateStar(MovesStar);

				_shouldAnimateMoves = false;
			}
		}

		private async void AnimateStar(Image starImage)
		{
			float startTime = Time.time;
			float duration = StarPopCurve.keys[StarPopCurve.length - 1].time;
			var originalScale = starImage.transform.localScale;

			while (true)
			{
				var time = Time.time - startTime;
				if (time > duration) break;

				var currScale = StarPopCurve.Evaluate(time);
				starImage.transform.localScale = new Vector3(originalScale.x * currScale, originalScale.y * currScale, 1);

				await Task.Delay((int)(Time.fixedDeltaTime * 1000));
			}

			starImage.transform.localScale = originalScale;
		}
	}
}