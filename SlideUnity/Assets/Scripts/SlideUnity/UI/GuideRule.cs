using UnityEngine;

namespace SlideUnity.UI
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class GuideRule : MonoBehaviour
	{
		private enum RuleTypes
		{
			Horizontal,
			Vertical
		}

		public Transform Target;

		[SerializeField]
		private RuleTypes RuleType = RuleTypes.Horizontal;
		private Vector3 _levelCenter;

		private SpriteRenderer _spriteRenderer;

		private void OnEnable()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			GameManager.Events.OnLevelChanged += level => SetSize(level.LevelWidth, level.LevelHeight);
		}

		private void Update()
		{
			if (Target == null) return;

			if (RuleType == RuleTypes.Horizontal)
				transform.position = new Vector3(_levelCenter.x, Target.transform.position.y, 0);
			else
				transform.position = new Vector3(Target.transform.position.x, _levelCenter.y, 0);
		}

		private void SetSize(int levelWidth, int levelHeight)
		{
			if (RuleType == RuleTypes.Horizontal)
				_spriteRenderer.size = new Vector2(levelWidth, 1);
			else
				_spriteRenderer.size = new Vector2(1, levelHeight);

			_levelCenter = GameManager.LevelCenter;
		}
	}
}
