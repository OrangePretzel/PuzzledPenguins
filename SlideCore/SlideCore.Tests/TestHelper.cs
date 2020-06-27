using NUnit.Framework;
using SlideCore.Data;
using SlideCore.Levels;
using System;
using System.IO;

namespace SlideCore.Tests
{
	[SetUpFixture]
	public class TestHelper
	{
		private InMemoryDataStore DataStore;
		private TestContentStore ContentStore;

		[OneTimeSetUp]
		public void SetupTests()
		{
			DataStore = new InMemoryDataStore();
			ContentStore = new TestContentStore();
		}

		[OneTimeTearDown]
		public void TearDownTests()
		{
			DataStore.Dispose();
			ContentStore.Dispose();
		}

		public static string LoadContent(ContentTypes contentType, string contentID, bool isTestContent) =>
        ContentManager.LoadContent(contentType, isTestContent ? Path.Combine(TestContentStore.TEST_PREFIX,contentID) : contentID);

		public static Level LoadLevelForTest(string levelID, bool isTestContent = false)
		{
            var levelContent = ContentManager.LoadContent(ContentTypes.Level, isTestContent ? Path.Combine(TestContentStore.TEST_PREFIX, levelID) : levelID);
			var level = Level.Parser.ParseLevel(levelContent);
			level.InitializeLevelForGame();
			return level;
		}

		public static LevelPack LoadLevelPackForTest(string levelPackID, bool isTestContent = false)
		{
            var packContent = ContentManager.LoadContent(ContentTypes.LevelPack, isTestContent ? Path.Combine(TestContentStore.TEST_PREFIX, levelPackID) : levelPackID);
			var levelPack = LevelPack.Parser.ParseLevelPack(levelPackID, packContent);
			levelPack.LoadProgressForPack();
			return levelPack;
		}

		public static AggregateUpdateResult PerformAction(Level level, PlayerActions action, int maxTicks = 100)
		{
			int ticks = 0;
			var result = level.DoPlayerAction(action);
			++ticks;
			while (result.Result == AggregateUpdateResult.ResultTypes.RequiresUpdate)
			{
				if (++ticks > maxTicks) throw new Exception($"Action [{action}] didn't finish updating within {maxTicks}");
				result = level.UpdateLevel();
			}
			return result;
		}

		public static AggregateUpdateResult PerformActionSequence(Level level, PlayerActions[] actionSequence, int maxTicks = 100)
		{
			if (actionSequence.Length < 1) throw new ArgumentOutOfRangeException("Must have at least one action to perform");

			AggregateUpdateResult result = new AggregateUpdateResult();
			int ticks = 0;
			for (int i = 0; i < actionSequence.Length; i++)
			{
				result = level.DoPlayerAction(actionSequence[i]);
				++ticks;
				while (result.Result == AggregateUpdateResult.ResultTypes.RequiresUpdate)
				{
					if (++ticks > maxTicks) throw new Exception($"Action [{actionSequence[i]}] sequence didn't finish updating within {maxTicks}");
					result = level.UpdateLevel();
				}
			}
			return result;
		}
	}

	public static class TestHelperExtensions
	{
		public static string ToStacklessExceptionString(this Exception ex)
		{
			string exStr = "";
			var curr = ex;
			while (curr != null)
			{
				exStr += $"{curr.Message}\n";
				curr = curr.InnerException;
			}
			return exStr;
		}
	}
}
