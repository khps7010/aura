//--- Aura Script -----------------------------------------------------------
// Entrance Puzzle
//--- Description -----------------------------------------------------------
// Used as first puzzle in every dungeon because <TODO: insert reason>.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;

[PuzzleScript("entrance_puzzle")]
public class EntrancePuzzleScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
		chestPlace.ReserveDoors();

		puzzle.Set("ChestOpen", false);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		var chestPlace = puzzle.GetPlace("ChestPlace");

		lockedPlace.CloseAllDoors();
		puzzle.LockPlace(lockedPlace, "Lock");

		chestPlace.AddProp(new Chest(puzzle, "KeyChest"), DungeonPropPositionType.Random);
	}

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var chest = prop as Chest;
		if (chest != null && chest.InternalName == "KeyChest" && !puzzle.Get("ChestOpen"))
		{
			puzzle.Set("ChestOpen", true);

			var chestPlace = puzzle.GetPlace("ChestPlace");
			chestPlace.CloseAllDoors();
			chestPlace.SpawnSingleMob("SingleMob1");
		}
	}

	public override void OnMobAllocated(Puzzle puzzle, MonsterGroup group)
	{
		if (group.Name == "SingleMob1")
			group.AddKeyForLock(puzzle.GetPlace("LockedPlace"));
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		puzzle.GetPlace("ChestPlace").OpenAllDoors();
	}
}
