using SlideCore.Entities;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class SlidingCrateBehaviour : DynamicEntityBehaviour
	{
		public new static SlidingCrateBehaviour Behaviour = new SlidingCrateBehaviour();

		public override void InitializeEntity(SpriteObject spriteObject, Entity entity)
		{
			base.InitializeEntity(spriteObject, entity);

			spriteObject.SetGraphic(GameManager.GraphicsDictionary.SlidingCrateSprite);
		}
	}
}
