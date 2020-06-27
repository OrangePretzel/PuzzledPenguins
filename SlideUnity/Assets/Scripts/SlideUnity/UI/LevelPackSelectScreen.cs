using SlideCore.Levels;
using System.Collections.Generic;
using UnityEngine;

namespace SlideUnity.UI
{
	public class LevelPackSelectScreen : UIScreen
	{
		[SerializeField]
		private Object LevelPackSelectButtonPrefab = null;

		[SerializeField]
		private RectTransform LevelPackSelectButtonContainer = null;

		private List<LevelPackSelectButton> _buttons = new List<LevelPackSelectButton>();

		public override void InitializeScreen()
		{
			base.InitializeScreen();

			InitializeButtons();
		}

		public override void ShowScreen()
		{
			foreach (var button in _buttons)
			{
				button.UpdateButton();
			}

			base.ShowScreen();
		}

		private void InitializeButtons()
		{
			var levelPacks = LevelManager.LevelPacks;

			_buttons = new List<LevelPackSelectButton>(LevelPackSelectButtonContainer.GetComponentsInChildren<LevelPackSelectButton>());

			// Remove extra buttons
			while (_buttons.Count > levelPacks.Count)
			{
				var extraButton = _buttons[_buttons.Count - 1];
				_buttons.Remove(extraButton);
				Destroy(extraButton.gameObject);
			}

			// Create new buttons
			while (_buttons.Count < levelPacks.Count)
			{
				var newButtonObj = (GameObject)Instantiate(LevelPackSelectButtonPrefab);
				var newButton = newButtonObj.GetComponent<LevelPackSelectButton>();

				newButton.transform.SetParent(LevelPackSelectButtonContainer, true);
				newButton.transform.localPosition = Vector3.zero;
				newButton.transform.localScale = Vector3.one;

				_buttons.Add(newButton);
			}

			// Setup buttons foreach pack
			for (int i = 0; i < levelPacks.Count; i++)
			{
				var button = _buttons[i];
				var pack = levelPacks[i];

				button.LevelPack = pack;
			}
		}
	}
}