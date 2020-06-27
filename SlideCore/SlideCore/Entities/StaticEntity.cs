using System;
using System.Diagnostics;

namespace SlideCore.Entities
{
	/// <summary>StaticEntities are stateless entities</summary>
	[DebuggerDisplay("StaticEntity {EntityType} {Position}")]
	public class StaticEntity : Entity, IEquatable<StaticEntity>
	{
		public StaticEntity(EntityTypes entityType, int id, int posX, int posY)
			: base(entityType, id, posX, posY)
		{
		}

		#region IEquatable

		public bool Equals(StaticEntity other) => base.Equals(other);
		public override bool Equals(object obj) => obj is StaticEntity && Equals((StaticEntity)obj);
		public static bool operator ==(StaticEntity entity1, StaticEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(StaticEntity entity1, StaticEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode() => base.GetHashCode();

		#endregion
	}
}
