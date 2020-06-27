using CommandLine;
using SlideCore.Levels;
using System;
using System.IO;
using System.Text;

namespace SlideCLI
{
	[Verb("solver", HelpText = "Utility for solving levels")]
	public class SolveUtilityOptions
	{
		[Option(shortName: 'b', longName: "baseDir", Default = null, HelpText = "The base directory to search for level files under")]
		public string BaseDir { get; set; }

		[Option(shortName: 'l', longName: "levelIDs", Required = true, HelpText = "A comma seperated list of levels to solve")]
		public string LevelIDs { get; set; }

		public static int RunUtility(SolveUtilityOptions options)
		{
			var baseFileDir = options.BaseDir ?? Directory.GetCurrentDirectory();
			var levelIDs = options.LevelIDs.Split(',', StringSplitOptions.RemoveEmptyEntries);

			if (levelIDs.Length < 1) throw new Exception("No level IDs specified");

			foreach (var levelID in levelIDs)
			{
				var levelFileName = $"{levelID}.json";
				var levelFile = Path.Combine(baseFileDir, levelFileName);

				var serializedLevelString = File.ReadAllText(levelFile);
				var level = Level.Parser.ParseLevel(serializedLevelString);
				level.InitializeLevelForGame();

				Console.WriteLine($"Solving Level [{levelID}]");

				var solutions = Level.Solver.SolveLevel(level);

				StringBuilder outputBuider = new StringBuilder();
				outputBuider.AppendLine($"Level [{levelID}] has {solutions.Count} solutions");
				int solID = 0;
				foreach (var solution in solutions)
					outputBuider.AppendLine($"{++solID}. {solution.ToString()}");

				Console.WriteLine(outputBuider.ToString());
				Console.WriteLine();
			}

			return 0;
		}
	}

	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				return CommandLine.Parser.Default.ParseArguments<SolveUtilityOptions>(args)
					.MapResult(
						(SolveUtilityOptions opts) => SolveUtilityOptions.RunUtility(opts),
						errors =>
						{
							Console.WriteLine("##ERROR##");
							foreach (var error in errors)
							{
								Console.WriteLine(error);
							}
							Console.WriteLine("##ERROR##");
							return -1;
						});
			}
			catch (Exception ex)
			{
				Console.WriteLine("##BEGIN ERROR##");
				Console.WriteLine(ex);
				Console.WriteLine("##END ERROR##");
				return -1;
			}
		}
	}
}
