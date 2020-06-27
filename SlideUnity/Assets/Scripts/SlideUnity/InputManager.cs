using System;
using UnityEngine;

namespace SlideUnity
{
	public class InputManager : MonoBehaviour
	{
		private const float MIN_SWIPE_DISTANCE = 128 * 128;

		public Action<Vector2> OnTouchSwiping;
		public Action<Vector2> OnTouchSwiped;

		private bool _isMouseDown;
		private Vector2 _mouseStartPos;
		private Vector2 _touchStartPos;

		private void Update()
		{
			HandleTouchInput();
			HandleMouseInput();
		}

		private void HandleTouchInput()
		{
			if (Input.touchCount < 1)
				return;

			var touch = Input.GetTouch(0);

			switch (touch.phase)
			{
				case TouchPhase.Began:
					_touchStartPos = touch.position;
					return;
				case TouchPhase.Moved:
					var swipingDir = (touch.position - _touchStartPos);
					if (swipingDir.sqrMagnitude > 0)
						OnTouchSwiping?.Invoke(swipingDir.normalized);
					break;
				case TouchPhase.Stationary:
					break;
				case TouchPhase.Ended:
					var swipeDir = (touch.position - _touchStartPos);
					OnTouchSwiped?.Invoke(swipeDir.sqrMagnitude >= MIN_SWIPE_DISTANCE ? swipeDir.normalized : Vector2.zero);
					break;
				case TouchPhase.Canceled:
					break;
				default:
					break;
			}
		}

		private void HandleMouseInput()
		{
			if (Input.GetMouseButtonDown(0))
			{
				_isMouseDown = true;
				_mouseStartPos = Input.mousePosition;
				return;
			}

			if (_isMouseDown)
			{
				if (Input.GetMouseButtonUp(0))
				{
					_isMouseDown = false;
					var swipeDir = ((Vector2)Input.mousePosition - _mouseStartPos);
					OnTouchSwiped?.Invoke(swipeDir.sqrMagnitude >= MIN_SWIPE_DISTANCE ? swipeDir.normalized : Vector2.zero);
				}
				else
				{
					var swipingDir = ((Vector2)Input.mousePosition - _mouseStartPos);
					var swipeMagnitude = swipingDir.sqrMagnitude / MIN_SWIPE_DISTANCE;
					if (swipeMagnitude > 0)
						OnTouchSwiping?.Invoke(swipingDir.normalized * Mathf.Min(1, swipeMagnitude));
				}
			}
		}
	}
}