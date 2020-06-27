using UnityEngine;

namespace SlideUnity
{
	[CreateAssetMenu(fileName = "SeamlessGraphicsDictionary", menuName = "Slide/Seamless Graphics Dictionary", order = 2)]
	public class SeamlessGraphicsDictionary : ScriptableObject
	{
		public struct SpriteWithTransform
		{
			public Sprite Sprite;
			public Vector3 Scale;
			public int Rotation;

			public SpriteWithTransform(Sprite sprite, Vector2 scale, int rotation)
			{
				Sprite = sprite;
				Scale = scale;
				Rotation = rotation;
			}

			public SpriteWithTransform(Sprite sprite, int rotation)
			{
				Sprite = sprite;
				Scale = Vector3.one;
				Rotation = rotation;
			}

			public SpriteWithTransform(Sprite sprite)
			{
				Sprite = sprite;
				Scale = Vector3.one;
				Rotation = 0;
			}
		}

		public Sprite NoAdjacentSides;

		public Sprite OneAdjacentSide;

		public Sprite TwoAdjacentWithNoCorners;
		public Sprite TwoAdjacentWithTwoOppositeSides;
		public Sprite TwoAdjacentWithCorners;

		public Sprite ThreeSidesWithNoCorners;
		public Sprite ThreeSidesWithOneCorner;
		public Sprite ThreeSidesWithCorners;

		public Sprite FourSidesWithNoCorners;
		public Sprite FourSidesWithOneCorner;
		public Sprite FourSidesWithTwoAdjacentCorners;
		public Sprite FourSidesWithTwoOppositeCorners;
		public Sprite FourSidesWithThreeAdjacentCorners;
		public Sprite FourSidesWithCorners;

		public SpriteWithTransform GetSpriteForLayout(byte layout)
		{
			bool right = (layout & (1 << 0)) != 0;
			bool topRight = (layout & (1 << 1)) != 0;
			bool top = (layout & (1 << 2)) != 0;
			bool topLeft = (layout & (1 << 3)) != 0;
			bool left = (layout & (1 << 4)) != 0;
			bool bottomLeft = (layout & (1 << 5)) != 0;
			bool bottom = (layout & (1 << 6)) != 0;
			bool bottomRight = (layout & (1 << 7)) != 0;

			int majorCount =
				(top ? 1 : 0) +
				(bottom ? 1 : 0) +
				(left ? 1 : 0) +
				(right ? 1 : 0);

			switch (majorCount)
			{
				case 0:
					return new SpriteWithTransform(NoAdjacentSides);
				case 1:
					if (right)
						return new SpriteWithTransform(OneAdjacentSide, 0);
					if (top)
						return new SpriteWithTransform(OneAdjacentSide, 1);
					if (left)
						return new SpriteWithTransform(OneAdjacentSide, 2);
					if (bottom)
						return new SpriteWithTransform(OneAdjacentSide, 3);
					break;
				case 2:
					if (left && right)
						return new SpriteWithTransform(TwoAdjacentWithTwoOppositeSides, 0);
					if (top && bottom)
						return new SpriteWithTransform(TwoAdjacentWithTwoOppositeSides, 1);
					if (top)
					{
						if (right)
							return new SpriteWithTransform(topRight ? TwoAdjacentWithCorners : TwoAdjacentWithNoCorners, 0);
						if (left)
							return new SpriteWithTransform(topLeft ? TwoAdjacentWithCorners : TwoAdjacentWithNoCorners, 1);
					}
					if (bottom)
					{
						if (left)
							return new SpriteWithTransform(bottomLeft ? TwoAdjacentWithCorners : TwoAdjacentWithNoCorners, 2);
						if (right)
							return new SpriteWithTransform(bottomRight ? TwoAdjacentWithCorners : TwoAdjacentWithNoCorners, 3);
					}
					break;
				case 3:
					if (!left)
					{
						if (topRight && bottomRight)
							return new SpriteWithTransform(ThreeSidesWithCorners, 0);
						if (topRight)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, new Vector3(1, -1, 1), 0);
						if (bottomRight)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, 0);
						return new SpriteWithTransform(ThreeSidesWithNoCorners, 0);
					}
					if (!bottom)
					{
						if (topLeft && topRight)
							return new SpriteWithTransform(ThreeSidesWithCorners, 1);
						if (topLeft)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, new Vector3(1, -1, 1), 1);
						if (topRight)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, 1);
						return new SpriteWithTransform(ThreeSidesWithNoCorners, 1);
					}
					if (!right)
					{
						if (topLeft && bottomLeft)
							return new SpriteWithTransform(ThreeSidesWithCorners, 2);
						if (bottomLeft)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, new Vector3(1, -1, 1), 2);
						if (topLeft)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, 2);
						return new SpriteWithTransform(ThreeSidesWithNoCorners, 2);
					}
					if (!top)
					{
						if (bottomLeft && bottomRight)
							return new SpriteWithTransform(ThreeSidesWithCorners, 3);
						if (bottomRight)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, new Vector3(1, -1, 1), 3);
						if (bottomLeft)
							return new SpriteWithTransform(ThreeSidesWithOneCorner, 3);
						return new SpriteWithTransform(ThreeSidesWithNoCorners, 3);
					}
					break;
				case 4:
					int minorCount =
						(topRight ? 1 : 0) +
						(bottomRight ? 1 : 0) +
						(topLeft ? 1 : 0) +
						(bottomLeft ? 1 : 0);

					switch (minorCount)
					{
						case 4:
							return new SpriteWithTransform(FourSidesWithCorners);
						case 3:
							if (!topRight)
								return new SpriteWithTransform(FourSidesWithThreeAdjacentCorners, 0);
							if (!topLeft)
								return new SpriteWithTransform(FourSidesWithThreeAdjacentCorners, 1);
							if (!bottomLeft)
								return new SpriteWithTransform(FourSidesWithThreeAdjacentCorners, 2);
							if (!bottomRight)
								return new SpriteWithTransform(FourSidesWithThreeAdjacentCorners, 3);
							break;
						case 2:
							if (bottomRight && topLeft)
								return new SpriteWithTransform(FourSidesWithTwoOppositeCorners, 0);
							if (topRight && bottomLeft)
								return new SpriteWithTransform(FourSidesWithTwoOppositeCorners, 1);

							if (topLeft && bottomLeft)
								return new SpriteWithTransform(FourSidesWithTwoAdjacentCorners, 0);
							if (bottomLeft && bottomRight)
								return new SpriteWithTransform(FourSidesWithTwoAdjacentCorners, 1);
							if (bottomRight && topRight)
								return new SpriteWithTransform(FourSidesWithTwoAdjacentCorners, 2);
							if (topRight && topLeft)
								return new SpriteWithTransform(FourSidesWithTwoAdjacentCorners, 3);
							break;
						case 1:
							if (topLeft)
								return new SpriteWithTransform(FourSidesWithOneCorner, 0);
							if (bottomLeft)
								return new SpriteWithTransform(FourSidesWithOneCorner, 1);
							if (bottomRight)
								return new SpriteWithTransform(FourSidesWithOneCorner, 2);
							if (topRight)
								return new SpriteWithTransform(FourSidesWithOneCorner, 3);
							break;
						case 0:
							return new SpriteWithTransform(FourSidesWithNoCorners);
					}
					break;
			}

			return new SpriteWithTransform(NoAdjacentSides);
		}
	}
}
