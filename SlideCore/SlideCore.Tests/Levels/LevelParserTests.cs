using NUnit.Framework;
using SlideCore.Data;
using SlideCore.Entities;
using SlideCore.Levels;
using SlideCore.Math;
using System.Collections.Generic;
using System.IO;

namespace SlideCore.Tests.Levels
{
	[TestFixture]
	public class LevelParserTests
	{
		// TODO: Add more robust tests around entity parsing

		[Test]
		public void Test_LevelParser_ParseLevel_V1_Success()
		{
			// Level 1
			Level level1 = Level.Parser.ParseLevel(TestHelper.LoadContent(ContentTypes.Level, @"ValidLevels\Level_Valid_4_3", true));

			Assert.AreEqual(null, level1.Info);
			Assert.AreEqual(6, level1.LevelWidth);
			Assert.AreEqual(7, level1.LevelHeight);

			CollectionAssert.AreEqual(
				new List<StaticEntity>()
				{
					new StaticEntity(EntityTypes.FinishFlag, 1, 3, 2),
					new StaticEntity(EntityTypes.Wall, 2, 1, 1),
					new StaticEntity(EntityTypes.Wall, 3, 5, 1),
					new StaticEntity(EntityTypes.Wall, 4, 2, 2)
				}, level1.StaticEntities);
			Assert.AreEqual(1, level1.PlayerEntities.Count);
			Assert.AreEqual(0, level1.DynamicEntities.Count);
			Assert.AreEqual(new IntVector2(2, 1), level1.PlayerEntities[0].Position); // Player Position
			Assert.AreEqual(4, level1.TargetMoves);

			// Level 2
			Level level2 = Level.Parser.ParseLevel(TestHelper.LoadContent(ContentTypes.Level, @"ValidLevels\Level_Valid_8_1", true));

			Assert.AreEqual(null, level2.Info);
			Assert.AreEqual(3, level2.LevelWidth);
			Assert.AreEqual(2, level2.LevelHeight);

			CollectionAssert.AreEqual(
				new List<Entity>()
				{
					new StaticEntity(EntityTypes.FinishFlag, 1, 2, 1),
					new StaticEntity(EntityTypes.Wall, 2, 1, 1)
				}, level2.StaticEntities);
			Assert.AreEqual(1, level2.PlayerEntities.Count);
			Assert.AreEqual(0, level2.DynamicEntities.Count);
			Assert.AreEqual(new IntVector2(0, 1), level2.PlayerEntities[0].Position); // Player Position
		}

		[Test]
		public void Test_LevelParser_ParseLevel_V1_FailsOnBadMetaData()
		{
			AssertLevelParseThrowsException("Unknown parser version [INVALID_PARSER]", @"InvalidLevels\Level_InvalidLevelParser");
			AssertLevelParseThrowsException("Invalid level width specified", @"InvalidLevels\Level_InvalidLevelWidth");
			AssertLevelParseThrowsException("Invalid level height specified", @"InvalidLevels\Level_InvalidLevelHeight");
			AssertLevelParseThrowsException("Invalid level entities specified", @"InvalidLevels\Level_InvalidLevelEntities");

			// TODO: The following line isn't bad metadata... move to another test
			AssertLevelParseThrowsException("Could not convert 'InvalidEntity' to EntityTypes", @"InvalidLevels\Level_InvalidEntityType");
		}

		[Test]
		public void Test_LevelParser_ParseLevel_V1_FailsOnNoRequiredEntities()
		{
			AssertLevelParseThrowsException("No player was present in level", @"InvalidLevels\Level_NoPlayerPresent");
			AssertLevelParseThrowsException("No finish flag was present in level", @"InvalidLevels\Level_NoFinishPresent");
		}

		private void AssertLevelParseThrowsException(string expectedExceptionMessage, string levelName)
		{
			var serializedLevel = TestHelper.LoadContent(ContentTypes.Level, levelName, true);
			var ex = Assert.Throws<InvalidSerializedContentException>(() => Level.Parser.ParseLevel(serializedLevel));
			StringAssert.Contains("Unable to parse level", ex.Message);
			StringAssert.Contains(expectedExceptionMessage, ex.InnerException?.Message);
		}
	}
}
