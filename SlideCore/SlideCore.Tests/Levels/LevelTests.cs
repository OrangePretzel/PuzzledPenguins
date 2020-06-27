using NUnit.Framework;
using SlideCore.Levels;
using SlideCore.Math;
using System;

namespace SlideCore.Tests.Levels
{
	[TestFixture]
	public class LevelTests
	{
		[Test]
		public void Test_LevelSnapshot_Equality()
		{
			var level = TestHelper.LoadLevelForTest(@"ValidLevels\Level_Valid_Trap", true);
			var player = level.PlayerEntities[0];

			// Initial Snapshots
			var initialSnapshot = level.InitializeSnapshot();
			var initialSnapshot2 = level.InitializeSnapshot();
			Assert.AreEqual(initialSnapshot, initialSnapshot2);

			// Move Down
			level.DoPlayerAction(PlayerActions.MoveDown);
			var downSnapshot = level.InitializeSnapshot();
			Assert.AreNotEqual(initialSnapshot, downSnapshot);

			// Move into Trap
			level.DoPlayerAction(PlayerActions.MoveRight);
			level.DoPlayerAction(PlayerActions.MoveUp);

			// Safety check
			Assert.AreEqual(new IntVector2(5, 5), player.Position);

			// Take snapshot
			var trapStartSnapshot = level.InitializeSnapshot();

			// Move around Trap
			level.DoPlayerAction(PlayerActions.MoveRight);
			level.DoPlayerAction(PlayerActions.MoveDown);

			// Safety check
			Assert.AreEqual(new IntVector2(7, 7), player.Position);

			// Move back to trap start
			level.DoPlayerAction(PlayerActions.Undo);
			level.DoPlayerAction(PlayerActions.Undo);

			// Snapshot check
			var trapEndSnapshot = level.InitializeSnapshot();
			Assert.AreEqual(trapStartSnapshot, trapEndSnapshot);
		}

		[Test]
		public void Test_Level_SetInfo()
		{
			var level = TestHelper.LoadLevelForTest(@"ValidLevels\Level_Valid_Trap", true);

			Assert.AreEqual(null, level.Info);

			level.SetInfo("LEVEL_ID", "LEVEL_DISPLAY_NAME", 3);

			Assert.AreEqual("LEVEL_ID", level.Info.ID);
			Assert.AreEqual("LEVEL_DISPLAY_NAME", level.Info.DisplayName);
			Assert.AreEqual(3, level.Info.Index);
		}

