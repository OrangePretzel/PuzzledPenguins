using NUnit.Framework;
using SlideCore.Levels;
using System;
using System.Threading.Tasks;

namespace SlideCore.Tests.Integration
{
	[TestFixture]
	public class DailyLevelTests
	{
		[Test]
		public async Task Test_LevelManager_GetDailLevel_2018_05_03()
		{
			// 2018.05.03 is the first daily level ever created
			var level = await LevelManager.GetDailyLevelAsync(new DateTime(2018, 05, 03));

			Assert.IsTrue(level.Info.IsDailyLevel);
		}
	}
}
