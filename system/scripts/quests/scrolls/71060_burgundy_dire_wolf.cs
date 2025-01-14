//--- Aura Script -----------------------------------------------------------
// Collect the Burgundy Dire Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BurgundyDireWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71060);
		SetScrollId(70133);
		SetName(L("Collect the Burgundy Dire Wolf's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Burgundy Dire Wolf Fomor Scrolls]."));
		SetType(QuestType.Collect);

		AddObjective("collect", L("Collect 10 Burgundy Dire Wolf Fomor Scrolls"), 0, 0, 0, Collect(71060, 10));

		AddReward(Gold(4000));
	}
}
