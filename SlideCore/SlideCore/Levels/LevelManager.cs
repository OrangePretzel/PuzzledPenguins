using Newtonsoft.Json.Linq;
using SlideCore.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SlideCore.Levels
{
	public static class LevelManager
	{
		public class LevelPackList : KeyedCollection<string, LevelPack>
		{
			protected override string GetKeyForItem(LevelPack item) => item.ID;
		}

		private static LevelPackList _levelPacks;
		public static LevelPackList LevelPacks => _levelPacks;

		public static void LoadLevelPacks()
		{
			var levelPackListContent = ContentManager.LoadContent(ContentTypes.LevelPackList, "LevelPackList");

			JArray levelPackIDsJSON;
			try
			{
				levelPackIDsJSON = JArray.Parse(levelPackListContent);
			}
			catch (Exception ex)
			{
				throw new InvalidSerializedContentException($"Exception thrown while parsing LevelPackList as valid JSON", ex);
			}

			_levelPacks = new LevelPackList();
			foreach (var packIDJSON in levelPackIDsJSON)
			{
				var packID = packIDJSON.Value<string>();
				var packContent = ContentManager.LoadContent(ContentTypes.LevelPack, packID);
				var levelPack = LevelPack.Parser.ParseLevelPack(packID, packContent);
				levelPack.LoadProgressForPack();

				_levelPacks.Add(levelPack);
			}

			// Always unlock the very first level (in case)
			_levelPacks?[0].Levels?[0].SetStatus(Level.LevelInfo.LevelInfoStatus.Unsolved);
		}

		public static LevelPack GetLevelPack(string levelPackID)
		{
			if (!_levelPacks.Contains(levelPackID))
				throw new Exception($"No LevelPack loaded with ID {levelPackID}");
			return _levelPacks[levelPackID];
		}

		public static Level GetLevel(LevelPack levelPack, string levelID)
		{
			if (!levelPack.Levels.Contains(levelID))
				throw new Exception($"No Level loaded with ID {levelID}");

			var levelInfo = levelPack.Levels[levelID];
			var serializedLevel = ContentManager.LoadContent(ContentTypes.Level, levelInfo.ID);
			var level = Level.Parser.ParseLevel(serializedLevel);
			level.SetInfo(levelInfo);
			return level;
		}

		// TODO: Write testing around the following four methods
		public static bool HasNextLevel(LevelPack levelPack, Level.LevelInfo levelInfo, ref bool isUnlocked)
		{
			if (GetNextLevel(ref levelPack, ref levelInfo))
			{
				isUnlocked = levelInfo.Status != Level.LevelInfo.LevelInfoStatus.Locked;
				return true;
			}
			return false;
		}
		public static bool HasPreviousLevel(LevelPack levelPack, Level.LevelInfo levelInfo) => GetPreviousLevel(ref levelPack, ref levelInfo);

		public static bool GetPreviousLevel(ref LevelPack levelPack, ref Level.LevelInfo levelInfo)
		{
			if (!levelPack.Levels.Contains(levelInfo)) throw new Exception($"Level [{levelInfo.ID}] is not part of Level Pack [{levelPack.ID}]");

			var levelIndex = levelInfo.Index;
			if (levelIndex - 1 >= 0 && levelIndex < levelPack.TotalLevelCount)
			{
				// Previous Level in current pack
				var prevLevel = levelPack.Levels[levelIndex - 1];

				levelInfo = prevLevel;
				return true;
			}

			if (levelIndex - 1 < 0)
			{
				// Previous Level in previous pack
				var prevPack = GetPreviousPack(levelPack);

				if (prevPack == null) return false; // No more levels

				levelPack = prevPack;
				levelInfo = prevPack.Levels[prevPack.Levels.Count - 1];
				return true;
			}

			return false;
		}

		public static bool GetNextLevel(ref LevelPack levelPack, ref Level.LevelInfo levelInfo)
		{
			if (!levelPack.Levels.Contains(levelInfo)) throw new Exception($"Level [{levelInfo.ID}] is not part of Level Pack [{levelPack.ID}]");

			var levelIndex = levelInfo.Index;
			if (levelIndex >= 0 && levelIndex + 1 < levelPack.TotalLevelCount)
			{
				// Next Level in current pack
				var nextLevel = levelPack.Levels[levelIndex + 1];

				levelInfo = nextLevel;
				return true;
			}

			if (levelIndex + 1 >= levelPack.TotalLevelCount)
			{
				// Next Level in next pack
				var nextPack = GetNextPack(levelPack);

				if (nextPack == null) return false; // No more levels

				levelPack = nextPack;
				levelInfo = nextPack.Levels[0];
				return true;
			}

			return false;
		}

		public async static Task<Level> GetDailyLevelAsync(DateTime date)
		{
			string serializedLevelJSON;
			try
			{
				var levelRequest = WebRequest.Create(GetDailyLevelURL(date));
				var response = await levelRequest.GetResponseAsync();
				var responseStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				serializedLevelJSON = await responseStream.ReadToEndAsync();
			}
			catch (Exception ex)
			{
				throw new Exception($"Unable to get daily level for {date:yyyy.MM.dd}", ex);
			}

			var level = Level.Parser.ParseLevel(serializedLevelJSON);
			level.SetInfo($"DailyLevels\\{date:yyyy.MM.dd}", "Daily Level", 0);
			return level;
		}

		private static string GetDailyLevelURL(DateTime date)
		{
			return $"https://teridiumlabs.github.io/SlideLevels/Levels/{date:yyyy.MM}/{date:yyyy.MM.dd}.json";
		}

		public static void SolveLevel(LevelPack levelPack, string levelID, bool isWithinMoves, bool hasCollectedSushi)
		{
			if (!levelPack.Levels.Contains(levelID))
				throw new Exception($"No Level loaded with ID {levelID}");

			// Get the pack's corresponding level info
			// Not using level.Info as the level's info might not be the same one as the pack
			var packLevelInfo = levelPack.Levels[levelID];

			// Mark the level as the correct level of solved
			bool isWithinMinMoves = packLevelInfo.IsSolvedWithinMinMoves || isWithinMoves;
			bool hasSushi = packLevelInfo.IsSolvedWithSushi || hasCollectedSushi;

			if (isWithinMinMoves && hasSushi) packLevelInfo.SetStatus(Level.LevelInfo.LevelInfoStatus.FullySolved);
			else if (isWithinMinMoves && !hasSushi) packLevelInfo.SetStatus(Level.LevelInfo.LevelInfoStatus.SolvedWithMinMoves);
			else if (!isWithinMinMoves && hasSushi) packLevelInfo.SetStatus(Level.LevelInfo.LevelInfoStatus.SolvedWithSushi);
			else packLevelInfo.SetStatus(Level.LevelInfo.LevelInfoStatus.Solved);

			// Unlock next level and save the pack
			UnlockNextLevel(levelPack, packLevelInfo);
			levelPack.SaveProgressForPack();
		}

		private static void UnlockNextLevel(LevelPack levelPack, Level.LevelInfo level)
		{
			var levelIndex = level.Index;
			if (levelIndex >= 0 && levelIndex + 1 < levelPack.TotalLevelCount)
			{
				// Next Level in current pack
				var nextLevel = levelPack.Levels[levelIndex + 1];
				nextLevel.SetStatus(Level.LevelInfo.LevelInfoStatus.Unsolved);
				levelPack.SaveProgressForPack();
			}
			else if (levelIndex + 1 >= levelPack.TotalLevelCount)
			{
				// Next Level in next pack
				var nextPack = GetNextPack(levelPack);
				nextPack?.UnlockFirstLevel();
			}
		}

		private static LevelPack GetPreviousPack(LevelPack currentLevelPack)
		{
			var packIndex = LevelPacks.IndexOf(currentLevelPack);
			if (packIndex - 1 < 0) return null;

			var nextPack = LevelPacks[packIndex - 1];
			return nextPack;
		}

		private static LevelPack GetNextPack(LevelPack currentLevelPack)
		{
			var packIndex = LevelPacks.IndexOf(currentLevelPack);
			if (packIndex < 0 || packIndex >= LevelPacks.Count - 1) return null;

			var nextPack = LevelPacks[packIndex + 1];
			return nextPack;
		}
	}
}
