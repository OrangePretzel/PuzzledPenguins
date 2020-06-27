using System.Threading.Tasks;
using UnityEngine;

namespace SlideUnity.Entities.Behaviours
{
	public class FlashEntityBehaviour : NoEntityBehaviour
	{
		public static FlashEntityBehaviour Behaviour = new FlashEntityBehaviour();

		public const string FLASHENTITY_FLASH_ACTION = "FLSH";

		private static Color FLASH_COLOR = new Color(0.75f, 0.75f, 0.75f);
		private static int FLASH_DURATION = 300;

		public override void TriggerAction(SpriteObject spriteObject, string actionID)
		{
			if (actionID != FLASHENTITY_FLASH_ACTION) return;

			spriteObject.OnEndOfUpdate.RegisterCallback(async () =>
			{
				spriteObject.SpriteRenderer.color = FLASH_COLOR;
				await Task.Delay(FLASH_DURATION);
				spriteObject.SpriteRenderer.color = Color.white;
			});
		}
	}
}
