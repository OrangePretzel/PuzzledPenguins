using SlideCore;
using SlideCore.Entities;
using SlideCore.Math;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class DynamicEntityBehaviour : IEntityBehaviour
	{
		public static DynamicEntityBehaviour Behaviour = new DynamicEntityBehaviour();

		public virtual void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
		}

		public virtual void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			dynamicSpriteObject.HandleAnimationComplete();
			dynamicSpriteObject.OnEntityReachedTarget.ClearCallback();
			dynamicSpriteObject.OnEntityFinishedAnimation.ClearCallback();

			dynamicSpriteObject.SetTargetPositionInstant(updateResult.Entity.Position.ToVector2(), updateResult.Entity.EntityType == EntityTypes.Player);
			dynamicSpriteObject.SetDirection(Vector3.zero);
		}

		public virtual void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			if (updateResult.CollisionDir != IntVector2.Zero)
			{
				dynamicSpriteObject.OnEntityReachedTarget.RegisterCallback(() =>
				{
					dynamicSpriteObject.StartBump(updateResult.CollisionDir.ToVector2());
				});
			}

			switch (updateResult.Result)
			{
				case UpdateResult.ResultTypes.None:
					// Do nothing
					break;
				case UpdateResult.ResultTypes.Collided:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;
				case UpdateResult.ResultTypes.Teleported:
					dynamicSpriteObject.SetTargetPositionInstant(updateResult.Entity.Position.ToVector2(), true);
					break;
				case UpdateResult.ResultTypes.ReachedFinish:
				case UpdateResult.ResultTypes.Moved:
				case UpdateResult.ResultTypes.Redirected:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;
				case UpdateResult.ResultTypes.DelayAction:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;
				default:
					Debug.LogError($"Unimplemented ResultType returned [{updateResult.Result}]");
					break;
			}
		}

		public virtual void UpdateToEntity(SpriteObject spriteObject, Entity entity)
		{
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			dynamicSpriteObject.SetTargetPositionInstant(entity.Position.ToVector2(), false);
		}

		public virtual void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }

		public virtual void TriggerAction(SpriteObject spriteObject, string actionID) { }
	}
}
