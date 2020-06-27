using SlideCore;
using SlideCore.Levels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SlideUnity.UI
{
	public class LevelSelectScreen : UIScreen
	{
		[SerializeField]
		private Object LevelSelectButtonPrefab = null;
		[SerializeField]
		private GameObject _levelSelectButtonsLayout = null;

		private Dictionary<Vector2Int, LevelSelectButton> _levelSelectButtons = new Dictionary<Vector2Int, LevelSelectButton>();

		private List<LevelSelectButton> _buttons = new List<LevelSelectButton>();

		public override void InitializeScreen()
		{
			base.InitializeScreen();

			_buttons = _levelSelectButtonsLayout.GetComponentsInChildren<LevelSelectButton>().ToList();
			InitLevelButtons(_buttons, 25 - _buttons.Count);
		}

		public override void ShowScreen()
		{
			// TODO: There is a slight visible UI switch in button names. Find a way to minimize this
			LoadLevelButtons(GameManager.CurrentLevelPack.Levels.ToList());

			base.ShowScreen();
		}

		public void LoadLevelButtons(List<Level.LevelInfo> levelInfos)
		{
			if (_buttons.Count < 25)
				InitLevelButtons(_buttons, 25 - _buttons.Count);
			LayoutButtons(_buttons, levelInfos);
		}

		private void InitLevelButtons(List<LevelSelectButton> buttons, int count)
		{
			for (int i = 0; i < count; i++)
			{
				var buttonObj = (GameObject)Instantiate(LevelSelectButtonPrefab, _levelSelectButtonsLayout.transform);
				var button = buttonObj.GetComponent<LevelSelectButton>();
				buttons.Add(button);
			}
		}

		private void LayoutButtons(List<LevelSelectButton> buttons, List<Level.LevelInfo> levelInfos)
		{
			const int BUTTONS_LAYOUT_WIDTH = 5;
			const int BUTTONS_LAYOUT_HEIGHT = 5;

			_levelSelectButtons.Clear();
			int b = 0;
			for (int j = 0; j < BUTTONS_LAYOUT_HEIGHT; j++)
				for (int i = 0; i < BUTTONS_LAYOUT_WIDTH; i++)
				{
					var button = buttons[b++];

					var index = j * BUTTONS_LAYOUT_WIDTH + i;
					if (levelInfos.Count <= index)
					{
						button.HideButton();
					}
					else
					{
						var buttonData = levelInfos[index];
						button.LevelData = buttonData;
						_levelSelectButtons.Add(new Vector2Int(i, j), button);
						button.ShowButton();
					}
				}
		}
	}
}