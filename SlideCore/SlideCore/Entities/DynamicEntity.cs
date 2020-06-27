using SlideCore.Levels;
using SlideCore.Math;
using System;
using System.Diagnostics;

namespace SlideCore.Entities
{
	/// <summary>DynamicEntities are entities that move. The are stateful by principal (change/save/restore state)</summary>
	[DebuggerDisplay("DynamicEntity {EntityType} {Position}")]
	public abstract class DynamicEntity : Entity, IEquatable<DynamicEntity>, IStatefulEntity, IUpdatingEntity
	{
		/// <summary>A snapshot of a DynamicEntity's state</summary>
		public class DynamicEntitySnapshot : IEquatable<DynamicEntitySnapshot>, IStatefulSnapshot
		{
			public bool Enabled;
			public IntVector2 Position;
			public IntVector2 Motion;

			public DynamicEntitySnapshot()
			{
				Enabled = false;
				Position = new IntVector2();
				Motion = new IntVector2();
			}

			#region IEquatable

			public bool Equals(DynamicEntitySnapshot other) =>
				other != null
				&& Enabled == other.Enabled
				&& Position == other.Position
				&& Motion == other.Motion;
			public override bool Equals(object obj) => obj is DynamicEntitySnapshot && Equals((DynamicEntitySnapshot)obj);
			public static bool operator ==(DynamicEntitySnapshot snapshot1, DynamicEntitySnapshot snapshot2) => Equals(snapshot1, snapshot2);
			public static bool operator !=(DynamicEntitySnapshot snapshot1, DynamicEntitySnapshot snapshot2) => !(snapshot1 == snapshot2);
			public override int GetHashCode()
			{
				var hashCode = 500944194;
				hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
				hashCode = hashCode * -1521134295 + Position.GetHashCode();
				hashCode = hashCode * -1521134295 + Motion.GetHashCode();
				return hashCode;
			}

			#endregion
		}

		protected bool _enabled;
		protected UpdateResult _updateResult;

		protected IntVector2 _motion;
		public IntVector2 Motion => _motion;

		protected Func<UpdateResult, bool> _delayedAction;

		public DynamicEntity(EntityTypes entityType, int id, int posX, int posY)
			: base(entityType, id, posX, posY)
		{
			_enabled = true;
			_updateResult = new UpdateResult(this);
		}

		/// <summary>Set the entity's motion</summary>
		public void SetMotion(IntVector2 motion)
		{
			_motion.X = motion.X;
			_motion.Y = motion.Y;
		}

		/// <summary>Set the entity's motion</summary>
		public void SetMotion(int motionX, int motionY)
		{
			_motion.X = motionX;
			_motion.Y = motionY;
		}

		/// <summary>Determines if there is an entity in a given direction</summary>
		public bool CanMoveInDirection(Level level, IntVector2 dir)
		{
			if (dir.X == 0 && dir.Y == 0)
				return false;

			return CanMoveToPosition(level, _position + dir);
		}

		/// <summary>True if the entity can move to the given position</summary>
		public bool CanMoveToPosition(Level level, IntVector2 pos)
		{
			if (!level.IsInBounds(pos.X, pos.Y))
				return false;

			// Static entities
			Entity entityAtXY = level.GetStaticEntityAtPosition(pos.X, pos.Y);
			if (entityAtXY != null && ShouldCollideWithEntity(entityAtXY)) return false;

			// Dynamic entities
			entityAtXY = level.GetDynamicEntityAtPosition(pos.X, pos.Y);
			if (entityAtXY == null) return true;
			if (ShouldCollideWithEntity(entityAtXY)) return false;

			return true;
		}

		/// <summary>Handle the interaction between this and the given entity</summary>
		protected virtual void HandleInteractionWithEntity(Entity entity, Level level, UpdateResult updateResult, IntVector2 newPosition, int updateTicks)
		{
			updateResult.InteractionEntity = entity;
		}

		/// <summary>Collide in the given direction, setting motion to 0 and updating the result</summary>
		protected void Collide(UpdateResult updateResult, IntVector2 direction)
		{
			updateResult.Result = UpdateResult.ResultTypes.Collided;
			updateResult.CollisionDir = direction;

			SetMotion(0, 0);
			return;
		}

