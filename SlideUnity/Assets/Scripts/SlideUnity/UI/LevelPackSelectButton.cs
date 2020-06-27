using SlideCore.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class LevelPackSelectButton : MonoBehaviour
	{
		[SerializeField]
		private Button Button;
		[SerializeField]
		private Image ButtonImage;
		[SerializeField]
		private Text PackNameText;

		[SerializeField]
		private Text SolvedProgressText;
		[SerializeField]
		private Text SushiProgressText;
		[SerializeField]
		private Text MovesProgressText;


		private bool _isButtonDataDirty = false;
		private LevelPack _levelPack;
		public LevelPack LevelPack
		{
			get { return _levelPack; }
			set
			{
				_isButtonDataDirty = true;
				_levelPack = value;
			}
		}

		private void Update()
		{
			if (!_isButtonDataDirty || _levelPack == null) return;
			UpdateButton();
			_isButtonDataDirty = false;
		}

		public void UpdateButton()
		{
			name = $"Level Pack Select Button ({_levelPack.DisplayName})";
			PackNameText.text = _levelPack.DisplayName;

			Button.interactable = !_levelPack.IsLocked();

			float totalLevels = _levelPack.TotalLevelCount;
			SolvedProgressText.text = $"{Mathf.RoundToInt((_levelPack.SolvedLevelCount / totalLevels) * 100)} %";
			SushiProgressText.text = $"{Mathf.RoundToInt((_levelPack.SolvedWithSushiLevelCount / totalLevels) * 100)} %";
			MovesProgressText.text = $"{Mathf.RoundToInt((_levelPack.SolvedWithMovesLevelCount / totalLevels) * 100)} %";

			ButtonImage.sprite = _levelPack.IsSolved() ? GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_Solved : GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_Unsolved;
		}

		public void SelectPack()
		{
			GameManager.GotoLevelSelectMenu(_levelPack);
		}

		public void ShowButton()
		{
			gameObject.SetActive(true);
		}

		public void HideButton()
		{
			gameObject.SetActive(false);
		}
	}
}