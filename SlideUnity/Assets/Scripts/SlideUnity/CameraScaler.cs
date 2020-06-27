using UnityEngine;

namespace SlideUnity
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class CameraScaler : MonoBehaviour
	{
		private Camera _camera;
		public Camera Camera => _camera;

		public Vector3 CameraOffset = Vector3.zero;

		private void OnEnable()
		{
			_camera = GetComponent<Camera>();
		}

		private void Awake()
		{
			GameManager.Events.OnLevelChanged += level => ScaleToLevel(level.LevelWidth, level.LevelHeight);
		}

		//[SerializeField]
		//private int LevelWidth = 10;
		//[SerializeField]
		//private int LevelHeight = 10;

		//private void Update()
		//{
		//	if (!Application.isEditor || Application.isPlaying) return;
		//	ScaleToLevel(LevelWidth, LevelHeight);
		//}

		public void ScaleToLevel(int width, int height)
		{
			if (width == 0 || height == 0)
				return;

			float effectiveWidth = width;

			const float MIN_RATIO = 12f / 13f;
			var widthToHeightRatio = effectiveWidth / height;
			if (widthToHeightRatio < MIN_RATIO)
			{
				Debug.Log("Less than minimum");
				var ratioOfRatios = MIN_RATIO / widthToHeightRatio;
				effectiveWidth = effectiveWidth * ratioOfRatios;
			}

			_camera.orthographicSize = effectiveWidth;
			transform.position = new Vector3(width / 2f - 0.5f, height / -2f + 0.5f, transform.position.z) + CameraOffset;
		}
	}
}