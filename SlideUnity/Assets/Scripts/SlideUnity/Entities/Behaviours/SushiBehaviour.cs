using SlideCore;
using SlideCore.Entities;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class SushiBehaviour : IEntityBehaviour
	{
		public static SushiBehaviour Behaviour = new SushiBehaviour();

		public void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }

		public void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
			var sushiEntity = (SushiEntity)entity;
			SetGraphic(spriteObject, sushiEntity.IsCollected);
		}

		public void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var sushiEntity = (SushiEntity)updateResult.Entity;
			SetGraphic(spriteObject, sushiEntity.IsCollected);
		}

		public void UpdateToEntity(SpriteObject spriteObject, Entity entity)
		{
			var sushiEntity = (SushiEntity)entity;
			SetGraphic(spriteObject, sushiEntity.IsCollected);
		}

		public void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			if (updateResult.Result != UpdateResult.ResultTypes.StateChanged) return;
			var sushiEntity = (SushiEntity)updateResult.Entity;
			if (sushiEntity.IsCollected)
				spriteObject.OnEndOfUpdate.RegisterCallback(() =>
				{
					spriteObject.OnEntityFinishedAnimation.RegisterCallback(() => SetGraphic(spriteObject, true));
					spriteObject.PlayAnimation(GameManager.GraphicsDictionary.SushiSprites.Sushi_CollectAnimation);
				});
		}

		public void TriggerAction(SpriteObject spriteObject, string actionID) { }

		private void SetGraphic(SpriteObject spriteObject, bool isCollected)
		{
			if (isCollected)
			{
				spriteObject.SetGraphic(GameManager.GraphicsDictionary.SushiSprites.SushiSprite_Ghost);
				spriteObject.PlayAnimation(GameManager.GraphicsDictionary.SushiSprites.Sushi_BobAnimation);

				Color ghostColor = Color.white;
				ghostColor.a = 0f;
				spriteObject.SpriteRenderer.color = ghostColor;
			}
			else
			{
				spriteObject.SetGraphic(GameManager.GraphicsDictionary.SushiSprites.SushiSprite);
				spriteObject.PlayAnimation(GameManager.GraphicsDictionary.SushiSprites.Sushi_BobAnimation);
				spriteObject.SpriteRenderer.color = Color.white;
			}
		}
	}
}
