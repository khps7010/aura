//--- Aura Script -----------------------------------------------------------
// Collect the Gold Goblin's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class GoldGoblinScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71039);
		SetScrollId(70113);
		SetName(L("Collect the Gold Goblin's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gold Goblin Fomor Scrolls]."));
		SetType(QuestType.Collect);

		AddObjective("collect", L("Collect 10 Gold Goblin Fomor Scrolls"), 0, 0, 0, Collect(71039, 10));

		AddReward(Gold(3500));
	}
}
