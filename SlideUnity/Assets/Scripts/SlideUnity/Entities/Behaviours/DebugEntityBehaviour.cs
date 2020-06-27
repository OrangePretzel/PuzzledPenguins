using SlideCore;
using SlideCore.Entities;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class DebugEntityBehaviour : IEntityBehaviour
	{
		public static DebugEntityBehaviour Behaviour = new DebugEntityBehaviour();
		public void InitializeEntity(SpriteObject spriteObject, Entity entity) => Debug.Log($"SpriteObject [{spriteObject.name}] initializing to match [{entity.EntityType}, {entity.ID}]");
		public void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult) => Debug.Log($"SpriteObject [{spriteObject.name}] undoing to match update result");
		public void UpdateToEntity(SpriteObject spriteObject, Entity entity) => Debug.Log($"SpriteObject [{spriteObject.name}] updating to match [{entity.EntityType}, {entity.ID}]");
		public void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult) => Debug.Log($"SpriteObject [{spriteObject.name}] updating to match update result [{updateResult.Result}]");
		public void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) => Debug.Log($"SpriteObject [{spriteObject.name}] handling cancelled action [{playerAction}]");
		public void TriggerAction(SpriteObject spriteObject, string actionID) => Debug.Log($"SpriteObject [{spriteObject.name}] triggering action [{actionID}]");
	}

	public class NoEntityBehaviour : IEntityBehaviour
	{
		public virtual void InitializeEntity(SpriteObject spriteObject, Entity entity) { }
		public virtual void UndoToResult(SpriteObject spriteObject, UpdateResult updateResult) { }
		public virtual void UpdateToEntity(SpriteObject spriteObject, Entity entity) { }
		public virtual void UpdateToResult(SpriteObject spriteObject, UpdateResult updateResult) { }
		public virtual void CancelAction(SpriteObject spriteObject, PlayerActions playerAction) { }
		public virtual void TriggerAction(SpriteObject spriteObject, string actionID) { }
	}
}
