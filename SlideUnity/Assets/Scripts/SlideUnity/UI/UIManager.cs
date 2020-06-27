using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem UISnowParticles;
		[SerializeField]
		private LevelCompleteAnimation LevelCompleteAnimation = null;

		[SerializeField]
		private UIScreen MainMenu_UIScreen = null;
		[SerializeField]
		private UIScreen PauseMenu_UIScreen = null;
		[SerializeField]
		private UIScreen Playing_UIScreen = null;
		[SerializeField]
		private UIScreen LevelPackMenu_UIScreen = null;
		[SerializeField]
		private UIScreen LevelSelectMenu_UIScreen = null;
		[SerializeField]
		private UIScreen OptionsMenu_UIScreen = null;
		[SerializeField]
		private LevelCompleteScreen LevelCompleteMenu_UIScreen = null;
		[SerializeField]
		private UIScreen HelpMenu_UIScreen = null;

		private Dictionary<GameState, UIScreen> _gameStateScreens;
		private UIScreen _currentUIScreen = null;

		private void OnEnable()
		{
			_gameStateScreens = new Dictionary<GameState, UIScreen>()
			{
				{ GameState.MainMenu, MainMenu_UIScreen },
				{ GameState.PauseMenu, PauseMenu_UIScreen },
				{ GameState.Playing, Playing_UIScreen },
				{ GameState.LevelPackMenu, LevelPackMenu_UIScreen },
				{ GameState.LevelSelectMenu, LevelSelectMenu_UIScreen },
				{ GameState.OptionsMenu, OptionsMenu_UIScreen },
				{ GameState.LevelCompleteMenu, LevelCompleteMenu_UIScreen },
				{ GameState.HelpMenu, HelpMenu_UIScreen }
			};
		}

		private void Start()
		{
			foreach (var screen in _gameStateScreens.Values)
				screen.HideScreen();
		}

		public void GotoScreen(GameState gameState)
		{
			_currentUIScreen?.HideScreen();
			if (_gameStateScreens.ContainsKey(gameState))
				_currentUIScreen = _gameStateScreens[gameState];
			else
			{
				Debug.LogError($"No UI screen was registered for game state [{gameState}]");
				_currentUIScreen = _gameStateScreens[GameState.MainMenu];
			}
			_currentUIScreen?.ShowScreen();
		}

		public void ReInitializeMenus()
		{
			foreach (var screen in _gameStateScreens.Values)
				screen.InitializeScreen();
		}

		public void ToggleParticles(bool enableParticles)
		{
			UISnowParticles?.gameObject.SetActive(enableParticles);
		}

		public void PlayLevelCompleteAnimation()
		{
			LevelCompleteAnimation.PlayAnimation();
		}

		public void SetLevelCompleteStars(bool sushiStar, bool movesStar)
		{
			LevelCompleteMenu_UIScreen.SetStarAnimations(sushiStar, movesStar);
		}

		#region UI Hooks

		public void Undo()
		{
			GameManager.Undo();
		}

		public void PauseGame()
		{
			GameManager.GotoState(GameState.PauseMenu);
		}

		public void ResumeGame()
		{
			GameManager.GotoState(GameState.Playing);
		}

		public void GotoLevelPackMenu()
		{
			GameManager.GotoState(GameState.LevelPackMenu);
		}

		public void GotoOptionsMenu()
		{
			GameManager.GotoState(GameState.OptionsMenu);
		}

		public void QuitLevel()
		{
			AnalyticsHelper.SendLevelQuitAnalyticsEvent(GameManager.CurrentLevel);
		}

		public void GotoMainMenu()
		{
			GameManager.GotoState(GameState.MainMenu);
		}

		public void RestartLevel()
		{
			GameManager.RestartLevel();
		}

		public void PreviousLevel()
		{
			GameManager.GotoPreviousLevel();
		}

		public void NextLevel()
		{
			GameManager.GotoNextLevel();
		}

		public void QuitGame()
		{
			// TODO: Implement confirmation
			GameManager.QuitGame();
		}

		public void HardResetProgress()
		{
			// TODO: Implement confirmation
			GameManager.HardResetProgress();
			ReInitializeMenus();
		}

		public void ShowHelpScreen()
		{
			GameManager.GotoState(GameState.HelpMenu);
		}

		#endregion
	}
}