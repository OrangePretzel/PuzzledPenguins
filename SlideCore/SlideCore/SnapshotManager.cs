using SlideCore.Levels;
using System.Collections.Generic;

namespace SlideCore
{
	/// <summary>SnapshotManager maintains a list of level snapshots and move history</summary>
	public class SnapshotManager
	{
		private List<PlayerActions> _playerActionHistory;
		private LinkedList<LevelSnapshot> _majorLevelSnapshots = new LinkedList<LevelSnapshot>();
		private LevelSnapshot _tentativeSnapshot;
		private LevelSnapshot _tentativeSnapshotComparer;
		private bool _hasTentativeSnapshot;
		private readonly int _maxMajorSnapshots = 3;

		public LevelSnapshot TentativeSnapshot => _hasTentativeSnapshot ? _tentativeSnapshot : null;

		private int _availableUndos = 0;
		/// <summary>True if there is a previous state to undo to</summary>
		public bool CanUndo => _availableUndos > 0 || _hasTentativeSnapshot;

		public SnapshotManager(int maxSnapshots = 3)
		{
			_playerActionHistory = new List<PlayerActions>();
			_maxMajorSnapshots = maxSnapshots;
			_hasTentativeSnapshot = false;
		}

		/// <summary>Initializes the correct number of snapshots, allocating memory for them in advance</summary>
		public void InitializeSnapshotsForLevel(Level level)
		{
			for (int i = 0; i < _maxMajorSnapshots; i++)
			{
				var majorSnapshot = level.InitializeSnapshot();
				_majorLevelSnapshots.AddLast(majorSnapshot);
			}

			_tentativeSnapshot = level.InitializeSnapshot();
			_tentativeSnapshotComparer = level.InitializeSnapshot();
		}

		/// <summary>Takes a snapshot of the level in it's current state</summary>
		public void TakeLevelSnapshot(Level level)
		{
			// Get the oldest snapshot in the list and update it
			var snapshot = _majorLevelSnapshots.First.Value;
			_majorLevelSnapshots.RemoveFirst();

			level.UpdateSnapshot(snapshot);

			// Move that snapshot to the end of the list
			_majorLevelSnapshots.AddLast(snapshot);

			if (++_availableUndos > _maxMajorSnapshots)
				_availableUndos = _maxMajorSnapshots;
		}

		public void TakeTentativeSnapshot(Level level)
		{
			level.UpdateSnapshot(_tentativeSnapshot);
			_hasTentativeSnapshot = true;
		}

		public void FinalizeTentativeSnapshot()
		{
			// Get the oldest snapshot in the list and remove it
			var snapshot = _majorLevelSnapshots.First.Value;
			_majorLevelSnapshots.RemoveFirst();

			// Add the tentive snapshot to the end
			_majorLevelSnapshots.AddLast(_tentativeSnapshot);

			// Reassign the tentative snapshot
			_tentativeSnapshot = snapshot;
			_hasTentativeSnapshot = false;

			if (++_availableUndos > _maxMajorSnapshots)
				_availableUndos = _maxMajorSnapshots;
		}

		public void ResetTentativeSnapshot()
		{
			_hasTentativeSnapshot = false;
		}

		public bool HasLevelChangedSinceTentative(Level level)
		{
			if (!_hasTentativeSnapshot) return false;

			// Update the comparer snapshot
			level.UpdateSnapshot(_tentativeSnapshotComparer);

			// Compare
			return !_tentativeSnapshot.TimeAgnosticEquals(_tentativeSnapshotComparer);
		}

		/// <summary>Restores the previous snapshot (if available)</summary>
		public void Undo(Level level)
		{
			if (!CanUndo) return;

			if (_hasTentativeSnapshot)
			{
				// Restore the tentative snapshot
				level.RestoreSnapshot(_tentativeSnapshot);
				_hasTentativeSnapshot = false;
				return;
			}

			// Get the last snapshot and restore it
			var snapshot = _majorLevelSnapshots.Last.Value;
			level.RestoreSnapshot(snapshot);

			// Move the last snapshot to the front of the list
			_majorLevelSnapshots.RemoveLast();
			_majorLevelSnapshots.AddFirst(snapshot);
			--_availableUndos;
		}

		/// <summary>Resets the snapshot manager</summary>
		public void Reset()
		{
			// No need to reset the actual snapshots, just the number of undos
			_availableUndos = 0;
			_hasTentativeSnapshot = false;
			_playerActionHistory.Clear(); // Reset the history as well
		}

		/// <summary>Add an action to the history</summary>
		public void AddPlayerActionHistory(PlayerActions playerAction)
		{
			_playerActionHistory.Add(playerAction);
		}

		/// <summary>Get a list of all player actions to the current point (including undo's)</summary>
		public List<PlayerActions> GetPlayerActionHistory()
		{
			return _playerActionHistory;
		}
	}
}
