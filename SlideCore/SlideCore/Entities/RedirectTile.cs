using SlideCore.Math;

namespace SlideCore.Entities
{
	public class RedirectTile : StaticEntity
	{
		protected int _redirectDir;
		/// <summary>The index direction to redirect towards</summary>
		public int RedirectDir => _redirectDir;
		/// <summary>The vector direction to redirect towards</summary>
		public IntVector2 RedirectVector => RotationHelper.GetDirectionFromRotationIndex(_redirectDir);

		public RedirectTile(int id,int posX, int posY, int redirectDir)
			: base(EntityTypes.RedirectTile, id, posX, posY)
		{
			_redirectDir = redirectDir;
		}

		#region IEquatable

		public bool Equals(RedirectTile other) =>
			base.Equals(other)
			&& _redirectDir == other.RedirectDir;
		public override bool Equals(object obj) => obj is RedirectTile && Equals((RedirectTile)obj);
		public static bool operator ==(RedirectTile entity1, RedirectTile entity2) => Equals(entity1, entity2);
		public static bool operator !=(RedirectTile entity1, RedirectTile entity2) => !(entity1 == entity2);
		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode = hashCode * 31 + _redirectDir.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
