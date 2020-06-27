using SlideCore;
using SlideCore.Entities;

namespace SlideUnity.Entities.Behaviours
{
	public class WireButtonBehaviour : IEntityBehaviour
	{
		public static WireButtonBehaviour Behaviour = new WireButtonBehaviour();

		public void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }

		public void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
			var wireButtonEntity = (WireButtonEntity)entity;
			SetGraphic(spriteObject, wireButtonEntity.ButtonMode, wireButtonEntity.IsPressed);
		}

		public void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			var wireButtonEntity = (WireButtonEntity)updateResult.Entity;
			SetGraphic(spriteObject, wireButtonEntity.ButtonMode, wireButtonEntity.IsPressed);
		}

		public void UpdateToEntity(SpriteObject spriteObject, Entity entity)
		{
			var wireButtonEntity = (WireButtonEntity)entity;
			SetGraphic(spriteObject, wireButtonEntity.ButtonMode, wireButtonEntity.IsPressed);
		}

		public void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult)
		{
			if (updateResult.Result != UpdateResult.ResultTypes.StateChanged) return;
			var wireButtonEntity = (WireButtonEntity)updateResult.Entity;
			spriteObject.OnEndOfUpdate.RegisterCallback(() => SetGraphic(spriteObject, wireButtonEntity.ButtonMode, wireButtonEntity.IsPressed));
		}

		public void TriggerAction(SpriteObject spriteObject, string actionID) { }

		private void SetGraphic(SpriteObject spriteObject, WireButtonEntity.ButtonModes buttonMode, bool isPressed)
		{
			switch (buttonMode)
			{
				case WireButtonEntity.ButtonModes.PressOnly:
					spriteObject.SetGraphic(isPressed ? GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Press_On : GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Press_Off);
					break;
				case WireButtonEntity.ButtonModes.Toggle:
					spriteObject.SetGraphic(isPressed ? GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Toggle_On : GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Toggle_Off);
					break;
				case WireButtonEntity.ButtonModes.Hold:
					spriteObject.SetGraphic(isPressed ? GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Hold_On : GameManager.GraphicsDictionary.WireButtonSprites.ButtonSprite_Hold_Off);
					break;
				default:
					throw new System.Exception($"Unimplemented graphics for button mode [{buttonMode}]");
			}
		}
	}
}
