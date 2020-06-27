using System.Collections.Generic;

namespace SlideCore
{
	public struct AggregateUpdateResult
	{
		public enum ResultTypes
		{
			Cancelled,
			RequiresUpdate,
			DoneUpdating,
			LevelComplete,
			UndoPerformed,
			FatalResult,
		}

		public ResultTypes Result;
		public int TicksTaken;
		public Dictionary<int, UpdateResult> EntityUpdateResults;

		public AggregateUpdateResult(ResultTypes result, int ticksTaken, Dictionary<int, UpdateResult> entityUpdateResults)
		{
			Result = result;
			TicksTaken = ticksTaken;
			EntityUpdateResults = entityUpdateResults;
		}
	}
}
