using NUnit.Framework;
using SlideCore.Levels;
using System;
using System.IO;
using System.Text;

namespace SlideCore.Tests
{
	[TestFixture]
	public class ContentTests
	{
		[Test]
		[Explicit]
		[TestCase("Basics")]
		[TestCase("Warehouse")]
		public void Test_LevelSolver_SolveLevels(string packName)
		{
			var contentDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "Level", packName);
			var levelFileList = Directory.GetFiles(contentDir, "*", SearchOption.AllDirectories);

			foreach (var levelFile in levelFileList)
			{
				var levelID = levelFile.Replace($"{contentDir}\\", "");
				var levelContent = File.ReadAllText(levelFile);
				var level = Level.Parser.ParseLevel(levelContent);
				level.InitializeLevelForGame();

				var solutions = Level.Solver.SolveLevel(level);
				Assert.IsTrue(solutions.Count >= 1, $"Level {levelID} expected to have at least one solution but none were found");

#if DEBUG
				StringBuilder allSolutionsBuilder = new StringBuilder();
				allSolutionsBuilder.Append($"[{levelID}] All solutions found ({solutions.Count}):\n");
				foreach (var solution in solutions)
					allSolutionsBuilder.Append($"[{levelID}] {solution}\n");
				allSolutionsBuilder.AppendLine(); // Adding an extra line for whitespace
				Console.WriteLine(allSolutionsBuilder.ToString());
#endif
			}
		}
	}
}
