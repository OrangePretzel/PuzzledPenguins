using Newtonsoft.Json.Linq;
using SlideCore.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SlideCore.Levels
{
	public partial class LevelPack
	{
		public class LevelList : KeyedCollection<string, Level.LevelInfo>
		{
			protected override string GetKeyForItem(Level.LevelInfo item) => item.ID;
		}

		public string ID { get; private set; }
		public string DisplayName { get; private set; }
		public LevelList Levels { get; private set; }

		public int TotalLevelCount => Levels.Count;
		public int SolvedLevelCount => Levels.Count(l => l.IsSolved);
		public int SolvedWithSushiLevelCount => Levels.Count(l => l.IsSolvedWithSushi);
		public int SolvedWithMovesLevelCount => Levels.Count(l => l.IsSolvedWithinMinMoves);

		public bool IsLocked() => Levels.All(l => l.Status == Level.LevelInfo.LevelInfoStatus.Locked);
		public bool IsSolved() => Levels.All(l => l.IsSolved);

		private string PackProgressString => $@"Progress\{ID}";

		public LevelPack(string id, string displayName, LevelList levelInfos)
		{
			ID = id;
			DisplayName = displayName;
			Levels = levelInfos;
		}

		public void UnlockFirstLevel()
		{
			SetLevelStatus(Levels[0].ID, Level.LevelInfo.LevelInfoStatus.Unsolved);
			SaveProgressForPack();
		}

		public void SetLevelStatus(string levelID, Level.LevelInfo.LevelInfoStatus status)
		{
			if (!Levels.Contains(levelID)) throw new Exception($"No level exists with the given ID [{levelID}]");
			Levels[levelID].SetStatus(status);
			SaveProgressForPack();
		}

		public void LoadProgressForPack()
		{
			var serializedPackProgress = DataManager.LoadData(PackProgressString, "{}");
			DeserializeProgressForPack(serializedPackProgress);
		}

		public void SaveProgressForPack()
		{
			var serializedPackProgress = SerializeProgressForPack();
			DataManager.StoreData(PackProgressString, serializedPackProgress);
		}

		private void DeserializeProgressForPack(string serializedPackProgress)
		{
			try
			{
				var progressJSON = JObject.Parse(serializedPackProgress);
				foreach (var level in Levels)
					if (progressJSON.ContainsKey(level.ID))
						level.SetStatus((Level.LevelInfo.LevelInfoStatus)Enum.Parse(
							typeof(Level.LevelInfo.LevelInfoStatus),
							progressJSON.Value<string>(level.ID)),
							true);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to apply progress", ex);
			}
		}

		private string SerializeProgressForPack()
		{
			try
			{
				var serializedPackProgress = new JObject();
				foreach (var level in Levels)
					serializedPackProgress.Add(level.ID, level.Status.ToString());
				return serializedPackProgress.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to save progress", ex);
			}
		}
	}
}
