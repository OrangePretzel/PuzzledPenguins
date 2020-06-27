using SlideCore;
using SlideCore.Entities;

namespace SlideUnity.Entities.Behaviours
{
	public interface IEntityBehaviour
	{
		void InitializeEntity(SpriteObject spriteObject, Entity entity);
		void UpdateToEntity(SpriteObject spriteObject, Entity entity);
		void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult);
		void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult);
		void CancelAction(SpriteObject spriteObject, PlayerActions playerAction);
		void TriggerAction(SpriteObject spriteObject, string actionID);
	}
}