		[Test]
		public void Test_Level_InvalidActions()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_03_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// No Action Taken
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.None);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.Cancelled, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 5), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");

			// Can't move into wall
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.Cancelled, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 5), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");

			// Can't move into edge
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.Cancelled, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 5), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");
		}

		[Test]
		public void Test_Level_CollideWithWallsAndEdges()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_01_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Up Edge
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(2, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 0), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
			Assert.AreEqual(new IntVector2(0, -1), playerResult.CollisionDir, "CollisionDir");

			// Down Wall
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(4, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
			Assert.AreEqual(new IntVector2(0, 1), playerResult.CollisionDir, "CollisionDir");

			// Left Edge
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(3, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(0, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
			Assert.AreEqual(new IntVector2(-1, 0), playerResult.CollisionDir, "CollisionDir");

			// Right Edge
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(6, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(5, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
			Assert.AreEqual(new IntVector2(1, 0), playerResult.CollisionDir, "CollisionDir");
		}

		[Test]
		public void Test_Level_Undo()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_01_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Perform some moves
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(new IntVector2(5, 0), player.Position, "Player position after initial moves was wrong");

			// Undo 1: Success
			Assert.IsTrue(level.CanUndo);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.Undo);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.UndoPerformed, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(0, 0), player.Position, "PlayerPosition");
			Assert.AreEqual(new IntVector2(0, 0), player.Motion, "PlayerMotion");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");

			// Undo 2: Success
			Assert.IsTrue(level.CanUndo);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.Undo);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.UndoPerformed, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(0, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");

			// Undo 3: Success
			Assert.IsTrue(level.CanUndo);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.Undo);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.UndoPerformed, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");

			// Undo 4: Fail
			Assert.IsFalse(level.CanUndo);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.Undo);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.Cancelled, aggregateResult.Result);
			Assert.AreEqual(0, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.None, playerResult.Result, "ResultType");
		}

		[Test]
		public void Test_Level_Finish()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_01_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Inital Moves
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[] {
				PlayerActions.MoveDown,
				PlayerActions.MoveRight,
				PlayerActions.MoveUp,
			});
			Assert.AreEqual(new IntVector2(5, 2), player.Position, "Player position after initial moves was wrong");

			// Down to finish
			aggregateResult = level.DoPlayerAction(PlayerActions.MoveLeft);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.LevelComplete, aggregateResult.Result);
			Assert.AreEqual(2, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(3, 2), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.ReachedFinish, playerResult.Result, "ResultType");

		}

		[Test]
		public void Test_Level_RedirectTiles()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_03_5");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Inital Moves
			Assert.AreEqual(new IntVector2(8, 7), player.Position, "Player position after initial moves was wrong");

			// Down to infinite loop
			aggregateResult = level.DoPlayerAction(PlayerActions.MoveDown);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(1, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(8, 8), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");
			Assert.AreEqual(level.StaticEntities[new IntVector2(8, 8)], playerResult.InteractionEntity);

			aggregateResult = level.UpdateLevel();
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(7, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(1, 8), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");

			aggregateResult = level.UpdateLevel();
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(2, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(1, 6), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");

			aggregateResult = level.UpdateLevel();
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(5, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(1, 1), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");

			aggregateResult = level.UpdateLevel();
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(4, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(5, 1), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");

			// The loop will keep going forever!
		}

		[Test]
		public void Test_Level_RedirectIntoWall()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_03_4");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Inital Moves
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(5, 1), player.Position, "Player position after initial moves was wrong");

			// Left to redirect into wall
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(3, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 1), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
			Assert.AreEqual(new IntVector2(0, -1), playerResult.CollisionDir, "CollisionDir");
		}

		[Test]
		public void Test_Level_RedirectToSamePosition()
		{
			var level = TestHelper.LoadLevelForTest(@"ValidLevels\Level_Valid_RedirectIntoWall", true);
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;

			// Inital Moves
			Assert.AreEqual(new IntVector2(4, 5), player.Position, "Player position after initial moves was wrong");

			// Position Crate + Player
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]{
				PlayerActions.MoveRight
			});

			// Sanity Check
			Assert.AreEqual(new IntVector2(5, 5), player.Position);
			Assert.AreEqual(2, level.Tick);
			Assert.AreEqual(1, level.Move);
			Assert.AreEqual(new IntVector2(1, 0), aggregateResult.EntityUpdateResults[player.ID].CollisionDir);

			// Move left into redirect arrow and back
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(new IntVector2(5, 5), player.Position);
			Assert.AreEqual(7, level.Tick);
			Assert.AreEqual(2, level.Move);
			Assert.AreEqual(new IntVector2(1, 0), aggregateResult.EntityUpdateResults[player.ID].CollisionDir);
		}

		[Test]
		public void Test_Level_HaltTiles()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_04_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Inital Moves
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveUp);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveDown);
			Assert.AreEqual(new IntVector2(5, 1), player.Position, "Player position after initial moves was wrong");

			// Left to halt tile
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(3, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 1), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");
		}

		[Test]
		public void Test_Level_PortalTiles()
		{
			var level = TestHelper.LoadLevelForTest(@"Basics/Level_05_1");
			var player = level.PlayerEntities[0];

			AggregateUpdateResult aggregateResult;
			UpdateResult playerResult = player.GetUpdateResult();

			// Inital Moves
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveLeft);
			Assert.AreEqual(new IntVector2(2, 5), player.Position, "Player position after initial moves was wrong");

			// Up to portal
			aggregateResult = level.DoPlayerAction(PlayerActions.MoveUp);
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(2, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(2, 3), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.DelayAction, playerResult.Result, "ResultType"); // Teleport delayed till next update

			aggregateResult = level.UpdateLevel(); // Teleport
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(1, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(4, 1), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Teleported, playerResult.Result, "ResultType");

			aggregateResult = level.UpdateLevel(); // Continue moving
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.RequiresUpdate, aggregateResult.Result);
			Assert.AreEqual(1, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(4, 0), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Redirected, playerResult.Result, "ResultType");

			aggregateResult = level.UpdateLevel(); // Continue moving
			Assert.AreEqual(AggregateUpdateResult.ResultTypes.DoneUpdating, aggregateResult.Result);
			Assert.AreEqual(5, aggregateResult.TicksTaken, "TicksTaken");
			Assert.AreEqual(new IntVector2(0, 0), player.Position, "PlayerPosition");
			Assert.AreEqual(UpdateResult.ResultTypes.Collided, playerResult.Result, "ResultType");

			// Reposition + sanity check
			aggregateResult = TestHelper.PerformActionSequence(level, new PlayerActions[]{
				PlayerActions.MoveDown,
				PlayerActions.MoveRight,
				PlayerActions.MoveDown,
				PlayerActions.MoveLeft,
				PlayerActions.MoveUp,
			});
			Assert.AreEqual(new IntVector2(0, 3), player.Position);

			// Right to portal
			aggregateResult = TestHelper.PerformAction(level, PlayerActions.MoveRight);
			Assert.AreEqual(new IntVector2(5, 1), player.Position, "PlayerPosition");
		}

		[Test]
		public void Test_Level_UndoInInfiniteLoop()
		{
			var level = TestHelper.LoadLevelForTest(@"ValidLevels\Level_Valid_InfiniteLoop", true);

			// Move down into infinite loop
			var result = level.DoPlayerAction(PlayerActions.MoveDown);
			result = level.UpdateLevel(); // Update (still in loop)

			result = level.DoPlayerAction(PlayerActions.Undo); // Undo

			result = level.DoPlayerAction(PlayerActions.MoveLeft); // Move Left
			result = level.DoPlayerAction(PlayerActions.MoveUp); // Move Up
		}

		[Test]
		public void Test_Level_ResetInInfiniteLoop()
		{
			var level = TestHelper.LoadLevelForTest(@"ValidLevels\Level_Valid_InfiniteLoop", true);

			// Move down into infinite loop
			var result = level.DoPlayerAction(PlayerActions.MoveDown);
			result = level.UpdateLevel(); // Update (still in loop)

			level.ResetLevel();

			result = level.DoPlayerAction(PlayerActions.MoveLeft); // Move Left
			result = level.DoPlayerAction(PlayerActions.MoveUp); // Move Up
		}
	}
}
