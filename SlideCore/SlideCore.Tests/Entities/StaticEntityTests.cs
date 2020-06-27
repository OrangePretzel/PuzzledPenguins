using NUnit.Framework;
using SlideCore.Entities;

namespace SlideCore.Tests.Entities
{
	[TestFixture]
	public class StaticEntityTests
	{
		[Test]
		public void Test_StaticEntity_Equality()
		{
			StaticEntity ent1 = new StaticEntity(EntityTypes.Wall, 0, 0, 0);
			StaticEntity ent2 = new StaticEntity(EntityTypes.Wall, 0, 0, 0);
			StaticEntity ent3 = new StaticEntity(EntityTypes.FinishFlag, 0, 0, 0);
			StaticEntity ent4 = new StaticEntity(EntityTypes.Wall, 0, 1, 0);
			StaticEntity ent5 = new StaticEntity(EntityTypes.Wall, 0, 0, 1);
			StaticEntity ent6 = new StaticEntity(EntityTypes.Wall, 1, 0, 0);

			Assert.AreEqual(ent1, ent2);
			Assert.AreNotEqual(ent1, ent3);
			Assert.AreNotEqual(ent1, ent4);
			Assert.AreNotEqual(ent1, ent5);
			Assert.AreNotEqual(ent1, ent6);
		}
	}
}
