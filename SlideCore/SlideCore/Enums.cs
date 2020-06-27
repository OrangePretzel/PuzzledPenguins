using SlideCore.Math;

namespace SlideCore
{
	/// <summary>Enumeration of all supported entity types</summary>
	public enum EntityTypes
	{
		// Static Stateless
		None = '.',
		Wall = 'X',
		RedirectTile = '>',
		HaltTile = 'O',
		Portal = 'P',
		FinishFlag = '#',
		Pit = 'U',
		JumpTile = 'J',

		// Static Stateful
		WireButton = 'B',
		WireDoor = 'D',
		Sushi = 'S',

		// Dynamic Stateful
		Player = '@',
		SlidingCrate = 'C',
	}

	/// <summary>Enumeration of all supported player actions</summary>
	public enum PlayerActions
	{
		/// <summary>No action performed</summary>
		None,
		/// <summary>Move rightward</summary>
		MoveRight,
		/// <summary>Move upward</summary>
		MoveUp,
		/// <summary>Move leftward</summary>
		MoveLeft,
		/// <summary>Move downward</summary>
		MoveDown,
		/// <summary>Undo the last action performed (if possible)</summary>
		Undo
	}
}
