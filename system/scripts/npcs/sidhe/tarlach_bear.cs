//--- Aura Script -----------------------------------------------------------
// Tarlach Bear
//--- Description -----------------------------------------------------------
// Bear form
//--- History ---------------------------------------------------------------
// 1.0 Added general keyword responses and day/night cycle
//---------------------------------------------------------------------------

public class TarlachBearScript : NpcScript
{
	public override void Load()
	{
		SetRace(4);
		SetName("_tarlachbear");
		SetFace(skinColor: 17, eyeType: 4, eyeColor: 43, mouthType: 3);
		SetColor(0x00553A26, 0x0000FF00, 0x000000FF);
		if (!ErinnTime.Now.IsNight)
			SetLocation(48, 11100, 30400, 167);
		else
			SetLocation(22, 5800, 7100, 167);

		AddGreeting(0, ".....");

		AddPhrase("...... ");
		AddPhrase("Grrrrr...");
		AddPhrase("Growl... ");
		AddPhrase("Rooooar... ");
		AddPhrase("Roar... ");
		AddPhrase("Rooar... ");
	}

	[On("ErinnDaytimeTick")]
	public void OnErinnDaytimeTick(ErinnTime time)
	{
		if (!time.IsNight)
			NPC.Warp(48, 11100, 30400);
		else
			NPC.Warp(22, 5800, 7100);
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Tarlach.mp3");

		await Intro(
			"The bear is enormous, and gazes at you with bright eyes.",
			"It sniffs the air and looks around as if searching for something.",
			"Its breath comes in steamy puffs, and it claws the ground from time to time."
		);

		Msg("Grr...", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				await Conversation();
				break;
		}

		End("You have ended your conversation with <npcname/>.");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			default:
				RndFavorMsg(
					".....<br/>(There's no way a bear could understand me...)",
					".....<br/>(I wonder if the bear wants a Mana Herb?)"
				);
				break;
		}
	}
}