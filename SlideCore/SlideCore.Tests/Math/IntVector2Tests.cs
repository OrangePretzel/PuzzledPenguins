using NUnit.Framework;
using SlideCore.Math;

namespace SlideCore.Tests.Math
{
	[TestFixture]
	public class IntVector2Tests
	{
		[Test]
		public void Test_IntVector2_Equality()
		{
			IntVector2 vec1 = new IntVector2(0, 0);
			IntVector2 vec2 = new IntVector2(0, 0);
			IntVector2 vec3 = new IntVector2(1, 0);
			IntVector2 vec4 = new IntVector2(0, 1);

			Assert.AreEqual(vec1, vec2);
			Assert.AreNotEqual(vec2, vec3);
			Assert.AreNotEqual(vec2, vec4);

			vec1.X = 1;

			Assert.AreEqual(vec1, vec3);
			Assert.AreNotEqual(vec1, vec2);
		}
	}
}
