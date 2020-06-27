using System;

namespace SlideCore.Entities
{
	public class BinaryStatefulEntity : StaticEntity, IEquatable<BinaryStatefulEntity>, IStatefulEntity
	{
		public class BinaryStateEntityEntitySnapshot : IEquatable<BinaryStateEntityEntitySnapshot>, IStatefulSnapshot
		{
			public bool State;

			#region IEquatable

			public bool Equals(BinaryStateEntityEntitySnapshot other) =>
				other != null
				&& State == other.State;
			public override bool Equals(object obj) => obj is BinaryStateEntityEntitySnapshot && Equals((BinaryStateEntityEntitySnapshot)obj);
			public static bool operator ==(BinaryStateEntityEntitySnapshot snapshot1, BinaryStateEntityEntitySnapshot snapshot2) => Equals(snapshot1, snapshot2);
			public static bool operator !=(BinaryStateEntityEntitySnapshot snapshot1, BinaryStateEntityEntitySnapshot snapshot2) => !(snapshot1 == snapshot2);
			public override int GetHashCode()
			{
				var hashCode = base.GetHashCode();
				hashCode = hashCode * 31 + State.GetHashCode();
				return hashCode;
			}

			#endregion
		}

		private bool _state;
		public bool State => _state;
		public void SetState(bool newState) => _state = newState;

		public BinaryStatefulEntity(EntityTypes entityType, int id, int posX, int posY, bool initialState)
			: base(entityType, id, posX, posY)
		{
			_state = initialState;
		}

		#region IStatefulEntity

		public virtual IStatefulSnapshot InitializeSnapshot()
		{
			var snapshot = new BinaryStateEntityEntitySnapshot();
			UpdateSnapshot(snapshot);
			return snapshot;
		}

		public virtual void UpdateSnapshot(IStatefulSnapshot snapshot)
		{
			var binarySnapshot = (BinaryStateEntityEntitySnapshot)snapshot;
			binarySnapshot.State = _state;
		}

		public virtual void RestoreSnapshot(IStatefulSnapshot snapshot)
		{
			var binarySnapshot = (BinaryStateEntityEntitySnapshot)snapshot;
			_state = binarySnapshot.State;
		}

		#endregion

		#region IEquatable

		public bool Equals(BinaryStatefulEntity other) => base.Equals(other);
		public override bool Equals(object obj) => obj is BinaryStatefulEntity && Equals((BinaryStatefulEntity)obj);
		public static bool operator ==(BinaryStatefulEntity entity1, BinaryStatefulEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(BinaryStatefulEntity entity1, BinaryStatefulEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode()
		{
			var hashCode = base.GetHashCode();
			hashCode = hashCode * 31 + State.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
