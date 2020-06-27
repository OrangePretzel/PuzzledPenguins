using UnityEngine;

namespace SlideUnity.UI
{
	[RequireComponent(typeof(Canvas))]
	public class UIScreen : MonoBehaviour
	{
		private Canvas _canvas;
		protected Canvas GetCanvas() => _canvas ?? (_canvas = GetComponent<Canvas>());

		public bool IsVisible => GetCanvas().enabled;

		protected void OnEnable()
		{
			InitializeScreen();
		}

		public virtual void InitializeScreen() { }

		public virtual void ShowScreen()
		{
			GetCanvas().enabled = true;
		}

		public virtual void HideScreen()
		{
			GetCanvas().enabled = false;
		}
	}
}
