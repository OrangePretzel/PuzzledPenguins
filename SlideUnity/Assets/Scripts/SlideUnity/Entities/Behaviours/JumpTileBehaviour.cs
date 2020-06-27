using SlideCore;
using SlideCore.Entities;
using System.Threading.Tasks;

namespace SlideUnity.Entities.Behaviours
{
	public class JumpTileBehaviour : IEntityBehaviour
	{
		public const string JUMPTILE_JUMP_ACTION = "JUMP";

		public static JumpTileBehaviour Behaviour = new JumpTileBehaviour();

		public void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }

		public void InitializeEntity(SpriteObject spriteObject, Entity entity) => spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
		public void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult) => spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
		public void UpdateToEntity(SpriteObject spriteObject, Entity entity) => spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
		public void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult) => spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);

		public async void TriggerAction(SpriteObject spriteObject, string actionID)
		{
			if (actionID != JUMPTILE_JUMP_ACTION) return;

			spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Compressed);
			await Task.Delay(50);
			spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
			await Task.Delay(25);
			spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Extended);
			await Task.Delay(50);
			spriteObject.SetGraphic(GameManager.GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
		}
	}
}
