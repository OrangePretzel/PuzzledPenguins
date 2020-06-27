using SlideCore;
using UnityEngine;

namespace SlideUnity.Game
{
	public class GameController : MonoBehaviour
	{
		protected PlayerActions _queuedAction = PlayerActions.None;
		protected PlayerActions _touchAction = PlayerActions.None;

		private void Start()
		{
			SetupCallbacks();
		}

		public virtual void EnableController()
		{
			gameObject.SetActive(true);
		}

		public virtual void DisableController()
		{
			gameObject.SetActive(false);
		}

		private void SetupCallbacks()
		{
			var inputManager = GameManager.GetInputManager();
			inputManager.OnTouchSwiped += HandleTouchSwiped;
		}

		private void Update()
		{
			if (GameManager.GameState != GameState.Playing) return;

			var action = GetPlayerAction();
			HandlePlayerInput(action);
		}

		private PlayerActions GetPlayerAction()
		{
			if (_touchAction != PlayerActions.None)
			{
				var action = _touchAction;
				_touchAction = PlayerActions.None;
				return action;
			}

			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
				return PlayerActions.MoveLeft;
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
				return PlayerActions.MoveRight;
			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
				return PlayerActions.MoveUp;
			if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
				return PlayerActions.MoveDown;
			if (Input.GetKeyDown(KeyCode.R))
				return PlayerActions.Undo;

			return PlayerActions.None;
		}

		protected virtual void HandlePlayerInput(PlayerActions action)
		{
			if (action != PlayerActions.None)
				_queuedAction = _queuedAction == PlayerActions.None ? action : PlayerActions.None;

			if (_queuedAction == PlayerActions.Undo)
			{
				GameManager.Undo();
				_queuedAction = PlayerActions.None;
				return;
			}
		}

		#region IGameController

		public void Reset()
		{
			_touchAction = PlayerActions.None;
			_queuedAction = PlayerActions.None;
		}

		public void ClearNextAction() => _queuedAction = PlayerActions.None;
		public PlayerActions GetNextPlayerAction() => _queuedAction;

		#endregion

		#region Player Input

		private void HandleTouchSwiped(Vector2 swipeDirection)
		{
			if (GameManager.GameState != GameState.Playing) return;

			var calculatedSwipeDir = GetSwipeDir(swipeDirection);
			_touchAction = DirectionToPlayerAction(calculatedSwipeDir);

			// TODO: Make dynamic objects react to the potential move
			//if (_touchAction == PlayerActions.None || _touchAction == PlayerActions.Undo)
			//	foreach (var playerSpriteObject in _playerSpriteObjects)
			//		playerSpriteObject.LeanTowards(Vector3.zero);
		}

		private Vector2 GetSwipeDir(Vector2 direction)
		{
			var absX = Mathf.Abs(direction.x);
			var absY = Mathf.Abs(direction.y);
			var dotH = Mathf.Abs(Vector2.Dot(Vector2.right, new Vector2(absX, direction.y)));
			var dotV = Mathf.Abs(Vector2.Dot(Vector2.up, new Vector2(direction.x, absY)));
			var dot = Mathf.Abs(dotV - dotH);
			if (absX > absY)
			{
				// Horizontal Swipe
				var dir = new Vector3(Mathf.Sign(direction.x) * dot, 0);

				Debug.DrawLine(transform.position, direction, new Color(dotH, 1 - dotH, 0));
				Debug.DrawLine(transform.position, dir, new Color(dotH, 1 - dotH, 0));
				return dir;
			}
			else
			{
				// Vertical Swipe
				var dir = new Vector3(0, Mathf.Sign(direction.y) * dot);

				Debug.DrawLine(transform.position, direction, new Color(0, 1 - dotV, dotV));
				Debug.DrawLine(transform.position, dir, new Color(0, 1 - dotV, dotV));
				return dir;
			}
		}

		private PlayerActions DirectionToPlayerAction(Vector2 direction)
		{
			const float DIR_THRESHHOLD = 0.5f;

			if (direction.x >= DIR_THRESHHOLD)
				return PlayerActions.MoveRight;

			if (direction.x <= -DIR_THRESHHOLD)
				return PlayerActions.MoveLeft;

			if (direction.y >= DIR_THRESHHOLD)
				return PlayerActions.MoveUp;

			if (direction.y <= -DIR_THRESHHOLD)
				return PlayerActions.MoveDown;

			return PlayerActions.None;
		}

		#endregion
	}
}
