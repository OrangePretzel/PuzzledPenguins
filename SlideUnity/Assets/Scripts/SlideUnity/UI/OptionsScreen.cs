using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	public class OptionsScreen : UIScreen
	{
		[SerializeField]
		private ToggleSettingsButton ToggleParticlesEnabledButton;

		[SerializeField]
		private Text GameVersionText;

		public override void InitializeScreen()
		{
			base.InitializeScreen();

			ToggleParticlesEnabledButton.SetOnClick(() =>
			{
				GameManager.Settings.ToggleParticlesEnabled();
				ToggleParticlesEnabledButton.UpdateSetting(GameManager.Settings.ParticlesEnabled);
				ToggleParticlesEnabledButton.SetText(GameManager.Settings.ParticlesEnabled ? "Disable Snow" : "Enable Snow");
			});
		}

		public override void ShowScreen()
		{
			base.ShowScreen();

			ToggleParticlesEnabledButton.UpdateSetting(GameManager.Settings.ParticlesEnabled);
			ToggleParticlesEnabledButton.SetText(GameManager.Settings.ParticlesEnabled ? "Disable Snow" : "Enable Snow");

			GameVersionText.text = $"Version {Application.version}";
		}
	}
}