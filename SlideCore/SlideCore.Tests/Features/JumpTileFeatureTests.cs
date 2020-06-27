using NUnit.Framework;
using SlideCore.Math;

namespace SlideCore.Tests.Features
{
	[TestFixture]
	public class JumpTileFeatureTests
	{
		[Test]
		public void Test_Feature_JumpTile()
		{
			var level = TestHelper.LoadLevelForTest("FeatureLevels\\Level_Feature_JumpTile", true);
			var player = level.PlayerEntities[0];
			AggregateUpdateResult aggregateUpdateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position);
			Assert.AreEqual(0, level.Tick);

			// Jump
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(0, 4), player.Position);
			Assert.AreEqual(4, level.Tick);
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, aggregateUpdateResult.EntityUpdateResults[player.ID].Result);
			Assert.AreEqual(new IntVector2(0, 1), aggregateUpdateResult.EntityUpdateResults[player.ID].CollisionDir);

			// Undo after wall
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(4, level.Tick);

			// Reposition
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);

			// Jump Collide
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			Assert.AreEqual(new IntVector2(4, 2), player.Position);
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, aggregateUpdateResult.EntityUpdateResults[player.ID].Result);
			Assert.AreEqual(new IntVector2(0, -1), aggregateUpdateResult.EntityUpdateResults[player.ID].CollisionDir);

			// Jump Collide
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(new IntVector2(0, 2), player.Position);
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, aggregateUpdateResult.EntityUpdateResults[player.ID].Result);
			Assert.AreEqual(new IntVector2(-1, 0), aggregateUpdateResult.EntityUpdateResults[player.ID].CollisionDir);
			Assert.AreEqual(17, level.Tick);

			// Undo after wall
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(17, level.Tick);
		}
	}
}
