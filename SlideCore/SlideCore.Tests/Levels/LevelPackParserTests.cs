using NUnit.Framework;
using SlideCore.Data;
using SlideCore.Levels;

namespace SlideCore.Tests.Levels
{
	[TestFixture]
	public class LevelPackParserTests
	{
		[Test]
		public void Test_LevelPackParser_InvalidPacks()
		{
			AssertParseLevelPackThrowsError(@"Invalid\Pack_NoPackName", @"Pack name was not valid \[\]");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_BadPackName", @"Pack name was not valid \[\s{4}\]");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_NoLevelsArray", @"Level pack did not contain any levels");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_NoLevels", @"Level pack did not contain any levels");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_BadLevelID", @"Level ID was not valid \[\s{2}\]");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_NoLevelDisplayName", @"Level display name was not valid \[\]");
			AssertParseLevelPackThrowsError(@"Invalid\Pack_BadLevelDisplayName", @"Level display name was not valid \[\s{2}\]");
		}

		[Test]
		public void Test_LevelPackParser_ValidPack()
		{
			const string PACK_ID = @"Valid\Test_Pack_1";
			var levelPack = LevelPack.Parser.ParseLevelPack(PACK_ID, TestHelper.LoadContent(ContentTypes.LevelPack, PACK_ID, true));
			Assert.AreEqual(PACK_ID, levelPack.ID);
			Assert.AreEqual("Test Pack 1", levelPack.DisplayName);

			int i = 0;
			foreach (var level in levelPack.Levels)
			{
				++i;
				Assert.AreEqual($"Level_{i}", level.ID);
				Assert.AreEqual($"Level {i}", level.DisplayName);
				Assert.AreEqual(Level.LevelInfo.LevelInfoStatus.Locked, level.Status);
			}
		}

		private void AssertParseLevelPackThrowsError(string packID, string errorRegex)
		{
			var ex = Assert.Throws<LevelPack.Parser.InvalidSerializedLevelPackException>(
				() => LevelPack.Parser.ParseLevelPack(packID, TestHelper.LoadContent(ContentTypes.LevelPack, packID, true)));
			StringAssert.IsMatch(errorRegex, ex.ToStacklessExceptionString());
		}
	}
}
