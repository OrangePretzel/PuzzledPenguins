using UnityEngine;
using UnityEngine.UI;

namespace SlideUnity.UI
{
	[RequireComponent(typeof(Button))]
	public class ImageTextButton : MonoBehaviour
	{
		[SerializeField]
		protected Image Image = null;
		[SerializeField]
		protected Text Text = null;

		private Button _button;
		protected Button Button { get { return _button ?? (_button = GetComponent<Button>()); } }

		public void SetText(string text)
		{
			Text.text = text;
		}

		public void SetOnClick(System.Action onClickAction)
		{
			var button = Button;
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => onClickAction());
		}
	}
}