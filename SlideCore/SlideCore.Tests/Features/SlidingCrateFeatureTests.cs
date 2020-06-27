using NUnit.Framework;
using SlideCore.Entities;
using SlideCore.Math;
using System.Linq;

namespace SlideCore.Tests.Features
{
	[TestFixture]
	public class SlidingCrateFeatureTests
	{
		[Test]
		public void Test_SlidingCrate_UndoAfterPushingBlockedCrate()
		{
			var level = TestHelper.LoadLevelForTest(@"FeatureLevels\Level_Feature_SlidingCrate", true);

			var player = level.PlayerEntities[0];
			var slidingCrates = level.DynamicEntities.Where(e => e.EntityType == EntityTypes.SlidingCrate).ToList();

			AggregateUpdateResult aggregateResult;

			// Inital Moves
			Assert.AreEqual(new IntVector2(1, 1), player.Position, "Player position after initial moves was wrong");

			// Position Crate + Player
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]{
				PlayerActions.MoveRight,
				PlayerActions.MoveRight,
				PlayerActions.MoveUp,
				PlayerActions.MoveRight,
				PlayerActions.MoveDown,
				PlayerActions.MoveDown
			});

			// Verify starting positions and time
			Assert.AreEqual(new IntVector2(5, 3), player.Position);
			Assert.AreEqual(new IntVector2(5, 4), slidingCrates[0].Position);
			Assert.AreEqual(23, level.Tick);
			Assert.AreEqual(6, level.Move);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[player.ID].CollisionDir);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[slidingCrates[0].ID].CollisionDir);

			// Move Down
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(5, 3), player.Position);
			Assert.AreEqual(new IntVector2(5, 4), slidingCrates[0].Position);
			Assert.AreEqual(23, level.Tick); // Blocked crate should not update time
			Assert.AreEqual(6, level.Move);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[player.ID].CollisionDir);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[slidingCrates[0].ID].CollisionDir);

			// Clear Collision Dirs
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]{
				PlayerActions.MoveLeft,
				PlayerActions.MoveRight
			});

			// Sanity Check
			Assert.AreEqual(35, level.Tick);
			Assert.AreEqual(8, level.Move);

			// Move Down
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(5, 3), player.Position);
			Assert.AreEqual(new IntVector2(5, 4), slidingCrates[0].Position);
			Assert.AreEqual(35, level.Tick); // Blocked crate should not update time
			Assert.AreEqual(8, level.Move);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[player.ID].CollisionDir);
			Assert.AreEqual(new IntVector2(0, 1), aggregateResult.EntityUpdateResults[slidingCrates[0].ID].CollisionDir);

			// Undo
			aggregateResult = level.DoPlayerAction(PlayerActions.Undo);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.UndoPerformed, aggregateResult.Result);
			Assert.AreEqual(new IntVector2(0, 3), player.Position);
			Assert.AreEqual(new IntVector2(5, 4), slidingCrates[0].Position);
			Assert.AreEqual(29, level.Tick);
			Assert.AreEqual(7, level.Move);
		}

		[Test]
		public void Test_SlidingCrate_RedirectToPlayer()
		{
			var level = TestHelper.LoadLevelForTest(@"FeatureLevels\Level_Feature_SlidingCrate_RedirectToPlayer", true);

			var player = level.PlayerEntities[0];
			var slidingCrate = level.DynamicEntities.Where(e => e.EntityType == EntityTypes.SlidingCrate).First();

			AggregateUpdateResult aggregateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position, "Player position after initial moves was wrong");

			// Move into crate
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(new IntVector2(1, 0), player.Position);
			Assert.AreEqual(new IntVector2(2, 0), slidingCrate.Position);
			Assert.AreEqual(new IntVector2(-1, 0), aggregateResult.EntityUpdateResults[slidingCrate.ID].CollisionDir);
			Assert.AreEqual(7, level.Tick);
		}

		[Test]
		public void Test_SlidingCrate_HoldOpenDoor()
		{
			var level = TestHelper.LoadLevelForTest(@"FeatureLevels\Level_Feature_SlidingCrate_HoldOpenDoor", true);

			var player = level.PlayerEntities[0];
			var slidingCrate = level.DynamicEntities.Where(e => e.EntityType == EntityTypes.SlidingCrate).First();
			var door = (WireDoorEntity)level.StaticEntities.First(e => e.EntityType == EntityTypes.WireDoor);

			AggregateUpdateResult aggregateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position, "Player position after initial moves was wrong");
			Assert.AreEqual(new IntVector2(3, 2), slidingCrate.Position);
			Assert.IsFalse(door.IsOpen);

			// Open door
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveDown,
				PlayerActions.MoveRight,
				PlayerActions.MoveDown,
				PlayerActions.MoveLeft,
			});

			// Sanity Check
			Assert.AreEqual(new IntVector2(3, 3), player.Position);
			Assert.AreEqual(new IntVector2(3, 2), slidingCrate.Position);
			Assert.IsTrue(door.IsOpen);

			// Move into crate into door
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			Assert.AreEqual(new IntVector2(3, 3), player.Position);
			Assert.AreEqual(new IntVector2(3, 0), slidingCrate.Position);
			Assert.IsTrue(door.IsOpen);

			// Close door on crate
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveLeft,
				PlayerActions.MoveUp,
				PlayerActions.MoveRight,
			});
			Assert.AreEqual(new IntVector2(5, 1), player.Position);
			Assert.AreEqual(new IntVector2(3, 0), slidingCrate.Position);
			Assert.IsTrue(door.IsOpen);

			// Push crate out of door
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveUp,
				PlayerActions.MoveLeft,
			});
			Assert.AreEqual(new IntVector2(4, 0), player.Position);
			Assert.AreEqual(new IntVector2(0, 0), slidingCrate.Position);
			Assert.IsFalse(door.IsOpen);
		}

		[Test]
		public void Test_SlidingCrate_StopInOpenDoor()
		{
			var level = TestHelper.LoadLevelForTest(@"FeatureLevels\Level_Feature_SlidingCrate_StopInOpenDoor", true);

			var player = level.PlayerEntities[0];
			var slidingCrate = level.DynamicEntities.Where(e => e.EntityType == EntityTypes.SlidingCrate).First();
			var door = (WireDoorEntity)level.StaticEntities.First(e => e.EntityType == EntityTypes.WireDoor);

			AggregateUpdateResult aggregateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position, "Player position after initial moves was wrong");
			Assert.AreEqual(new IntVector2(2, 0), slidingCrate.Position);
			Assert.IsFalse(door.IsOpen);

			// Bump crate into closed door
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(new IntVector2(1, 0), player.Position);
			Assert.AreEqual(new IntVector2(3, 0), slidingCrate.Position);
			Assert.IsFalse(door.IsOpen);

			// Open door, move crate into open door
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]
			{
				PlayerActions.MoveDown,
				PlayerActions.MoveLeft,
				PlayerActions.MoveUp,
				PlayerActions.MoveRight,
			});
			Assert.AreEqual(new IntVector2(2, 0), player.Position);
			Assert.AreEqual(new IntVector2(4, 0), slidingCrate.Position);
			Assert.IsTrue(door.IsOpen);
		}

		[Test]
		public void Test_SlidingCrate_TestEndlessError()
		{
			var level = TestHelper.LoadLevelForTest(@"FeatureLevels\Level_Feature_SlidingCrate_HoldButtonAndDoor", true);

			var player = level.PlayerEntities[0];
			AggregateUpdateResult aggregateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(9, 6), player.Position, "Player position after initial moves was wrong");

			// This is a long integration test. This can be maybe be removed?
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.LevelComplete, aggregateResult.Result);
		}
	}
}
