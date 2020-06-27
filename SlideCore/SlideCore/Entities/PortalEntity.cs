using SlideCore.Math;

namespace SlideCore.Entities
{
	public class PortalEntity : StaticEntity
	{
		protected IntVector2 _portalTarget;
		/// <summary>The position to teleport to</summary>
		public IntVector2 PortalTarget => _portalTarget;

		public PortalEntity(int id, int posX, int posY, IntVector2 portalTarget)
			: base(EntityTypes.Portal,id, posX, posY)
		{
			_portalTarget = portalTarget;
		}

		#region IEquatable

		public bool Equals(PortalEntity other) =>
			base.Equals(other)
			&& _portalTarget == other.PortalTarget;
		public override bool Equals(object obj) => obj is PortalEntity && Equals((PortalEntity)obj);
		public static bool operator ==(PortalEntity entity1, PortalEntity entity2) => Equals(entity1, entity2);
		public static bool operator !=(PortalEntity entity1, PortalEntity entity2) => !(entity1 == entity2);
		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode = hashCode * 31 + _portalTarget.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
