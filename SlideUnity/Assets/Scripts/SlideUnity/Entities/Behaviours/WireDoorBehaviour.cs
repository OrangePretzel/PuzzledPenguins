using SlideCore;
using SlideCore.Entities;
using System.Threading.Tasks;

namespace SlideUnity.Entities.Behaviours
{
	public class WireDoorBehaviour : IEntityBehaviour
	{
		public static WireDoorBehaviour Behaviour = new WireDoorBehaviour();

		public void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }

		public void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
			var wireDoorEntity = (WireDoorEntity)entity;
			SetGraphic(spriteObject, wireDoorEntity.IsOpen);
		}

		public void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var wireDoorEntity = (WireDoorEntity)updateResult.Entity;
			SetGraphic(spriteObject, wireDoorEntity.IsOpen);
		}

		public void UpdateToEntity(SpriteObject spriteObject, Entity entity)
		{
			var wireDoorEntity = (WireDoorEntity)entity;
			SetGraphic(spriteObject, wireDoorEntity.IsOpen);
		}

		public void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			if (updateResult.Result != UpdateResult.ResultTypes.StateChanged) return;
			var wireDoorEntity = (WireDoorEntity)updateResult.Entity;
			spriteObject.OnEndOfUpdate.RegisterCallback(() => SetGraphic(spriteObject, wireDoorEntity.IsOpen));
		}

		public void TriggerAction(SpriteObject spriteObject, string actionID) { }

		private async Task SetGraphic(SpriteObject spriteObject, bool isOpen)
		{
			if ((isOpen && spriteObject.SpriteRenderer.sprite == GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Open)
				|| (!isOpen && spriteObject.SpriteRenderer.sprite == GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Closed))
				return;

			if (isOpen)
				for (int i = 0; i < GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Transition.Length; i++)
				{
					spriteObject.SetGraphic(GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Transition[i]);
					await Task.Delay(25);
				}
			else
				for (int i = GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Transition.Length - 1; i >= 0; i--)
				{
					spriteObject.SetGraphic(GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Transition[i]);
					await Task.Delay(25);
				}
			spriteObject.SetGraphic(isOpen ? GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Open : GameManager.GraphicsDictionary.WireDoorSprites.WireDoor_Closed);
		}
	}
}
