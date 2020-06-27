using System.Collections.Generic;
using System.Linq;

namespace SlideCore.Levels
{
	public partial class Level
	{
		public static class Solver
		{
			public enum SolveMode
			{
				ShortestSolution,
				AllSolutions,
			}

			public class LevelSolution
			{
				public List<PlayerActions> MoveList;

				public LevelSolution(List<PlayerActions> moveList)
				{
					MoveList = moveList;
				}

				public override string ToString()
				{
					string moves = "";
					foreach (var move in MoveList)
						switch (move)
						{
							case PlayerActions.MoveRight:
								moves += "R";
								break;
							case PlayerActions.MoveUp:
								moves += "U";
								break;
							case PlayerActions.MoveLeft:
								moves += "L";
								break;
							case PlayerActions.MoveDown:
								moves += "D";
								break;
							case PlayerActions.Undo:
							case PlayerActions.None:
							default:
								break;
						}

					return $"Level Solution {{ Moves: ({MoveList.Count}) [{System.Text.RegularExpressions.Regex.Replace(moves, ".{4}", "$0_")}] }}";
				}
			}

			private class MoveResult
			{
				public LevelSnapshot ResultSnapshot;
				public AggregateUpdateResult AggregateUpdateResult;
			}

			private class ResultSoFar
			{
				public LevelSnapshot CurrentLevelSnapshot;
				public HashSet<LevelSnapshot> VisitedStates;
				public List<PlayerActions> MoveList;

				public ResultSoFar(LevelSnapshot currentLevelSnapshot, HashSet<LevelSnapshot> visitedStates, List<PlayerActions> moveList)
				{
					CurrentLevelSnapshot = currentLevelSnapshot;
					VisitedStates = visitedStates;
					MoveList = moveList;
				}

				public ResultSoFar(LevelSnapshot initialLevelSnapshot)
				{
					CurrentLevelSnapshot = initialLevelSnapshot;
					VisitedStates = new HashSet<LevelSnapshot>();
					MoveList = new List<PlayerActions>();
				}

				public override string ToString()
				{
					string moves = "";
					foreach (var move in MoveList)
						switch (move)
						{
							case PlayerActions.MoveRight:
								moves += "R";
								break;
							case PlayerActions.MoveUp:
								moves += "U";
								break;
							case PlayerActions.MoveLeft:
								moves += "L";
								break;
							case PlayerActions.MoveDown:
								moves += "D";
								break;
							case PlayerActions.Undo:
							case PlayerActions.None:
							default:
								break;
						}

					return $"Result So Far {{ Moves: ({MoveList.Count}) [{moves}] }}";
				}
			}

			public static bool IsSolvable(Level level, SolveMode solveMode = SolveMode.ShortestSolution)
			{
				return SolveLevel(level).Count > 0;
			}

			public static List<LevelSolution> SolveLevel(Level level, SolveMode solveMode = SolveMode.ShortestSolution)
			{
				// TODO: This function is probably very inefficient. Investigate performance improvements
				// Note: The method seems to perform fast (enough) but need to investigate the memory usage

				var solutions = new List<LevelSolution>();
				if (solveMode == SolveMode.AllSolutions) solutions.AddRange(GetAllUniqueSolutions(level));
				else solutions.Add(GetShortestSolution(level));
				return solutions;
			}

