using Newtonsoft.Json.Linq;
using System;

namespace SlideCore.Levels
{
	public partial class LevelPack
	{
		public static class Parser
		{
			/// <summary>Exception thrown when the string was not able to parsed as a level pack</summary>
			[Serializable]
			public class InvalidSerializedLevelPackException : Exception
			{
				public InvalidSerializedLevelPackException() { }

				public InvalidSerializedLevelPackException(string message) : base(message) { }

				public InvalidSerializedLevelPackException(string message, Exception innerException) : base(message, innerException) { }

				protected InvalidSerializedLevelPackException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
			}

			public static LevelPack ParseLevelPack(string packID, string serializedLevelPack)
			{
				if (string.IsNullOrWhiteSpace(packID)) throw new Exception($"Pack ID was not valid [{packID}]");

				try
				{
					// Parse the string as JSON
					JObject levelJSON = JObject.Parse(serializedLevelPack);
					if (levelJSON == null) throw new InvalidSerializedLevelPackException("Level was not valid JSON");

					var packName = levelJSON.Value<string>("PackName");
					if (string.IsNullOrWhiteSpace(packName)) throw new InvalidSerializedLevelPackException($"Pack name was not valid [{packName}]");

					var levelInfosJSON = (JArray)levelJSON["Levels"];
					if (levelInfosJSON == null || levelInfosJSON.Count < 1) throw new InvalidSerializedLevelPackException("Level pack did not contain any levels");

					var levelInfos = new LevelList();
					foreach (var levelInfoJSON in levelInfosJSON)
					{
						var levelInfo = ParseLevelInfo(levelInfoJSON, levelInfos.Count);
						levelInfos.Add(levelInfo);
					}

					var levelPack = new LevelPack(packID, packName, levelInfos);
					return levelPack;
				}
				catch (Exception ex)
				{
					throw new InvalidSerializedLevelPackException("Unable to parse level", ex);
				}
			}

			private static Level.LevelInfo ParseLevelInfo(JToken levelInfoJSON, int index)
			{
				var levelID = levelInfoJSON.Value<string>("ID");
				if (string.IsNullOrWhiteSpace(levelID)) throw new InvalidSerializedLevelPackException($"Level ID was not valid [{levelID}]");

				var displayName = levelInfoJSON.Value<string>("DisplayName");
				if (string.IsNullOrWhiteSpace(displayName)) throw new InvalidSerializedLevelPackException($"Level display name was not valid [{displayName}]");

				var levelInfo = new Level.LevelInfo(levelID, displayName, index);
				return levelInfo;
			}
		}
	}
}
