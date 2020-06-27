using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	[RequireComponent(typeof(Button))]
	public class ToggleSettingsButton : ImageTextButton
	{
		private static Color ENABLED_COLOR = Color.white;
		private static Color DISABLED_COLOR = new Color(0.75f, 0.75f, 0.75f, 1);

		public void UpdateSetting(bool enabled)
		{
			Image.color = enabled ? ENABLED_COLOR : DISABLED_COLOR;
			Button.image.color = enabled ? ENABLED_COLOR : DISABLED_COLOR;
		}
	}
}