using SlideCore.Levels;
using SlideCore.Math;
using System;

namespace SlideCore.Entities
{
	public class PlayerEntity : DynamicEntity, IEquatable<PlayerEntity>
	{
		public class PlayerEntitySnapshot : DynamicEntitySnapshot, IEquatable<PlayerEntitySnapshot>
		{
			#region IEquatable

			public bool Equals(PlayerEntitySnapshot other) => base.Equals(other);
			public override bool Equals(object obj) => obj is PlayerEntitySnapshot && Equals((PlayerEntitySnapshot)obj);
			public static bool operator ==(PlayerEntitySnapshot snapshot1, PlayerEntitySnapshot snapshot2) => Equals(snapshot1, snapshot2);
			public static bool operator !=(PlayerEntitySnapshot snapshot1, PlayerEntitySnapshot snapshot2) => !(snapshot1 == snapshot2);
			public override int GetHashCode() => base.GetHashCode();

			#endregion
		}

		public PlayerEntity(int id, int posX, int posY)
			: base(EntityTypes.Player, id, posX, posY)
		{
			_motion = new IntVector2(0, 0);
		}

		public override bool HandlePlayerAction(Level level, PlayerActions playerAction)
		{
			base.HandlePlayerAction(level, playerAction);

			var dir = RotationHelper.PlayerActionToDirection(playerAction);
			if (CanMoveInDirection(level, dir))
			{
				SetMotion(dir);
				return true;
			}

			return false;
		}

		protected override void HandleInteractionWithEntity(Entity entity, Level level, UpdateResult updateResult, IntVector2 newPosition, int updateTicks)
		{
			base.HandleInteractionWithEntity(entity, level, updateResult, newPosition, updateTicks);

			switch (entity?.EntityType)
			{
				case EntityTypes.Wall:
				case EntityTypes.Player:
					Collide(updateResult, _motion);
					return;
				case EntityTypes.RedirectTile:
					SetPosition(newPosition);
					var redirectTile = (RedirectTile)entity;
					var redirectDir = RotationHelper.GetDirectionFromRotationIndex(redirectTile.RedirectDir);

					if (!CanMoveInDirection(level, redirectDir))
					{
						Collide(updateResult, redirectDir);
						return;
					}

					SetMotion(redirectDir);
					updateResult.Result = UpdateResult.ResultTypes.Redirected;
					return;
				case EntityTypes.HaltTile:
					SetPosition(newPosition);
					Collide(updateResult, IntVector2.Zero);
					return;
				case EntityTypes.Portal:
					SetPosition(newPosition);
					var portalEntity = (PortalEntity)entity;
					updateResult.Result = UpdateResult.ResultTypes.DelayAction;
					updateResult.CollisionDir = IntVector2.Zero;
					_delayedAction = ur =>
					{
						SetPosition(portalEntity.PortalTarget);
						updateResult.Result = UpdateResult.ResultTypes.Teleported;
						return true;
					};
					return;
				case EntityTypes.FinishFlag:
					SetPosition(newPosition);
					SetMotion(0, 0);
					updateResult.Result = UpdateResult.ResultTypes.ReachedFinish;
					return;
				case EntityTypes.SlidingCrate:
					var slidingCrate = (SlidingCrate)entity;
					// We only need to check for a blocked crate if this is the first update
					if (updateTicks <= 1 && !slidingCrate.CanMoveInDirection(level, _motion))
					{
						// By pushing the crate this turn we don't add extra delayed ticks
						slidingCrate.Push(this, _motion);
						Collide(updateResult, _motion);
						return;
					}

					// Push the sliding crate
					updateResult.Result = UpdateResult.ResultTypes.DelayAction;
					updateResult.CollisionDir = _motion;
					_delayedAction = ur =>
					{
						slidingCrate.Push(this, _motion);
						Collide(ur, _motion);
						return true;
					};
					return;
				case EntityTypes.Pit:
					SetPosition(newPosition);
					SetMotion(0, 0);
					updateResult.Result = UpdateResult.ResultTypes.FatalResult;
					return;
				case EntityTypes.JumpTile:
					updateResult.Result = UpdateResult.ResultTypes.DelayAction;
					SetPosition(newPosition);
					_delayedAction = ur =>
					{
						var newPos = _position + 2 * _motion;
						if (CanMoveToPosition(level, newPos))
						{
							ur.Result = UpdateResult.ResultTypes.JumpTo;
							SetPosition(newPos);
							return true;
						}

						return false;
					};
					return;

				case EntityTypes.WireButton:
					updateResult.Result = UpdateResult.ResultTypes.Moved;
					SetPosition(newPosition);

					var wireButtonEntity = (WireButtonEntity)entity;
					wireButtonEntity.Interact();
					return;

				case EntityTypes.WireDoor:
					var wireDoorEntity = (WireDoorEntity)entity;
					if (wireDoorEntity.IsOpen)
					{
						updateResult.Result = UpdateResult.ResultTypes.Moved;
						SetPosition(newPosition);
					}
					else
						Collide(updateResult, _motion);
					return;

				case EntityTypes.Sushi:
					var sushiEntity = (SushiEntity)entity;
					sushiEntity.Collect();
					goto case EntityTypes.None;

				case EntityTypes.None:
				case null:
					SetPosition(newPosition);
					updateResult.Result = UpdateResult.ResultTypes.Moved;
					return;
				default:
					throw new NotSupportedException($"Unsupported interaction between Player and [{entity?.EntityType}]. Please implement interaction!");
			}
		}

		#region IStatefulEntity

		public override IStatefulSnapshot InitializeSnapshot()
		{
			var snapshot = new PlayerEntitySnapshot();
			UpdateSnapshot(snapshot);
			return snapshot;
		}

		public override void UpdateSnapshot(IStatefulSnapshot snapshot)
		{
			base.UpdateSnapshot(snapshot);
			var playerSnapshot = (PlayerEntitySnapshot)snapshot;
		}

		public override void RestoreSnapshot(IStatefulSnapshot snapshot)
		{
			base.RestoreSnapshot(snapshot);
			var playerSnapshot = (PlayerEntitySnapshot)snapshot;
		}

		#endregion

		#region IEquatable

		public bool Equals(PlayerEntity other) => base.Equals(other);
		public override bool Equals(object obj) => obj is PlayerEntity && Equals((PlayerEntity)obj);
		public static bool operator ==(PlayerEntity entity1, PlayerEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(PlayerEntity entity1, PlayerEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode() => base.GetHashCode();

		#endregion
	}
}