		/// <summary>Returns true if the entity should collide with the given entity</summary>
		protected virtual bool ShouldCollideWithEntity(Entity entity)
		{
			if (entity?.EntityType == EntityTypes.Wall) return true;
			if (entity?.EntityType == EntityTypes.WireDoor)
				return ((WireDoorEntity)entity).IsClosed;
			return false;
		}

		#region IUpdatingEntity

		public bool Enabled => _enabled;
		public UpdateResult GetUpdateResult() => _updateResult;

		/// <summary>Handles actions that the player input</summary>
		public virtual bool HandlePlayerAction(Level level, PlayerActions playerAction) { return false; }

		/// <summary>Update the entity</summary>
		public virtual void Update(Level level, int updateTicks)
		{
			// Reset
			_updateResult.CollisionDir = IntVector2.Zero;
			_updateResult.InteractionEntity = null;

			if (_motion.X == 0 && _motion.Y == 0)
			{
				_updateResult.Result = UpdateResult.ResultTypes.None;
				return;
			}

			if (_updateResult.Result == UpdateResult.ResultTypes.DelayAction)
				if (_delayedAction?.Invoke(_updateResult) ?? false)
					return;

			var newPosition = new IntVector2(_position.X + _motion.X, _position.Y + _motion.Y);

			if (!level.IsInBounds(newPosition.X, newPosition.Y))
			{
				_updateResult.Result = UpdateResult.ResultTypes.Collided;
				_updateResult.CollisionDir = _motion;

				SetMotion(0, 0);
				return;
			}

			Entity entityAtXY = level.GetDynamicEntityAtPosition(newPosition.X, newPosition.Y);
			if (entityAtXY != null)
				HandleInteractionWithEntity(entityAtXY, level, _updateResult, newPosition, updateTicks);
			else
			{
				entityAtXY = level.GetStaticEntityAtPosition(newPosition.X, newPosition.Y);
				HandleInteractionWithEntity(entityAtXY, level, _updateResult, newPosition, updateTicks);
			}
		}

		#endregion

		#region IStatefulEntity

		/// <summary>Initialize and take a snapshot of the entity's state</summary>
		public virtual IStatefulSnapshot InitializeSnapshot()
		{
			DynamicEntitySnapshot dynamicEntitySnapshot = new DynamicEntitySnapshot();
			UpdateSnapshot(dynamicEntitySnapshot);
			return dynamicEntitySnapshot;
		}

		/// <summary>Take a snapshot of the entity's state</summary>
		public virtual void UpdateSnapshot(IStatefulSnapshot snapshot)
		{
			var dynamicSnapshot = (DynamicEntitySnapshot)snapshot;
			dynamicSnapshot.Enabled = _enabled;
			dynamicSnapshot.Position.X = _position.X;
			dynamicSnapshot.Position.Y = _position.Y;
			dynamicSnapshot.Motion.X = _motion.X;
			dynamicSnapshot.Motion.Y = _motion.Y;
		}

		/// <summary>Restore the entity's state to that of the snapshot</summary>
		public virtual void RestoreSnapshot(IStatefulSnapshot snapshot)
		{
			var dynamicSnapshot = (DynamicEntitySnapshot)snapshot;
			_enabled = dynamicSnapshot.Enabled;
			_position.X = dynamicSnapshot.Position.X;
			_position.Y = dynamicSnapshot.Position.Y;
			_motion.X = dynamicSnapshot.Motion.X;
			_motion.Y = dynamicSnapshot.Motion.Y;
		}

		#endregion

		#region IEquatable

		public bool Equals(DynamicEntity other) =>
			base.Equals(other)
			&& ID == other.ID
			&& Enabled == other.Enabled
			&& Motion == other.Motion;
		public override bool Equals(object obj) => obj is DynamicEntity && Equals((DynamicEntity)obj);
		public static bool operator ==(DynamicEntity entity1, DynamicEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(DynamicEntity entity1, DynamicEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode = hashCode * 31 + ID.GetHashCode();
			hashCode = hashCode * 31 + Enabled.GetHashCode();
			hashCode = hashCode * 31 + Motion.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
