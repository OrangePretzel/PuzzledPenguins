using UnityEngine;

namespace SlideUnity
{
	[CreateAssetMenu(fileName = "GraphicsDictionary", menuName = "Slide/Graphics Dictionary", order = 1)]
	public class GraphicsDictionary : ScriptableObject
	{
		#region UI Sub-structs

		[System.Serializable]
		public struct UIDictionary
		{
			[System.Serializable]
			public struct LevelTileDictionary
			{
				public Sprite LevelTile_Locked;
				public Sprite LevelTile_Unsolved;
				public Sprite LevelTile_Solved;

				public Sprite LevelTile_StarOff;
				public Sprite LevelTile_StarOn;

				public Sprite LevelTile_SushiOff;
				public Sprite LevelTile_SushiOn;

				public Sprite LevelTile_MinMovesOff;
				public Sprite LevelTile_MinMovesOn;
			}

			[System.Serializable]
			public struct LevelCompleteDictionary
			{
				public Sprite LevelComplete_StarOff;
				public Sprite LevelComplete_StarOn;

				public Sprite LevelComplete_SushiOff;
				public Sprite LevelComplete_SushiOn;

				public Sprite LevelComplete_MinMovesOff;
				public Sprite LevelComplete_MinMovesOn;
			}

			public LevelTileDictionary LevelTileSprites;
			public LevelCompleteDictionary LevelCompleteSprites;
		}

		#endregion

		#region Entity Sub-structs

		[System.Serializable]
		public struct JumpTileSpritesDictionary
		{
			public Sprite JumpTileSprite_Normal;
			public Sprite JumpTileSprite_Extended;
			public Sprite JumpTileSprite_Compressed;
		}

		[System.Serializable]
		public struct WireButtonSpritesDictionary
		{
			public Sprite ButtonSprite_Toggle_Off;
			public Sprite ButtonSprite_Toggle_Transition;
			public Sprite ButtonSprite_Toggle_On;

			public Sprite ButtonSprite_Hold_On;
			public Sprite ButtonSprite_Hold_Off;

			public Sprite ButtonSprite_Press_On;
			public Sprite ButtonSprite_Press_Off;
		}

		[System.Serializable]
		public struct WireDoorSpritesDictionary
		{
			public Sprite WireDoor_Open;
			public Sprite[] WireDoor_Transition;
			public Sprite WireDoor_Closed;
		}

		[System.Serializable]
		public struct SushiSpritesDictionary
		{
			public Sprite SushiSprite;
			public Sprite SushiSprite_Ghost;

			public AnimationClip Sushi_BobAnimation;
			public AnimationClip Sushi_CollectAnimation;
		}

		#endregion

		public Sprite WallSprite;
		public SeamlessGraphicsDictionary SeamlessWallGraphics;
		public SeamlessGraphicsDictionary SeamlessWaterGraphics;
		public Sprite[] FinishFlagSprites;
		public Sprite RedirectTileSprite;
		public Sprite HaltTileSprite;
		public Sprite PitSprite;
		public Sprite[] PortalSprites;
		public Sprite SlidingCrateSprite;

		public JumpTileSpritesDictionary JumpTileSprites;
		public WireButtonSpritesDictionary WireButtonSprites;
		public WireDoorSpritesDictionary WireDoorSprites;
		public SushiSpritesDictionary SushiSprites;

		public UIDictionary UISprites;
	}
}
