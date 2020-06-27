using NUnit.Framework;
using SlideCore.Math;

namespace SlideCore.Tests.Features
{
	[TestFixture]
	public class PitFeatureTests
	{
		[Test]
		public void Test_Feature_Pit_WithPlayer()
		{
			var level = TestHelper.LoadLevelForTest("FeatureLevels\\Level_Feature_Pit", true);
			TestHelper.PerformActionSequence(level, new PlayerActions[] { PlayerActions.MoveDown });

			// Sanity Check
			Assert.AreEqual(new IntVector2(2, 3), level.PlayerEntities[0].Position);

			var aggregateUpdateResult = level.DoPlayerAction(PlayerActions.MoveRight);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.FatalResult, aggregateUpdateResult.Result);

			var playerUpdateResult = aggregateUpdateResult.EntityUpdateResults[level.PlayerEntities[0].ID];
			Assert.AreEqual(new IntVector2(4, 3), level.PlayerEntities[0].Position);
			Assert.AreEqual(UpdateResult.ResultTypes.FatalResult, playerUpdateResult.Result);

			// Undo
			level.DoPlayerAction(PlayerActions.Undo);
			Assert.AreEqual(new IntVector2(2, 3), level.PlayerEntities[0].Position);
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerUpdateResult.Result);
		}

		// TODO: Implement and write test for crate interacting with pit
		// TODO: Implement logic to prevent actions after FatalResults (until Undo is performed)
	}
}
