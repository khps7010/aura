//--- Aura Script -----------------------------------------------------------
// Collect the Black Grizzly Bear's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlackGrizzlyBearScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71030);
		SetScrollId(70104);
		SetName(L("Collect the Black Grizzly Bear's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Grizzly Bear Fomor Scrolls]."));
		SetType(QuestType.Collect);

		AddObjective("collect", L("Collect 10 Black Grizzly Bear Fomor Scrolls"), 0, 0, 0, Collect(71030, 10));

		AddReward(Gold(33400));
	}
}
