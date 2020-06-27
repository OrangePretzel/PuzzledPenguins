using NUnit.Framework;
using SlideCore.Levels;
using SlideCore.Math;

namespace SlideCore.Tests.Features
{
	[TestFixture]
	public class SushiFeatureTests
	{
		[Test]
		public void Test_Feature_SushiSolve()
		{
			LevelManager.LoadLevelPacks();
			var levelPack = LevelManager.LevelPacks[0];
			var level = LevelManager.GetLevel(levelPack, levelPack.Levels[0].ID);
			level.InitializeLevelForGame();

			var player = level.PlayerEntities[0];
			var sushi = level.SushiEntities[0];

			// Sanity Check
			Assert.AreEqual("Basics\\Level_01_1", level.Info.ID);
			Assert.AreEqual(new IntVector2(2, 1), player.Position);
			Assert.AreEqual(new IntVector2(5, 0), sushi.Position);
			Assert.AreEqual(1, level.SushiEntities.Count);
			Assert.IsFalse(sushi.IsCollected);
			Assert.IsFalse(level.HasCollectedAllSushi);

			// Collect Sushi
			var aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveUp,
				PlayerActions.MoveRight
			});
			Assert.IsTrue(sushi.IsCollected);
			Assert.IsTrue(level.HasCollectedAllSushi);

			// Finish Level
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveLeft,
				PlayerActions.MoveDown,
				PlayerActions.MoveRight,
				PlayerActions.MoveUp,
				PlayerActions.MoveLeft
			});
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.LevelComplete, aggregateUpdateResult.Result);
		}
	}
}
