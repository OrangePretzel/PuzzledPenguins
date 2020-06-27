using System;
using System.Diagnostics;
using SlideCore.Entities;
using SlideCore.Math;

namespace SlideCore
{
	/// <summary>Represents the result of a level update</summary>
	[DebuggerDisplay("UpdateResult [{Result}] ID: [{Entity.ID}]")]
	public class UpdateResult
	{
		/// <summary>Enumeration of all supported update result types</summary>
		public enum ResultTypes
		{
			None,
			Moved,
			Collided,
			ReachedFinish,
			Redirected,
			Teleported,
			DelayAction,
			FatalResult,
			StateChanged,

			JumpTo,
		}

		/// <summary>True if the result should be yielded to the game manager</summary>
		public bool ShouldYieldResult =>
			Result != ResultTypes.None
			&& Result != ResultTypes.Moved;
		/// <summary>True if there are no more updates for this entity</summary>
		public bool IsDoneUpdating =>
			Result == ResultTypes.None
			|| Result == ResultTypes.Collided
			|| Result == ResultTypes.ReachedFinish
			|| Result == ResultTypes.StateChanged;

		/// <summary>The entity which returned this result</summary>
		public Entity Entity;
		/// <summary>The update result type</summary>
		public ResultTypes Result;
		/// <summary>If the result is 'Collided', the direction of the collision</summary>
		public IntVector2 CollisionDir;
		/// <summary>The Entity which interacted with the Dynamic Entity (if any)</summary>
		public Entity InteractionEntity;

		public UpdateResult(Entity entity)
		{
			Entity = entity;
			Result = ResultTypes.None;
			CollisionDir = IntVector2.Zero;
			InteractionEntity = null;
		}

		public void Reset()
		{
			Result = ResultTypes.None;
			CollisionDir = IntVector2.Zero;
			InteractionEntity = null;
		}

		public void SoftReset()
		{
			Result = ResultTypes.None;
			CollisionDir = IntVector2.Zero;
			InteractionEntity = null;
		}
	}
}
