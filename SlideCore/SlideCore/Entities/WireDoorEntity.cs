using SlideCore.Levels;

namespace SlideCore.Entities
{
	public class WireDoorEntity : BinaryStatefulEntity, IWireInteractable, IUpdatingEntity
	{
		protected bool _enabled;
		protected UpdateResult _updateResult;

		protected bool _desiredState;

		public bool IsOpen => State;
		public bool IsClosed => !State;

		public void WireActivate() => _desiredState = true;
		public void WireDeactivate() => _desiredState = false;

		public WireDoorEntity(int id, int posX, int posY, bool initialState)
			: base(EntityTypes.WireDoor, id, posX, posY, initialState)
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
				bool blocked = false;
				if (!_desiredState) // Closing
				{
					foreach (var entity in level.GetEntitiesAtPosition(Position.X, Position.Y))
						if (entity.ID != ID)
						{
							blocked = true;
							break;
						}
				}

				if (!blocked)
				{
					SetState(_desiredState);
					_updateResult.Result = UpdateResult.ResultTypes.StateChanged;
				}
			}
		}

		#endregion
	}
}
