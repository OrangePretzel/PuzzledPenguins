using SlideCore.Math;
using UnityEngine;

namespace SlideUnity
{
	public static class IntVector2Extensions
	{
		public static Vector2 ToVector2(this IntVector2 intVector2)
		{
			return new Vector2(intVector2.X, -intVector2.Y);
		}

		public static Vector2Int ToVector2Int(this IntVector2 intVector2)
		{
			return new Vector2Int(intVector2.X, -intVector2.Y);
		}

		public static IntVector2 RoundToIntVector2(this Vector2 vector2)
		{
			return new IntVector2(Mathf.RoundToInt(vector2.x), -Mathf.RoundToInt(vector2.y));
		}

		public static IntVector2 TruncToIntVector2(this Vector2 vector2)
		{
			return new IntVector2((int)vector2.x, -(int)vector2.y);
		}

		public static bool IsNonZero(this IntVector2 intVector2) => intVector2.X != 0 || intVector2.Y != 0;
	}
}
