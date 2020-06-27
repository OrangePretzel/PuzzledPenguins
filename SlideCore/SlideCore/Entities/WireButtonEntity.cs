using SlideCore.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlideCore.Entities
{
	public struct WireInteraction
	{
		public IWireInteractable Interactable;
		public bool Invert;
	}

	public class WireButtonEntity : BinaryStatefulEntity, IUpdatingEntity
	{
		public enum ButtonModes
		{
			PressOnly,
			Toggle,
			Hold,
		}

		protected bool _enabled;
		protected UpdateResult _updateResult;

		protected bool _oldState;

		public bool IsPressed => State;

		public readonly ButtonModes ButtonMode;
		public List<WireInteraction> WireInteractions;

		public WireButtonEntity(int id, int posX, int posY, ButtonModes buttonMode, bool initialState = false)
			: base(EntityTypes.WireButton, id, posX, posY, initialState)
		{
			_enabled = true;
			_updateResult = new UpdateResult(this);

			_oldState = false;
			ButtonMode = buttonMode;
			WireInteractions = new List<WireInteraction>();
		}

		public void Interact()
		{
			// Currently active
			if (State)
			{
				if (ButtonMode == ButtonModes.Toggle)
					Interact(false);
				return;
			}

			// Not active
			Interact(true);
		}

		protected void Interact(bool isActive)
		{
			SetState(isActive);
			foreach (var wireInt in WireInteractions)
			{
				if (wireInt.Invert ? !isActive : isActive)
					wireInt.Interactable.WireActivate();
				else
					wireInt.Interactable.WireDeactivate();
			}
		}

		public override void RestoreSnapshot(IStatefulSnapshot snapshot)
		{
			base.RestoreSnapshot(snapshot);
			_oldState = State;
		}

		#region IUpdatingEntity

		public bool Enabled => _enabled;
		public UpdateResult GetUpdateResult() => _updateResult;

		public virtual bool HandlePlayerAction(Level level, PlayerActions playerAction) => false;

		public virtual void Update(Level level, int updateTicks)
		{
			// Reset
			_updateResult.SoftReset();

			switch (ButtonMode)
			{
				case ButtonModes.PressOnly:
				case ButtonModes.Toggle:
					break;
				case ButtonModes.Hold:
					if (State)
					{
						var dynamicEntitiesCount = level
							.GetEntitiesAtPosition(_position.X, _position.Y)
							.Count(de => de != this);
						if (dynamicEntitiesCount < 1)
							Interact(false);
					}
					break;
				default:
					throw new Exception($"Unimplemented WireButton update mode {ButtonMode}");
			}

			if (_oldState != State)
			{
				_oldState = State;
				_updateResult.Result = UpdateResult.ResultTypes.StateChanged;
			}
		}

		#endregion
	}
}
