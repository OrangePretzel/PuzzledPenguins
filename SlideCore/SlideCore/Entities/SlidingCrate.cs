using System;
using SlideCore.Levels;
using SlideCore.Math;

namespace SlideCore.Entities
{
	public class SlidingCrate : DynamicEntity
	{
		public SlidingCrate(int id, int posX, int posY)
			: base(EntityTypes.SlidingCrate, id, posX, posY)
		{
		}

		public void Push(PlayerEntity playerEntity, IntVector2 motion)
		{
			SetMotion(motion);
		}

		protected override void HandleInteractionWithEntity(Entity entity, Level level, UpdateResult updateResult, IntVector2 newPosition, int updateTicks)
		{
			base.HandleInteractionWithEntity(entity, level, updateResult, newPosition, updateTicks);

			switch (entity?.EntityType)
			{
				case EntityTypes.Wall:
				case EntityTypes.Player:
				case EntityTypes.FinishFlag:
				case EntityTypes.SlidingCrate:
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
					_delayedAction = ur =>
					{
						SetPosition(portalEntity.PortalTarget);
						updateResult.Result = UpdateResult.ResultTypes.Teleported;
						return true;
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
				case EntityTypes.None:
				case null:
					SetPosition(newPosition);
					updateResult.Result = UpdateResult.ResultTypes.Moved;
					return;
				default:
					throw new NotSupportedException($"Unsupported interaction between [{EntityType}] and [{entity?.EntityType}]. Please implement interaction!");
			}
		}

		// No need to implement equality checks as the sliding crate does not add any additional data
	}
}
