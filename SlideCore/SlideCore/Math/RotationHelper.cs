namespace SlideCore.Math
{
	/// <summary>A helper for converting between various rotation representations</summary>
	public static class RotationHelper
	{
		private static IntVector2[] _rotationIndexToDirectionMapping = new IntVector2[]
		{
			new IntVector2(1, 0),
			new IntVector2(0, -1),
			new IntVector2(-1, 0),
			new IntVector2(0, 1)
		};
		/// <summary>Get the direction associated with a rotation</summary>
		public static IntVector2 GetDirectionFromRotationIndex(int index) => _rotationIndexToDirectionMapping[index];

		private static int[] _rotationIndexToAngleInDegrees = new int[] { 0, 90, 180, 270 };
		/// <summary>Get the angle in degrees associated with a ration</summary>
		public static int GetAngleInDegreesFromRotationIndex(int index) => _rotationIndexToAngleInDegrees[index];

		/// <summary>Get the vector direction associated with a player action</summary>
		public static IntVector2 PlayerActionToDirection(PlayerActions playerAction)
		{
			switch (playerAction)
			{
				case PlayerActions.MoveRight:
					return new IntVector2(1, 0);
				case PlayerActions.MoveUp:
					return new IntVector2(0, -1);
				case PlayerActions.MoveLeft:
					return new IntVector2(-1, 0);
				case PlayerActions.MoveDown:
					return new IntVector2(0, 1);
				case PlayerActions.None:
				case PlayerActions.Undo:
					return new IntVector2(0, 0);
				default:
					throw new System.NotSupportedException($"Unimplemented player action {playerAction}. Please implement!");
			}
		}
	}
}
