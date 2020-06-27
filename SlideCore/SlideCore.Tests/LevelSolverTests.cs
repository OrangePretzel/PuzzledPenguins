using NUnit.Framework;
using SlideCore.Levels;

namespace SlideCore.Tests
{
	[TestFixture]
	public class LevelSolverTests
	{
		[Test]
		[TestCase(@"ValidLevels\Level_Valid_4_3", 10, "RDRUL")]
		[TestCase(@"ValidLevels\Level_Valid_8_1", 1, "URD")]
		[TestCase(@"ValidLevels\Level_Valid_RedirectIntoWall", 10, "URDLD")]
		[TestCase(@"ValidLevels\Level_Valid_Trap", 80, "ULDRURDLURD")]
		[TestCase(@"ValidLevels\Level_Valid_InfiniteLoop", 24, "LURULLD")]
		public void Test_LevelSolver_SolveLevels(string levelID, int totalExpectedSolutions, string expectedShortestSolution)
		{
			var level = TestHelper.LoadLevelForTest(levelID, true);

			var solutions = Level.Solver.SolveLevel(level, Level.Solver.SolveMode.AllSolutions);
			Assert.AreEqual(totalExpectedSolutions, solutions.Count);
			if (expectedShortestSolution != null)
				StringAssert.Contains($"[{expectedShortestSolution}]", solutions[0].ToString().Replace("_", ""));

#if DEBUG
			System.Text.StringBuilder allSolutionsBuilder = new System.Text.StringBuilder();
			allSolutionsBuilder.Append($"All solutions found ({solutions.Count}):\n");
			foreach (var solution in solutions)
				allSolutionsBuilder.Append($"{solution}\n");
			System.Console.WriteLine(allSolutionsBuilder.ToString());
#endif
		}
	}
}
