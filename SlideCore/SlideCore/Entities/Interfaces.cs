using SlideCore.Levels;

namespace SlideCore.Entities
{
	public interface IStatefulEntity
	{
		IStatefulSnapshot InitializeSnapshot();
		void UpdateSnapshot(IStatefulSnapshot snapshot);
		void RestoreSnapshot(IStatefulSnapshot snapshot);
	}

	public interface IStatefulSnapshot
	{

	}

	public interface IUpdatingEntity
	{
		int ID { get; }
		bool Enabled { get; }
		UpdateResult GetUpdateResult();
		bool HandlePlayerAction(Level level, PlayerActions playerAction);
		void Update(Level level, int updateTicks);
	}

	public interface IWireInteractable
	{
		void WireActivate();
		void WireDeactivate();
	}
}
