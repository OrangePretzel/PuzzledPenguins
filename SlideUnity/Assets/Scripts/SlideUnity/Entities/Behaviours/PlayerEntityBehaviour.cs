using SlideCore;
using SlideCore.Entities;
using SlideCore.Math;
using System.Threading.Tasks;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class PlayerEntityBehaviour : DynamicEntityBehaviour
	{
		public new static PlayerEntityBehaviour Behaviour = new PlayerEntityBehaviour();

		public override void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
			base.InitializeEntity(spriteObject, entity);
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			dynamicSpriteObject.ResetAnimator("HasWon", "HasFatality");
		}

		public override async void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			switch (updateResult.Result)
			{
				case UpdateResult.ResultTypes.None:
					// Do nothing
					break;
				case UpdateResult.ResultTypes.Collided:
					SetupBumpOnTargetReached(dynamicSpriteObject, updateResult.CollisionDir);
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					if (updateResult.InteractionEntity?.EntityType == EntityTypes.HaltTile)
						GameManager.GetStaticSpriteObjectFromPosition(updateResult.InteractionEntity.Position.ToVector2Int()).TriggerAction(FlashEntityBehaviour.FLASHENTITY_FLASH_ACTION);
					break;
				case UpdateResult.ResultTypes.Teleported:
					dynamicSpriteObject.SetTargetPositionInstant(updateResult.Entity.Position.ToVector2(), true);
					break;
				case UpdateResult.ResultTypes.ReachedFinish:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					dynamicSpriteObject.AnimateOnReachTarget("HasWon");
					break;
				case UpdateResult.ResultTypes.FatalResult:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					dynamicSpriteObject.AnimateOnReachTarget("HasFatality");
					break;
				case UpdateResult.ResultTypes.Redirected:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					if (updateResult.InteractionEntity?.EntityType == EntityTypes.RedirectTile)
						GameManager.GetStaticSpriteObjectFromPosition(updateResult.InteractionEntity.Position.ToVector2Int()).TriggerAction(FlashEntityBehaviour.FLASHENTITY_FLASH_ACTION);
					break;
				case UpdateResult.ResultTypes.Moved:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;
				case UpdateResult.ResultTypes.DelayAction:
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;

				case UpdateResult.ResultTypes.JumpTo:
					if (updateResult.InteractionEntity?.EntityType == EntityTypes.JumpTile)
						GameManager.GetStaticSpriteObjectFromPosition(updateResult.InteractionEntity.Position.ToVector2Int()).TriggerAction(JumpTileBehaviour.JUMPTILE_JUMP_ACTION);

					dynamicSpriteObject.AnimateToReachTarget("IsJumping");
					await Task.Delay(50);
					dynamicSpriteObject.SetTargetPosition(updateResult.Entity.Position.ToVector2());
					break;
				default:
					Debug.LogError($"Unimplemented ResultType returned [{updateResult.Result}]");
					break;
			}
		}

		public override void CancelAction(SpriteObject spriteObject, PlayerActions playerAction)
		{
			base.CancelAction(spriteObject, playerAction);
			var dynamicSpriteObject = (DynamicSpriteObject)spriteObject;

			var dir = RotationHelper.PlayerActionToDirection(playerAction);
			dynamicSpriteObject.StartBump(dir.ToVector2());
		}

		protected void SetupBumpOnTargetReached(DynamicSpriteObject dynamicSpriteObject, IntVector2 dir)
		{
			if (dir != IntVector2.Zero)
			{
				dynamicSpriteObject.OnEntityReachedTarget.RegisterCallback(() =>
				{
					dynamicSpriteObject.StartBump(dir.ToVector2());
				});
			}
		}
	}
}
