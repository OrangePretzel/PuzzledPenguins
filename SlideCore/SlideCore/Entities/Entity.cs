using SlideCore.Math;
using System;

namespace SlideCore.Entities
{
	/// <summary>A class representing any character/object in the level</summary>
	public abstract class Entity : IEquatable<Entity>
	{
		protected int _id;
		public int ID => _id;

		protected EntityTypes _entityType;
		/// <summary>The entity's type</summary>
		public EntityTypes EntityType => _entityType;

		protected IntVector2 _position;
		/// <summary>The position of the entity within the level</summary>
		public IntVector2 Position => _position;

		protected Entity(EntityTypes entityType, int id, int posX, int posY)
		{
			_id = id;
			_entityType = entityType;
			_position = new IntVector2(posX, posY);
		}

		/// <summary>Set the entity's position</summary>
		public void SetPosition(IntVector2 position)
		{
			_position.X = position.X;
			_position.Y = position.Y;
		}

		/// <summary>Set the entity's position</summary>
		public void SetPosition(int positionX, int positionY)
		{
			_position.X = positionX;
			_position.Y = positionY;
		}

		#region IEquatable

		public bool Equals(Entity other) =>
			other != null
			&& ID == other.ID
			&& EntityType == other.EntityType
			&& Position == other.Position;
		public override bool Equals(object obj) => obj is Entity && Equals((Entity)obj);
		public static bool operator ==(Entity entity1, Entity entity2) => Equals(entity1, entity2);
		public static bool operator !=(Entity entity1, Entity entity2) => !(entity1 == entity2);

		public override int GetHashCode()
		{
			var hashCode = -391242039;
			hashCode = hashCode * -1521134295 + ID.GetHashCode();
			hashCode = hashCode * -1521134295 + EntityType.GetHashCode();
			hashCode = hashCode * -1521134295 + Position.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
