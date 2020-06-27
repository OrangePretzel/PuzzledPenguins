using NUnit.Framework;
using SlideCore.Entities;

namespace SlideCore.Tests.Entities
{
	[TestFixture]
	public class PlayerEntityTests
	{
		[Test]
		public void Test_PlayerEntity_Equality()
		{
			PlayerEntity ent1 = new PlayerEntity(0, 0, 0);
			ent1.SetMotion(0, 0);
			PlayerEntity ent2 = new PlayerEntity(0, 0, 0);
			ent2.SetMotion(0, 0);
			PlayerEntity ent3 = new PlayerEntity(0, 1, 0);
			ent3.SetMotion(0, 0);
			PlayerEntity ent4 = new PlayerEntity(0, 0, 1);
			ent4.SetMotion(0, 0);
			PlayerEntity ent5 = new PlayerEntity(0, 0, 0);
			ent5.SetMotion(1, 0);
			PlayerEntity ent6 = new PlayerEntity(0, 0, 0);
			ent6.SetMotion(0, 1);

			Assert.AreEqual(ent1, ent2);
			Assert.AreNotEqual(ent1, ent3);
			Assert.AreNotEqual(ent1, ent4);
			Assert.AreNotEqual(ent1, ent5);
			Assert.AreNotEqual(ent1, ent6);

			Assert.AreEqual(ent1.InitializeSnapshot(), ent2.InitializeSnapshot());
			Assert.AreNotEqual(ent1.InitializeSnapshot(), ent3.InitializeSnapshot());
			Assert.AreNotEqual(ent1.InitializeSnapshot(), ent4.InitializeSnapshot());
			Assert.AreNotEqual(ent1.InitializeSnapshot(), ent5.InitializeSnapshot());
			Assert.AreNotEqual(ent1.InitializeSnapshot(), ent6.InitializeSnapshot());
		}

		[Test]
		public void Test_PlayerEntity_Snapshots()
		{
			// Create the entity for testing
			PlayerEntity player = new PlayerEntity(0, 2, 1);

			// Take snapshots
			PlayerEntity.PlayerEntitySnapshot snapshot1 = (PlayerEntity.PlayerEntitySnapshot)player.InitializeSnapshot();
			PlayerEntity.PlayerEntitySnapshot snapshot2 = (PlayerEntity.PlayerEntitySnapshot)player.InitializeSnapshot();

			// Validate snapshots
			Assert.AreEqual(player.Position, snapshot1.Position);
			Assert.AreEqual(player.Motion, snapshot1.Motion);
			Assert.AreEqual(player.Position, snapshot2.Position);
			Assert.AreEqual(player.Motion, snapshot2.Motion);

			// Modify entity
			player.SetPosition(4, 5);
			player.SetMotion(1, 2);

			// Update a snapshot
			player.UpdateSnapshot(snapshot2);

			// Validate snapshot
			Assert.AreEqual(player.Position, snapshot2.Position);
			Assert.AreEqual(player.Motion, snapshot2.Motion);

			// Restore original snapshot
			player.RestoreSnapshot(snapshot1);

			// Validate entity
			Assert.AreEqual(snapshot1.Position, player.Position);
			Assert.AreEqual(snapshot1.Motion, player.Motion);
		}
	}
}
