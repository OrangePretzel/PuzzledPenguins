using NUnit.Framework;
using SlideCore.Levels;
using System;

namespace SlideCore.Tests.Levels
{
	[TestFixture]
	public class LevelManagerTests
	{
		[Test]
		public void Test_LevelManager_InitialState()
		{
			LevelManager.LoadLevelPacks();

			var firstLevelPack = LevelManager.LevelPacks[0];
			Assert.IsFalse(firstLevelPack.IsLocked());
			Assert.IsFalse(firstLevelPack.IsSolved());
			Assert.AreEqual(0, firstLevelPack.SolvedLevelCount);
			Assert.AreEqual(25, firstLevelPack.TotalLevelCount);

			for (int i = 1; i < LevelManager.LevelPacks.Count; i++)
			{
				var levelPack = LevelManager.LevelPacks[i];
				Assert.IsTrue(levelPack.IsLocked());
				Assert.IsFalse(levelPack.IsSolved());
				Assert.AreEqual(0, levelPack.SolvedLevelCount);
				Assert.AreEqual(25, levelPack.TotalLevelCount);
			}
		}

		[Test]
		public void Test_LevelManager_UnlockSecondPack()
		{
			LevelManager.LoadLevelPacks();

			var firstPack = LevelManager.LevelPacks[0];
			var secondPack = LevelManager.LevelPacks[1];

			// Sanity Check
			Assert.IsFalse(firstPack.IsSolved());
			Assert.IsFalse(firstPack.IsLocked());
			Assert.IsFalse(secondPack.IsSolved());
			Assert.IsTrue(secondPack.IsLocked());

			// Solve all levels in first level pack
			foreach (var levelInfo in firstPack.Levels)
				LevelManager.SolveLevel(firstPack, levelInfo.ID, false, false);

			Assert.IsTrue(firstPack.IsSolved());
			Assert.IsFalse(firstPack.IsLocked());
			Assert.IsFalse(secondPack.IsSolved());
			Assert.IsFalse(secondPack.IsLocked());
		}

