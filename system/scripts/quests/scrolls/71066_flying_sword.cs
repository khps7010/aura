//--- Aura Script -----------------------------------------------------------
// Collect the Flying Sword's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class FlyingSwordScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71066);
		SetScrollId(70139);
		SetName(L("Collect the Flying Sword's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Flying Sword Fomor Scrolls]."));
		SetType(QuestType.Collect);

		AddObjective("collect", L("Collect 10 Flying Sword Fomor Scrolls"), 0, 0, 0, Collect(71066, 10));

		AddReward(Gold(3470));
	}
}