			private static LevelSolution GetShortestSolution(Level level)
			{
				var initialSnapshot = level.InitializeSnapshot();

				var frontier = new Queue<ResultSoFar>();
				frontier.Enqueue(new ResultSoFar(initialSnapshot));

				var visitedStates = new HashSet<LevelSnapshot>();

				PlayerActions[] ACTIONS_TO_TRY = new PlayerActions[] {
					PlayerActions.MoveRight,
					PlayerActions.MoveUp,
					PlayerActions.MoveLeft,
					PlayerActions.MoveDown
				};

				int ticks = 0;
				const int MAX_TICKS = 100000000;
				while (frontier.Count > 0)
				{
					// To prevent infinite loop (fail safe)
					if (++ticks > MAX_TICKS) throw new System.Exception($"Could not solve level in {MAX_TICKS}");

					var resultToContinueFrom = frontier.Dequeue();
					if (visitedStates.Any(s => s.TimeAgnosticEquals(resultToContinueFrom.CurrentLevelSnapshot)))
						continue;

					for (int actionIndex = 0; actionIndex < ACTIONS_TO_TRY.Length; actionIndex++)
					{
						level.RestoreSnapshot(resultToContinueFrom.CurrentLevelSnapshot);
						var action = ACTIONS_TO_TRY[actionIndex];

						var moves = new List<PlayerActions>(resultToContinueFrom.MoveList) { action };
						var nextResultSoFar = new ResultSoFar(resultToContinueFrom.CurrentLevelSnapshot, new HashSet<LevelSnapshot>(), moves);

						var moveResult = GetNextMoveBranch(action, level);
						if (moveResult != null)
						{
							visitedStates.Add(resultToContinueFrom.CurrentLevelSnapshot);
							nextResultSoFar.CurrentLevelSnapshot = moveResult.ResultSnapshot;
							if (moveResult.AggregateUpdateResult.Result == AggregateUpdateResult.ResultTypes.LevelComplete && level.HasCollectedAllSushi)
								return new LevelSolution(nextResultSoFar.MoveList);
							else
								frontier.Enqueue(nextResultSoFar);
						}
					}
				}

				return null;
			}

			private static List<LevelSolution> GetAllUniqueSolutions(Level level)
			{
				var initialSnapshot = level.InitializeSnapshot();

				var frontier = new Queue<ResultSoFar>();
				frontier.Enqueue(new ResultSoFar(initialSnapshot));

				var uniqueSolutions = new List<LevelSolution>();

				PlayerActions[] ACTIONS_TO_TRY = new PlayerActions[] {
					PlayerActions.MoveRight,
					PlayerActions.MoveUp,
					PlayerActions.MoveLeft,
					PlayerActions.MoveDown
				};

				int ticks = 0;
				const int MAX_TICKS = 100000000;
				while (frontier.Count > 0)
				{
					// To prevent infinite loop (fail safe)
					if (++ticks > MAX_TICKS) throw new System.Exception($"Could not solve level in {MAX_TICKS}");

					var resultToContinueFrom = frontier.Dequeue();
					if (resultToContinueFrom.VisitedStates.Any(s => s.TimeAgnosticEquals(resultToContinueFrom.CurrentLevelSnapshot)))
						continue;

					for (int actionIndex = 0; actionIndex < ACTIONS_TO_TRY.Length; actionIndex++)
					{
						level.RestoreSnapshot(resultToContinueFrom.CurrentLevelSnapshot);
						var action = ACTIONS_TO_TRY[actionIndex];

						var moves = new List<PlayerActions>(resultToContinueFrom.MoveList) { action };
						var nextResultSoFar = new ResultSoFar(resultToContinueFrom.CurrentLevelSnapshot, new HashSet<LevelSnapshot>(resultToContinueFrom.VisitedStates), moves);

						var moveResult = GetNextMoveBranch(action, level);
						if (moveResult != null)
						{
							nextResultSoFar.VisitedStates.Add(resultToContinueFrom.CurrentLevelSnapshot);
							nextResultSoFar.CurrentLevelSnapshot = moveResult.ResultSnapshot;
							if (moveResult.AggregateUpdateResult.Result == AggregateUpdateResult.ResultTypes.LevelComplete)
								uniqueSolutions.Add(new LevelSolution(nextResultSoFar.MoveList));
							else
								frontier.Enqueue(nextResultSoFar);
						}
					}
				}

				return uniqueSolutions;
			}

			private static MoveResult GetNextMoveBranch(PlayerActions action, Level level)
			{
				var updateSnapshots = new HashSet<LevelSnapshot>();

				AggregateUpdateResult result = level.DoPlayerAction(action);
				while (result.Result == AggregateUpdateResult.ResultTypes.RequiresUpdate)
				{
					result = level.UpdateLevel();
					var currentSnapshot = level.InitializeSnapshot();
					if (updateSnapshots.Any(s => s.TimeAgnosticEquals(currentSnapshot))) return null; // Infinite loop
					updateSnapshots.Add(currentSnapshot);
				}

				LevelSnapshot levelSnapshot = level.InitializeSnapshot();

				return new MoveResult()
				{
					AggregateUpdateResult = result,
					ResultSnapshot = levelSnapshot
				};
			}
		}
	}
}
