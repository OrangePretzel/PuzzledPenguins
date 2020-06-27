using NUnit.Framework;
using SlideCore.Entities;
using SlideCore.Math;

namespace SlideCore.Tests.Features
{
	[TestFixture]
	public class WireButtonFeatureTests
	{
		// TODO: Test multiple buttons affecting one door (should throw error)?

		[Test]
		public void Test_Feature_WireButton_PressOnly()
		{
			var level = TestHelper.LoadLevelForTest("FeatureLevels\\Level_Feature_ToggleButton_PressOnly", true);
			var player = level.PlayerEntities[0];
			var button = (WireButtonEntity)level.StaticEntities[new IntVector2(4, 2)];
			var door = (WireDoorEntity)level.StaticEntities[new IntVector2(2, 4)];
			AggregateUpdateResult aggregateUpdateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position);
			Assert.AreEqual(0, level.Tick);
			Assert.IsFalse(button.IsPressed);
			Assert.IsFalse(door.IsOpen);

			// Collide with door
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveDown,
				PlayerActions.MoveDown,
				PlayerActions.MoveRight
			});
			Assert.AreEqual(new IntVector2(1, 4), player.Position);
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, aggregateUpdateResult.EntityUpdateResults[player.ID].Result);
			Assert.AreEqual(new IntVector2(1, 0), aggregateUpdateResult.EntityUpdateResults[player.ID].CollisionDir);

			// Interact with button
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveUp,
				PlayerActions.MoveRight,
				PlayerActions.MoveDown
			});
			Assert.AreEqual(new IntVector2(4, 2), player.Position);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);

			// Goto Finish
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveLeft,
				PlayerActions.MoveDown,
				PlayerActions.MoveRight
			});
			Assert.AreEqual(new IntVector2(4, 4), player.Position);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.LevelComplete, aggregateUpdateResult.Result);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);

		}

		[Test]
		public void Test_Feature_WireButton_Toggle()
		{
			var level = TestHelper.LoadLevelForTest("FeatureLevels\\Level_Feature_ToggleButton_Toggle", true);
			var player = level.PlayerEntities[0];
			var button = (WireButtonEntity)level.StaticEntities[new IntVector2(4, 1)];
			var door = (WireDoorEntity)level.StaticEntities[new IntVector2(2, 4)];
			AggregateUpdateResult aggregateUpdateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position);
			Assert.AreEqual(0, level.Tick);
			Assert.IsFalse(button.IsPressed);
			Assert.IsFalse(door.IsOpen);

			// Interact with button 1
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveRight,
				PlayerActions.MoveDown,
			});
			Assert.AreEqual(new IntVector2(4, 2), player.Position);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);

			// Interact with button 2
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			Assert.AreEqual(new IntVector2(4, 0), player.Position);
			Assert.IsFalse(button.IsPressed);
			Assert.IsFalse(door.IsOpen);

			// Interact with button 3
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(4, 2), player.Position);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);
		}

		[Test]
		public void Test_Feature_WireButton_Hold()
		{
			var level = TestHelper.LoadLevelForTest("FeatureLevels\\Level_Feature_ToggleButton_Hold", true);
			var player = level.PlayerEntities[0];
			var button = (WireButtonEntity)level.StaticEntities[new IntVector2(4, 2)];
			var door = (WireDoorEntity)level.StaticEntities[new IntVector2(2, 4)];
			AggregateUpdateResult aggregateUpdateResult;

			// Sanity Check
			Assert.AreEqual(new IntVector2(0, 0), player.Position);
			Assert.AreEqual(0, level.Tick);
			Assert.IsFalse(button.IsPressed);
			Assert.IsFalse(door.IsOpen);

			// Move onto button
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveRight,
				PlayerActions.MoveDown,
			});
			Assert.AreEqual(new IntVector2(4, 2), player.Position);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);

			// Move off button
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			Assert.AreEqual(new IntVector2(4, 0), player.Position);
			Assert.IsFalse(button.IsPressed);
			Assert.IsFalse(door.IsOpen);

			// Move crate onto button
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveLeft,
				PlayerActions.MoveDown,
				PlayerActions.MoveRight,
			});
			Assert.AreEqual(new IntVector2(1, 2), player.Position);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);

			// Push blocked crate on top of button
			// TODO: Move the blocked crate test to a separate unit test
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			int originalTick = level.Tick;
			aggregateUpdateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(originalTick, level.Tick);

			// Goto Finish
			// TODO: Move the Undo to a separate unit test
			aggregateUpdateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveLeft,
				PlayerActions.MoveDown,
				PlayerActions.Undo,
				PlayerActions.MoveDown,
				PlayerActions.MoveRight
			});
			Assert.AreEqual(new IntVector2(4, 4), player.Position);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.LevelComplete, aggregateUpdateResult.Result);
			Assert.IsTrue(button.IsPressed);
			Assert.IsTrue(door.IsOpen);
		}
	}
}
