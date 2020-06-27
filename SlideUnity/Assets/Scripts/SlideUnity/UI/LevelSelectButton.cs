using SlideCore.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(Button))]
	public class LevelSelectButton : MonoBehaviour
	{
		[SerializeField]
		private Image StarStickerImage = null;
		[SerializeField]
		private Image SushiStickerImage = null;
		[SerializeField]
		private Image MovesStickerImage = null;

		private bool _isButtonDataDirty = false;
		private Level.LevelInfo _levelData;
		public Level.LevelInfo LevelData
		{
			get { return _levelData; }
			set
			{
				_isButtonDataDirty = true;
				_levelData = value;
			}
		}

		private Button _button;
		private Image _image;
		private Text _text;

		private void OnEnable()
		{
			_button = GetComponent<Button>();
			_image = GetComponent<Image>();
			_text = GetComponentInChildren<Text>();
		}

		private void Update()
		{
			if (!_isButtonDataDirty || _button == null) return;

			name = $"Level Select Button ({LevelData.DisplayName})";
			_text.text = LevelData.DisplayName;

			_button.interactable = LevelData.Status != Level.LevelInfo.LevelInfoStatus.Locked;
			switch (LevelData.Status)
			{
				case Level.LevelInfo.LevelInfoStatus.Locked:
					_image.sprite = GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_Locked;
					DisabelStickers();
					break;
				case Level.LevelInfo.LevelInfoStatus.Unsolved:
					_image.sprite = GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_Unsolved;
					UpdateStickers();
					break;
				case Level.LevelInfo.LevelInfoStatus.Solved:
				case Level.LevelInfo.LevelInfoStatus.SolvedWithSushi:
				case Level.LevelInfo.LevelInfoStatus.SolvedWithMinMoves:
				case Level.LevelInfo.LevelInfoStatus.FullySolved:
					_image.sprite = GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_Solved;
					UpdateStickers();
					break;
				default:
					throw new System.Exception($"Unsupported Level Status for LevelSelectButton [{LevelData.Status}]");
			}

			_isButtonDataDirty = false;
		}

		private void DisabelStickers()
		{
			StarStickerImage.enabled = false;
			SushiStickerImage.enabled = false;
			MovesStickerImage.enabled = false;
		}

		private void UpdateStickers()
		{
			StarStickerImage.enabled = true;
			SushiStickerImage.enabled = true;
			MovesStickerImage.enabled = true;

			StarStickerImage.sprite = LevelData.IsSolved ?
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_StarOn :
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_StarOff;
			SushiStickerImage.sprite = LevelData.IsSolvedWithSushi ?
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_SushiOn :
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_SushiOff;
			MovesStickerImage.sprite = LevelData.IsSolvedWithinMinMoves ?
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_MinMovesOn :
				GameManager.GraphicsDictionary.UISprites.LevelTileSprites.LevelTile_MinMovesOff;
		}

		public void SelectLevel()
		{
			GameManager.GotoLevel(_levelData);
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