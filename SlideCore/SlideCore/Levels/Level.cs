using SlideCore.Entities;
using SlideCore.Math;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SlideCore.Levels
{
	public partial class Level
	{
		public class StaticEntitiesList : KeyedCollection<IntVector2, StaticEntity>
		{
			protected override IntVector2 GetKeyForItem(StaticEntity item) => item.Position;
		}

		public class LevelInfo
		{
			public enum LevelInfoStatus
			{
				Locked = 0,
				Unsolved = 1,
				Solved = 2,
				SolvedWithSushi = 3,
				SolvedWithMinMoves = 4,
				FullySolved = 5,
			}

			public string ID { get; private set; }
			public string DisplayName { get; private set; }
			public int Index { get; private set; }
			public LevelInfoStatus Status { get; private set; }

			public bool IsDailyLevel => ID.StartsWith("DailyLevels\\");
			public bool IsLocked => Status == LevelInfoStatus.Locked;
			public bool IsSolved => Status != LevelInfoStatus.Locked && Status != LevelInfoStatus.Unsolved;
			public bool IsSolvedWithinMinMoves => Status == LevelInfoStatus.SolvedWithMinMoves || Status == LevelInfoStatus.FullySolved;
			public bool IsSolvedWithSushi => Status == LevelInfoStatus.SolvedWithSushi || Status == LevelInfoStatus.FullySolved;

			public LevelInfo(string levelID, string displayName, int index)
			{
				ID = levelID;
				DisplayName = displayName;
				Index = index;
				Status = LevelInfoStatus.Locked;
			}

			public void SetStatus(LevelInfoStatus status, bool force = false)
			{
				if (!force && Status > status) return; // Already at a greater status
				Status = status;
			}
		}

		public static StaticEntity EDGE_WALL_ENTITY = new StaticEntity(EntityTypes.Wall, 0, -1, -1);

		public LevelInfo Info { get; private set; }

		public int LevelWidth { get; private set; }
		public int LevelHeight { get; private set; }
		public int TargetMoves { get; private set; }
		public int Move { get; private set; }
		public int Tick { get; private set; }

		public StaticEntitiesList StaticEntities { get; private set; }
		public List<DynamicEntity> DynamicEntities { get; private set; }
		public List<PlayerEntity> PlayerEntities { get; private set; }

		public LevelSnapshot CurrentLevelState { get; private set; }
		public bool CanUndo => _snapshotManager.CanUndo;
		public bool IsWithinTargetMoves => Move <= TargetMoves;

		private SnapshotManager _snapshotManager;
		private Dictionary<int, UpdateResult> _updateResults;
		private LevelSnapshot _initialLevelState;

		// Sushi List for easy sushi querying
		public bool HasCollectedAllSushi => SushiEntities.Count > 0 && SushiEntities.All(s => s.IsCollected);
		public int CollectedSushiCount => SushiEntities.Count(s => s.IsCollected);
		public int TotalSushiCount => SushiEntities.Count;
		public List<SushiEntity> SushiEntities;

		private int _nextEntityID = 1;
		public int GetNextEntityID() => _nextEntityID++;

		private Level()
		{
			PlayerEntities = new List<PlayerEntity>();
			DynamicEntities = new List<DynamicEntity>();
			StaticEntities = new StaticEntitiesList();
			SushiEntities = new List<SushiEntity>();

			_snapshotManager = new SnapshotManager();
			_updateResults = new Dictionary<int, UpdateResult>();

			Move = 0;
			Tick = 0;
		}

		public void SetInfo(LevelInfo levelInfo)
		{
			Info = levelInfo;
		}

		public void SetInfo(string levelID, string levelDisplayName, int index)
		{
			Info = new LevelInfo(levelID, levelDisplayName, index);
		}

		public void InitializeLevelForGame()
		{
			foreach (var playerEntity in PlayerEntities)
				_updateResults.Add(playerEntity.ID, playerEntity.GetUpdateResult());
			foreach (var dynamicEntity in DynamicEntities)
				_updateResults.Add(dynamicEntity.ID, dynamicEntity.GetUpdateResult());
			foreach (var staticEntity in StaticEntities)
				if (staticEntity is IUpdatingEntity)
					_updateResults.Add(staticEntity.ID, ((IUpdatingEntity)staticEntity).GetUpdateResult());

			_snapshotManager.InitializeSnapshotsForLevel(this);
			_initialLevelState = InitializeSnapshot();
		}

		public AggregateUpdateResult ResetLevel()
		{
			RestoreSnapshot(_initialLevelState);
			_snapshotManager.Reset();
			return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.UndoPerformed, 0, _updateResults);
		}

		public LevelSnapshot InitializeSnapshot()
		{
			var snapshot = new LevelSnapshot();
			snapshot.InitializeForLevel(this);
			return snapshot;
		}

		public void UpdateSnapshot(LevelSnapshot snapshot)
		{
			if (snapshot == null) throw new System.Exception("Attempting to update null LevelSnapshot. Perhaps the level wasn't initialized before use?");

			foreach (var playerEntity in PlayerEntities)
				playerEntity.UpdateSnapshot(snapshot.EntitySnapshots[playerEntity.ID]);
			foreach (var dynamicEntity in DynamicEntities)
				dynamicEntity.UpdateSnapshot(snapshot.EntitySnapshots[dynamicEntity.ID]);
			foreach (var staticEntity in StaticEntities)
				if (staticEntity is IStatefulEntity)
					((IStatefulEntity)staticEntity).UpdateSnapshot(snapshot.EntitySnapshots[staticEntity.ID]);
			snapshot.Move = Move;
			snapshot.Tick = Tick;
		}

		public void RestoreSnapshot(LevelSnapshot snapshot)
		{
			if (snapshot == null) throw new System.Exception("Attempting to update null LevelSnapshot. Perhaps the level wasn't initialized before use?");

			foreach (var updateResult in _updateResults.Values)
			{
				var entitySnapshot = snapshot.EntitySnapshots[updateResult.Entity.ID];
				((IStatefulEntity)updateResult.Entity).RestoreSnapshot(entitySnapshot);
				updateResult.Reset();
			}

			Move = snapshot.Move;
			Tick = snapshot.Tick;
		}

		public List<PlayerActions> GetPlayerActionHistory() => _snapshotManager.GetPlayerActionHistory();

		public AggregateUpdateResult DoPlayerAction(PlayerActions playerAction)
		{
			switch (playerAction)
			{
				case PlayerActions.MoveRight:
				case PlayerActions.MoveUp:
				case PlayerActions.MoveLeft:
				case PlayerActions.MoveDown:
					_snapshotManager.TakeTentativeSnapshot(this);

					bool requiresUpdate = false;
					foreach (var playerEntity in PlayerEntities)
						requiresUpdate |= playerEntity.HandlePlayerAction(this, playerAction);

					if (requiresUpdate)
					{
						_snapshotManager.AddPlayerActionHistory(playerAction);
						Move++;
						var updateResult = UpdateLevel();
						return updateResult;
					}

					return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.Cancelled, 0, _updateResults);
				case PlayerActions.Undo:
					if (!_snapshotManager.CanUndo)
						return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.Cancelled, 0, _updateResults);
					_snapshotManager.AddPlayerActionHistory(playerAction);
					Undo();
					return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.UndoPerformed, 0, _updateResults);
				case PlayerActions.None:
				default:
					return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.Cancelled, 0, _updateResults);
			}
		}

		public AggregateUpdateResult UpdateLevel()
		{
			int updateTicks = 0;
			int maxTicks = System.Math.Max(LevelWidth, LevelHeight);
			bool shouldYield, isDoneUpdating, levelComplete, hasPlayerReturnedFatal;
			do
			{
				shouldYield = false;
				isDoneUpdating = true;
				levelComplete = true;
				hasPlayerReturnedFatal = false;

				++Tick;
				if (++updateTicks > maxTicks) // Saftey check incase of infinite loop
					throw new System.Exception($"Entities did not return a significant result within the maximum ticks. Max: {maxTicks}.");

				foreach (var playerEntity in PlayerEntities)
					if (playerEntity.Enabled)
						playerEntity.Update(this, updateTicks);

				foreach (var dynamicEntity in DynamicEntities)
					if (dynamicEntity.Enabled)
						dynamicEntity.Update(this, updateTicks);

				foreach (var staticEntity in StaticEntities)
					if (staticEntity is IUpdatingEntity)
					{
						var updatableStaticEntity = (IUpdatingEntity)staticEntity;
						if (updatableStaticEntity.Enabled)
							updatableStaticEntity.Update(this, updateTicks);
					}

				foreach (var playerEntity in PlayerEntities)
				{
					var updateResult = _updateResults[playerEntity.ID];

					shouldYield |= updateResult.ShouldYieldResult;
					isDoneUpdating &= updateResult.IsDoneUpdating;
					hasPlayerReturnedFatal |= updateResult.Result == UpdateResult.ResultTypes.FatalResult;
					levelComplete &= updateResult.Result == UpdateResult.ResultTypes.ReachedFinish;
				}

				foreach (var dynamicEntity in DynamicEntities)
				{
					var updateResult = _updateResults[dynamicEntity.ID];

					shouldYield |= updateResult.ShouldYieldResult;
					isDoneUpdating &= updateResult.IsDoneUpdating;
				}

				foreach (var staticEntity in StaticEntities)
					if (staticEntity is IUpdatingEntity)
					{
						var updatableStaticEntity = (IUpdatingEntity)staticEntity;
						var updateResult = _updateResults[staticEntity.ID];

						shouldYield |= updateResult.ShouldYieldResult;
						isDoneUpdating &= updateResult.IsDoneUpdating;
					}

				// Always yield mode
				//shouldYield = true;
			}
			while (!shouldYield);

			if (hasPlayerReturnedFatal)
				return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.FatalResult, updateTicks, _updateResults);
			if (levelComplete)
			{
				FinalizeTentativeSnapshot();
				return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.LevelComplete, updateTicks, _updateResults);
			}
			if (isDoneUpdating)
			{
				FinalizeTentativeSnapshot();
				return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.DoneUpdating, updateTicks, _updateResults);
			}
			return new AggregateUpdateResult(AggregateUpdateResult.ResultTypes.RequiresUpdate, updateTicks, _updateResults);
		}

		private void FinalizeTentativeSnapshot()
		{
			var snapshot = _snapshotManager.TentativeSnapshot;
			if (snapshot == null) return;

			var tickDiff = this.Tick - snapshot.Tick;

			// Only finalize the tentative snapshot if the level has changed since the update
			if (tickDiff > 1 || _snapshotManager.HasLevelChangedSinceTentative(this))
			{
				_snapshotManager.FinalizeTentativeSnapshot();
				return;
			}

			// The update resulted in no change so undoing it
			this.Tick = snapshot.Tick;
			this.Move = snapshot.Move;
			_snapshotManager.ResetTentativeSnapshot();
		}

		private void Undo() => _snapshotManager.Undo(this);

		public bool IsInBounds(int x, int y)
		{
			return x >= 0 && x < LevelWidth && y >= 0 && y < LevelHeight;
		}

		public StaticEntity GetStaticEntityAtPosition(int x, int y)
		{
			if (!IsInBounds(x, y)) return EDGE_WALL_ENTITY;

			var position = new IntVector2(x, y);

			if (StaticEntities.Contains(position))
				return StaticEntities[position];

			return null;
		}

		public DynamicEntity GetDynamicEntityAtPosition(int x, int y)
		{
			if (!IsInBounds(x, y)) return null;

			var position = new IntVector2(x, y);

			foreach (var entity in DynamicEntities)
				if (entity.Position == position)
					return entity;
			foreach (var entity in PlayerEntities)
				if (entity.Position == position)
					return entity;

			return null;
		}

		public IEnumerable<Entity> GetEntitiesAtPosition(int x, int y)
		{
			if (!IsInBounds(x, y)) return null;

			var position = new IntVector2(x, y);
			var entities = new List<Entity>();
			if (StaticEntities.Contains(position)) entities.Add(StaticEntities[position]);
			entities.AddRange(DynamicEntities.Where(e => e.Position == position));
			entities.AddRange(PlayerEntities.Where(e => e.Position == position));
			return entities;
		}
	}
}
