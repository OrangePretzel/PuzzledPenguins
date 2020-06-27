using SlideCore;
using SlideCore.Levels;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SlideUnity
{
	public static class AnalyticsHelper
	{
		private const string ANALYTICS_ENDPOINT = "<SANITIZED>";

		private static async Task PostAnalytics(string endpoint, Dictionary<string, string> data)
		{
			try
			{
				using (var webRequest = UnityWebRequest.Post($"{ANALYTICS_ENDPOINT}/{endpoint}", data))
				{
					var result = webRequest.SendWebRequest();
					while (!result.isDone) await Task.Delay(1);
					if (webRequest.isNetworkError || webRequest.isHttpError)
					{
						throw new System.Exception($"Error sending analytics: {webRequest.error}");
					}
				}
			}
			catch (System.Exception ex)
			{
				// Ignore as it's just analytics failing
				Debug.LogError(ex);
			}
		}

		public static void SendCustomAnalyticsEventForLevel(string eventName, Level level)
		{
			if (level == null) return;

			//AnalyticsEvent.Custom(eventName, new Dictionary<string, object>
			//{
			//	{ "level_id", level.Info.ID },
			//	{ "moves", GetMoveHistoryForLevel(level) }
			//});
		}

		public static async Task SendLevelCompleteAnalyticsEventAsync(Level level)
		{
			var data = new Dictionary<string, string>
			{
				{ "playerID", GameManager.Settings.PlayerID.ToString() },
				{ "levelID", level.Info.ID },
				{ "solution", GetMoveHistoryForLevel(level) },
			};
			await PostAnalytics("solve", data);
		}

		public static void SendLevelQuitAnalyticsEvent(Level level)
		{
			//AnalyticsEvent.LevelComplete(level.Info.ID, new Dictionary<string, object>
			//{
			//	{ "moves", GetMoveHistoryForLevel(level) }
			//});
		}

		public static string GetMoveHistoryForLevel(Level level)
		{
			StringBuilder builder = new StringBuilder();
			var levelMoves = level.GetPlayerActionHistory();

			char? moveChar;
			foreach (var move in levelMoves)
			{
				moveChar = GetCharForPlayerAction(move);
				if (moveChar.HasValue) builder.Append(moveChar.Value);
			}

			return builder.ToString();
		}

		private static char? GetCharForPlayerAction(PlayerActions playerAction)
		{
			switch (playerAction)
			{
				case PlayerActions.MoveRight:
					return 'R';
				case PlayerActions.MoveUp:
					return 'U';
				case PlayerActions.MoveLeft:
					return 'L';
				case PlayerActions.MoveDown:
					return 'D';
				case PlayerActions.Undo:
					return 'N';
				case PlayerActions.None:
				default:
					return null;
			}
		}
	}
}