using SlideCore.Entities;
using SlideCore.Levels;
using System;
using System.Collections.Generic;

namespace SlideCore
{
	public class LevelSnapshot : IEquatable<LevelSnapshot>
	{
		public Dictionary<int, IStatefulSnapshot> EntitySnapshots;
		public int Move;
		public int Tick;

		public LevelSnapshot()
		{
			EntitySnapshots = new Dictionary<int, IStatefulSnapshot>();
			Move = 0;
			Tick = 0;
		}

		public void InitializeForLevel(Level level)
		{
			foreach (var playerEntity in level.PlayerEntities)
			{
				var snapshot = playerEntity.InitializeSnapshot();
				EntitySnapshots.Add(playerEntity.ID, snapshot);
			}
			foreach (var dynamicEntity in level.DynamicEntities)
			{
				var snapshot = dynamicEntity.InitializeSnapshot();
				EntitySnapshots.Add(dynamicEntity.ID, snapshot);
			}
			foreach (var staticEntity in level.StaticEntities)
				if (staticEntity is IStatefulEntity)
				{
					var snapshot = ((IStatefulEntity)staticEntity).InitializeSnapshot();
					EntitySnapshots.Add(staticEntity.ID, snapshot);
				}
			Move = level.Move;
			Tick = level.Tick;
		}

		public bool TimeAgnosticEquals(LevelSnapshot snapshot)
		{
			if (snapshot == null
				|| EntitySnapshots.Count != snapshot.EntitySnapshots.Count)
				return false;

			IStatefulSnapshot eSnapshot;
			foreach (var entitySnapshot in EntitySnapshots)
				if (!snapshot.EntitySnapshots.TryGetValue(entitySnapshot.Key, out eSnapshot) || !eSnapshot.Equals(entitySnapshot.Value))
					return false;

			return true;
		}

		#region IEquatable

		public bool Equals(LevelSnapshot other) =>
			Move == other.Move
			&& Tick == other.Tick
			&& TimeAgnosticEquals(other);
		public override bool Equals(object obj) => obj is LevelSnapshot && Equals((LevelSnapshot)obj);
		public static bool operator ==(LevelSnapshot entity1, LevelSnapshot entity2) => Equals(entity1, entity2);
		public static bool operator !=(LevelSnapshot entity1, LevelSnapshot entity2) => !(entity1 == entity2);
		public override int GetHashCode()
		{
			var hashCode = -1197554814;
			foreach (var entitySnapshot in EntitySnapshots)
				hashCode = hashCode * -1521134295 + entitySnapshot.GetHashCode();
			hashCode = hashCode * -1521134295 + Move.GetHashCode();
			hashCode = hashCode * -1521134295 + Tick.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