		[Test]
		public void Test_LevelManager_MaintainsPreviousProgress()
		{
			LevelManager.LoadLevelPacks();

			var levelPack = LevelManager.LevelPacks[0];

			// Test Regular Solved
			var solvedLevelInfo = levelPack.Levels[0];
			Assert.IsFalse(solvedLevelInfo.IsSolved);
			Assert.IsFalse(solvedLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(solvedLevelInfo.IsSolvedWithSushi);
			LevelManager.SolveLevel(levelPack, solvedLevelInfo.ID, false, false); // Solve
			Assert.IsTrue(solvedLevelInfo.IsSolved);
			Assert.IsFalse(solvedLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(solvedLevelInfo.IsSolvedWithSushi);

			// Test Min Moves
			var movesTestLevelInfo = levelPack.Levels[1];
			Assert.IsFalse(movesTestLevelInfo.IsSolved);
			Assert.IsFalse(movesTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(movesTestLevelInfo.IsSolvedWithSushi);
			LevelManager.SolveLevel(levelPack, movesTestLevelInfo.ID, true, false); // Solve
			Assert.IsTrue(movesTestLevelInfo.IsSolved);
			Assert.IsTrue(movesTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(movesTestLevelInfo.IsSolvedWithSushi);

			// Test Sushi
			var sushiTestLevelInfo = levelPack.Levels[2];
			Assert.IsFalse(sushiTestLevelInfo.IsSolved);
			Assert.IsFalse(sushiTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(sushiTestLevelInfo.IsSolvedWithSushi);
			LevelManager.SolveLevel(levelPack, sushiTestLevelInfo.ID, false, true); // Solve
			Assert.IsTrue(sushiTestLevelInfo.IsSolved);
			Assert.IsFalse(sushiTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsTrue(sushiTestLevelInfo.IsSolvedWithSushi);

			// Test Full Solve (at once)
			var singleFullTestLevelInfo = levelPack.Levels[3];
			Assert.IsFalse(singleFullTestLevelInfo.IsSolved);
			Assert.IsFalse(singleFullTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(singleFullTestLevelInfo.IsSolvedWithSushi);
			LevelManager.SolveLevel(levelPack, singleFullTestLevelInfo.ID, true, true); // Solve
			Assert.IsTrue(singleFullTestLevelInfo.IsSolved);
			Assert.IsTrue(singleFullTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsTrue(singleFullTestLevelInfo.IsSolvedWithSushi);

			// Test Full Solve (apart)
			var multiFullTestLevelInfo = levelPack.Levels[4];
			Assert.IsFalse(multiFullTestLevelInfo.IsSolved);
			Assert.IsFalse(multiFullTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsFalse(multiFullTestLevelInfo.IsSolvedWithSushi);
			LevelManager.SolveLevel(levelPack, multiFullTestLevelInfo.ID, true, false); // Solve
			LevelManager.SolveLevel(levelPack, multiFullTestLevelInfo.ID, false, true); // Solve
			Assert.IsTrue(multiFullTestLevelInfo.IsSolved);
			Assert.IsTrue(multiFullTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsTrue(multiFullTestLevelInfo.IsSolvedWithSushi);

			// Check that progress is maintained
			LevelManager.SolveLevel(levelPack, multiFullTestLevelInfo.ID, false, false); // Solve
			Assert.IsTrue(multiFullTestLevelInfo.IsSolved);
			Assert.IsTrue(multiFullTestLevelInfo.IsSolvedWithinMinMoves);
			Assert.IsTrue(multiFullTestLevelInfo.IsSolvedWithSushi);
		}

		[Test]
		public void Test_LevelManager_SolveNonExistantLevel()
		{
			// Get the first level of the first level pack
			LevelManager.LoadLevelPacks();
			var levelPack0 = LevelManager.LevelPacks[0];
			var levelInfo0 = levelPack0.Levels[0];
			var levelInfo1 = levelPack0.Levels[1];

			// Sanity check
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Unsolved, levelInfo0.Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Locked, levelInfo1.Status);

			// Solve an invalid level
			var ex = Assert.Throws<Exception>(() => LevelManager.SolveLevel(levelPack0, "NONEXISTANT_LEVEL", false, false));

			StringAssert.IsMatch("No Level loaded with ID NONEXISTANT_LEVEL", ex.Message);
		}

		[Test]
		public void Test_LevelManager_CorrectlyStoresProgress()
		{
			// Load a pack & progress
			LevelManager.LoadLevelPacks();
			var levelPack = LevelManager.LevelPacks[0];

			// Assert current progress
			Assert.IsFalse(levelPack.IsSolved());
			Assert.AreEqual(0, levelPack.SolvedLevelCount);
			Assert.AreEqual(25, levelPack.TotalLevelCount);

			// Solve some levels and & save new progress
			LevelManager.SolveLevel(levelPack, levelPack.Levels[0].ID, false, false);
			LevelManager.SolveLevel(levelPack, levelPack.Levels[1].ID, true, false);
			LevelManager.SolveLevel(levelPack, levelPack.Levels[2].ID, false, true);
			LevelManager.SolveLevel(levelPack, levelPack.Levels[3].ID, true, true);

			// Manually load a the same pack & progress again
			var levelPack2 = TestHelper.LoadLevelPackForTest(levelPack.ID, false);

			// Assert updated progress
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Solved, levelPack2.Levels[0].Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.SolvedWithMinMoves, levelPack2.Levels[1].Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.SolvedWithSushi, levelPack2.Levels[2].Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.FullySolved, levelPack2.Levels[3].Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Unsolved, levelPack2.Levels[4].Status);
			Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Locked, levelPack2.Levels[5].Status);
		}
	}
}
