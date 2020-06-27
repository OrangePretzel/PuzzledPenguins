using System;

namespace SlideCore.Math
{
	/// <summary>A 2D vector with integer components</summary>
	public struct IntVector2 : IEquatable<IntVector2>
	{
		public int X;
		public int Y;

		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static IntVector2 Zero = new IntVector2(0, 0);
		public static IntVector2 One = new IntVector2(1, 1);

		public override string ToString() => $"({X}, {Y})";

		#region Operators

		public static IntVector2 operator +(IntVector2 vector1, IntVector2 vector2) =>
			new IntVector2(vector1.X + vector2.X, vector1.Y + vector2.Y);

		public static IntVector2 operator *(IntVector2 vector, int scalar) =>
			new IntVector2(vector.X * scalar, vector.Y * scalar);
		public static IntVector2 operator *(int scalar, IntVector2 vector) =>
			new IntVector2(vector.X * scalar, vector.Y * scalar);

		#endregion

		#region IEquatable

		public bool Equals(IntVector2 other) =>
			X == other.X &&
			Y == other.Y;
		public override bool Equals(object obj) => obj is IntVector2 && Equals((IntVector2)obj);
		public static bool operator ==(IntVector2 vector1, IntVector2 vector2) => vector1.Equals(vector2);
		public static bool operator !=(IntVector2 vector1, IntVector2 vector2) => !(vector1 == vector2);
		public override int GetHashCode()
		{
			var hashCode = 1861411795;
			//hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
