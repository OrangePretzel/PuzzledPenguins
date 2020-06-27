using SlideCore.Levels;

namespace SlideCore.Entities
{
	public class SushiEntity : BinaryStatefulEntity, IUpdatingEntity
	{
		protected bool _enabled;
		protected UpdateResult _updateResult;

		protected bool _desiredState;

		public bool IsCollected => !State;
		public void Collect() => _desiredState = false;

		public SushiEntity(int id, int posX, int posY, bool initialState)
			: base(EntityTypes.Sushi, id, posX, posY, initialState)
		{
			_enabled = true;
			_updateResult = new UpdateResult(this);

			_desiredState = initialState;
		}

		#region IStatefulEntity

		public override void RestoreSnapshot(IStatefulSnapshot snapshot)
		{
			base.RestoreSnapshot(snapshot);
			_desiredState = State;
		}

		#endregion

		#region IUpdatingEntity

		public bool Enabled => _enabled;
		public UpdateResult GetUpdateResult() => _updateResult;

		public virtual bool HandlePlayerAction(Level level, PlayerActions playerAction) => false;

		public virtual void Update(Level level, int updateTicks)
		{
			// Reset
			_updateResult.SoftReset();

			if (_desiredState != State)
			{
				SetState(_desiredState);
				_updateResult.Result = UpdateResult.ResultTypes.StateChanged;
			}
		}

		#endregion
	}
}
