using System;

namespace SlideCore.Entities
{
	public class WallEntity : StaticEntity, IEquatable<WallEntity>
	{
		// Adjacent Wall Bit Index:
		// =====
		//  321 
		//  4.0 
		//  567 
		// =====
		private byte _adjacentWalls;
		/// <summary>A byte representing whose bits represent the presence of adjacent walls</summary>
		public byte AdjacentWalls => _adjacentWalls;
		/// <summary>Returns true if there is a wall at the given position index</summary>
		public bool IsAdjacentWall(int position) => (_adjacentWalls & (1 << position)) != 0;

		public WallEntity(int id, int posX, int posY, byte adjacentWalls = 0)
			: base(EntityTypes.Wall, id, posX, posY)
		{
			_adjacentWalls = adjacentWalls;
		}

		#region IEquatable

		// Not including Adjacent Walls as an equality parameter as it is used purely for cosmentics

		public bool Equals(WallEntity other) => base.Equals(other);
		public override bool Equals(object obj) => obj is WallEntity && Equals((WallEntity)obj);
		public static bool operator ==(WallEntity entity1, WallEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(WallEntity entity1, WallEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode() => base.GetHashCode();

		#endregion
	}
}
