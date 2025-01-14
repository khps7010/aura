﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	/// <summary>
	/// Flags for SkillInfo
	/// </summary>
	[Flags]
	public enum SkillFlags : ushort
	{
		Shown = 0x01,
		CountType = 0x02,
		InUse = 0x04,
		Rankable = 0x08,
		PassiveApplied = 0x10,

		ShowCondition1 = 0x80,
		ShowCondition2 = 0x100,
		ShowCondition3 = 0x200,
		ShowCondition4 = 0x400,
		ShowCondition5 = 0x800,
		ShowCondition6 = 0x1000,
		ShowCondition7 = 0x2000,
		ShowCondition8 = 0x4000,
		ShowCondition9 = 0x8000,

		/// <summary>
		/// ShowCondition1-9
		/// </summary>
		ShowAllConditions = 0xFF80,
	}

	/// <summary>
	/// Skill ranks.
	/// </summary>
	/// <remarks>
	/// The client calculates the Dan based on the rank id.
	/// 19 = Dan4, 30 = Dan15, etc.
	/// </remarks>
	public enum SkillRank : byte
	{
		Novice = 0, RF = 1, RE = 2, RD = 3, RC = 4, RB = 5, RA = 6, R9 = 7,
		R8 = 8, R7 = 9, R6 = 10, R5 = 11, R4 = 12, R3 = 13, R2 = 14, R1 = 15,
		Dan1 = 16, Dan2 = 17, Dan3 = 18
	}

	/// <summary>
	/// Skill ids
	/// </summary>
	public enum SkillId : ushort
	{
		None = 0,

		// Life
		Tailoring = 10001,
		ReadInDetail = 10002,
		Rest = 10004,
		TownCry = 10007,
		Campfire = 10008,
		FirstAid = 10009,
		Gathering = 10010,
		Weaving = 10011,
		Milling = 10012,
		Handicraft = 10013,
		Weaving2 = 10014,
		Refining = 10015,
		Blacksmithing = 10016,
		Cooking = 10020,
		Herbalism = 10021,
		PotionMaking = 10022,
		Fishing = 10023,
		ProductionMastery = 10024,
		OpenTreasureChest = 10025,
		TamingWildAnimals = 10026,
		ControlPaddle = 10027,
		Metallurgy = 10028,
		HotAirBalloonControl = 10029,
		Fragmentation = 10030,
		Synthesis = 10031,
		ExclusiveStreetArtistskill = 10032,
		Carpentry = 10033,
		Gardening = 10034,
		InstallKiosk = 10035,
		MakeTheatreMissionPass = 10036,
		WineMaking = 10037,
		//향수제조 =  10038, 
		StudyPotionLore = 10039,
		HillwenEngineering = 10040,
		MagicCraft = 10041,
		RareMineralogy = 10042,
		ShyllienEcology = 10043,
		Tasting = 10044,
		IngredientHunting = 10045,
		Catering = 10046,

		// Combat
		Defense = 20001,
		Smash = 20002,
		Counterattack = 20003,
		NaturalShield = 20004,
		HeavyStander = 20005,
		ManaDeflector = 20006,
		FullSwing = 20007,
		FinalSmash = 20008,
		FuryOfBard = 20009,
		Jump = 20010,
		Charge = 20011,
		AssaultSlash = 20012,
		Berserker = 20013,
		BerserkerHidden = 20014,
		AngerManagement = 20015,
		LanceCounter = 20016,
		LanceCharge = 20017,
		Bash = 20019,
		RangedAttack = 21001,
		MagnumShot = 21002,
		ArrowRevolver = 21003,
		ArrowRevolver2 = 21004,
		Chaingun = 21005,
		SupportShot = 21006,
		MirageMissile = 21007,
		BombIgnition = 21008,
		SpiritBowAwakening = 21009,
		ThrowingAttack = 21010,
		HotAirBalloonCrossbowShot = 21011,
		SpiderShot = 21012,
		UrgentShot = 21014,
		Windmill = 22001,
		Stomp = 22002,
		SelfDestructionActive = 22003,
		FinalHit = 22004,
		SpiritSwordAwakening = 22005,
		SpiritBluntAwakening = 22006,
		GiantStomp = 22007,
		WendigoStomp = 22008,
		AlligatorFullSwing = 22009,
		GiantLionRoar = 22010,
		CrashShot = 22011,
		InstinctiveReaction = 23001,
		CombatMastery = 23002,
		CriticalHit = 23003,
		NaturalShieldPassive = 23004,
		HeavyStanderPassive = 23005,
		ManaDeflectorPassive = 23006,
		DarkLord = 23007,
		GlasGhaibhleannSkill = 23008,
		ChainCasting = 23009,
		SelfDestruction = 23010,
		SharpMind = 23011,
		SwordMastery = 23012,
		AxeMastery = 23013,
		BluntMastery = 23014,
		DaggerMastery = 23015,
		ShieldMastery = 23016,
		DualWeaponMastery = 23017,
		DragonTailAttack = 23021,
		StoneBreath = 23022,
		DragonFireBreath = 23023,
		DragonDashAttack = 23024,
		SandwormMeleeAttack = 23025,
		LionBite = 23026,
		LionSwing = 23027,
		WindGuard = 23028,
		Taunt = 23029,
		FinalShot = 23030,
		ShadowBreath = 23031,
		DarkFlame = 23032,
		LightOfSword = 23033,
		ClaimhSolasDash = 23034,
		ClaimhSolasFullSwing = 23035,
		ScarecrowCurse = 23036,
		GriffinRoar = 23037,
		WallDecorationFireBolt = 23038,
		WallDecorationIceBolt = 23039,
		WallDecorationLightningBolt = 23040,
		WallDecorationBoltAttack = 23041,
		WallDecorationPoisonGasAttack = 23042,
		Evasion = 23043,
		PythonStoneLongRangeAttack = 23044,
		PythonStoneShortRangeAttack = 23045,
		PythonStoneBreath = 23046,
		NuadhaStomp = 23047,
		NuadhaSpearOfLight = 23048,
		NuadhaFuryOfLight = 23049,
		NuadhaLightOfSword = 23050,
		NuadhaSmash = 23051,
		TigerRoar = 23052,
		GrimReaperVerticalAttack = 23053,
		GrimReaperHorizontalAttack = 23054,
		GrimReaperWindmill = 23055,
		ArmorBearRoar = 23057,
		ShadowBunshin = 23058,
		BowMastery = 23060,
		CrossbowMastery = 23061,
		LanceMastery = 23062,
		KnuckleMastery = 23063,
		BranShockwave = 23101,
		BranCharge = 23102,
		BranArmSwing = 23103,
		Loot = 23104,
		Escape = 23105,
		GoldStrike = 23106,
		Berserk = 23107,
		ChainMastery = 24001,
		ChargingStrike = 24101,
		FocusedFist = 24102,
		ChainCounterPunch = 24103,
		SpinningUppercut = 24201,
		SomersaultKick = 24202,
		DropKick = 24301,
		Pummel = 24302,
		Respite = 25000,
		Tumble = 25001,

		// Magic
		Meditation = 30003,
		Enchant = 30005,
		Healing = 30006,
		MagicMastery = 30007,
		PartyHealing = 30008,
		PetMeditation = 30009,
		LifeDrainMagic = 30010,
		MirrorWitchRest = 30011,
		MonsterResurrection = 30012,
		YarnBinding = 30013,
		SilentMove = 30014,
		FireMastery = 30016,
		IceMastery = 30017,
		LightningMastery = 30018,
		BoltMastery = 30019,
		MagicWandMastery = 30020,
		MagicStaffMastery = 30021,
		Lightningbolt = 30101,
		Thunder = 30102,
		FloatingStoneThunder = 30103,
		InstantThunder = 30104,
		Shockwave = 30105,
		LightningRod = 30106,
		Firebolt = 30201,
		Fireball = 30202,
		TrainingFireball = 30203,
		FloatingStoneFireBall = 30204,
		Icebolt = 30301,
		IceSpear = 30302,
		IceHug = 30303,
		SuperIcebolt = 30304,
		WaterSpray = 30305,
		FloatingStoneIceSpear = 30306,
		HailStorm = 30307,
		SpiritWandAwakening = 30401,
		IceLightning = 30450,
		IceFire = 30451,
		FireLightning = 30452,
		FusionBolt = 30453,
		FireMagicShield = 30460,
		IceMagicShield = 30461,
		LightningMagicShield = 30462,
		NaturalMagicShield = 30463,
		ManaShield = 30464,
		Blaze = 30470,
		PaperAirplaneBomb = 30471,
		InvitationofDeath = 30472,
		FlamesOfHell = 30473,
		Spellwalk = 30480,
		SnapCast = 30481,
		Inspiration = 30482,

		// Alchemy
		ManaCrystallization = 35001,
		LifeDrain = 35002,
		AlchemyMastery = 35003,
		WaterCannon = 35004,
		Crystallization = 35005,
		BarrierSpikes = 35006,
		WindBlast = 35007,
		FlameBurst = 35008,
		SandBurst = 35009,
		RainCasting = 35010,
		FrozenBlast = 35011,
		MetalConversion = 35012,
		Shock = 35013,
		HeatBuster = 35014,
		ChainCylinder = 35015,
		FireAlchemy = 35016,
		WaterAlchemy = 35017,
		ClayAlchemy = 35018,
		WindAlchemy = 35019,
		AlchemyCylinderMastery = 35020,
		Transmutation = 35021,
		SpiritCylinderAwakening = 35101,

		// Trans
		SpiritOfOrder = 40001,
		PowerOfOrder = 40002,
		EyeOfOrder = 40003,
		SwordOfOrder = 40004,
		PaladinNaturalShield = 40011,
		PaladinHeavyStander = 40012,
		PaladinManaDeflector = 40013,
		SoulOfChaos = 41001,
		ControlofDarkness = 41002,
		BodyOfChaos = 41011,
		MindOfChaos = 41012,
		HandsOfChaos = 41013,
		DarkNaturalShield = 41021,
		DarkHeavyStander = 41022,
		DarkManaDeflector = 41023,
		RaceTransformation = 42001,
		WindMillTrans = 42002,
		FuryOfConnous = 43001,
		ElvenMagicMissile = 43002,
		ArmorOfConnous = 43011,
		MindOfConnous = 43012,
		SharpnessOfConnous = 43013,
		ConnousNaturalShield = 43021,
		ConnousHeavyStander = 43022,
		ConnousManaDeflector = 43023,
		DemonOfPhysis = 44001,
		GiantFullSwing = 44002,
		ShieldOfPhysis = 44011,
		LifeOfPhysis = 44012,
		SpellOfPhysis = 44013,
		PhysisNaturalShield = 44021,
		PhysisHeavyStander = 44022,
		PhysisManaDeflector = 44023,
		AwakeningofLight = 45001,
		SpearOfLight = 45002,
		FuryOfLight = 45003,
		AwakeningofLightDisposable = 45004,
		SpearOfLightDisposable = 45005,
		FuryOfLightDisposable = 45006,
		ShadowSpirit = 45007,
		WingsOfEclipse = 45008,
		WingsOfRage = 45009,

		// Hidden
		HiddenEnchant = 50001,
		HiddenResurrection = 50002,
		HiddenTownBack = 50003,
		HiddenGuildStoneSetting = 50004,
		WebSpinning = 50005,
		HiddenBlessing = 50006,
		CampfireKit = 50007,
		SkillUntrainKit = 50008,
		BigBlessingWaterKit = 50009,
		Dye = 50010,
		EnchantElementalAllSlot = 50011,
		HiddenPoison = 50012,

		// Actions
		Sketch = 50013,
		Exploration = 50014,
		HiddenBomb = 50015,
		Playdead = 50016,
		Hide = 50017,
		Performance = 50018,
		LandMaker = 50019,
		RockThrowing = 50020,
		DiceTossing = 50021,
		JamSession = 50022,
		ThrowPaperAirplane = 50023,
		MakeChocoStatue = 50024,
		FossilRestoration = 50025,
		SeesawJump = 50026,
		SeesawCreate = 50027,
		DragonSupport = 50029,
		IceMine = 50030,
		Scan = 50031,
		SummonGolem = 50032,
		UseManaForming = 50033,
		UseSupportItem = 50034,
		TickingQuizBomb = 50035,
		ItemSeal = 50036,
		ItemUnseal = 50037,
		ItemDungeonPass = 50038,
		UseElathaItem = 50039,
		ThrowConfetti = 50040,
		UsePartyPopper = 50041,
		HammerGame = 50042,
		WaterBalloonThrowing = 50043,
		WaterBalloonThrowing2 = 50044,
		SpiritShift = 50045,
		EmergencyEscapeBomb = 50046,
		NameColorChange = 50047,
		InstallUninstallCylinder = 50048,
		HolyFlame = 50049,
		OutfitAction = 50167,

		UseMorrighansFeather = 50050,
		CreateFaliasPortal = 50051,
		UseItemChattingColorChange = 50052,
		InstallPrivateFarmFacility = 50053,
		ReorientHomesteadbuilding = 50054,

		Paint = 50055,
		MixPaint = 50056,
		ContinentWarp = 50057,
		AddSeasoning = 50058,
		WaterBalloonAttack = 50059,
		EmergencyIceBomb = 50060,
		MarionetteHiddenResurrection = 50061,
		SnowBomb = 50062,
		PetBuffing = 50101,
		Swallow = 50102,
		BubbleBlast = 50103,
		Dash = 50104,
		FakeDeathCombat = 50105,
		MasterTeleport = 50106,
		Detection = 50107,
		PetSealToItem = 50108,

		PetHide = 50109,
		DragonMeteor = 50110,
		RedDragonFireBreath = 50111,
		ThunderRain = 50112,
		/*EatVolcanicBomb = 50113, // WARNING: POSSESSING THIS SKILL *WILL* CRASH YOUR CLIENT. */
		DragonRoar = 50114,
		WyvernFireBreath = 50115,
		WyvernLightning = 50116,
		WyvernIceBreath = 50117,

		CherryTreeKit = 50118,
		DragonSupportMeteor = 50119,
		Spin = 50120,
		Bewilder = 50121,
		NeidRisingandDiving = 50122,
		NeidRoar = 50123,
		NeidTailAttack = 50124,
		NeidWaterBomb = 50125,
		Watering = 50126,
		Fertilizing = 50127,
		BugCatching = 50128,
		BeholderBeamAttack = 50129,
		BeholderAlarm = 50130,
		AxeThrowing = 50131,
		ChandelierAttack = 50132,
		ShadowDeath = 50133,
		GachaponSynthesis = 50134,
		Summon = 50135,
		ThunderStorm = 50136,
		Boost = 50137,
		ThunderBreath = 50138,
		FireStorm = 50139,
		FireBreath = 50140,
		IceStorm = 50142,
		FlameDive = 50143,
		RunningBoost = 50144,
		FlownHotAirBalloon = 50145,
		Umbrella = 50146,
		ItemSeal2 = 50147,
		CureZombie = 50148,
		HandsofSalvation = 50149,
		Scare = 50150,
		FrostStorm = 50151,
		IceBreath = 50152,
		NormalAttack = 50153,
		FlameofResurrection = 50154,
		ExplosiveResurrection = 50155,
		PhoenixsFlame = 50156,
		DevilsDash = 50157,
		PetrifyingRoar = 50158,
		DevilsCry = 50159,
		SaveLocation = 50160,
		PetTeleport = 50161,
		RestfulWind = 50162,
		HolyShower = 50163,
		HolyRush = 50164,
		SpreadWings = 50165,
		//몬스터소환 =  50500, 
		//악의확산 =  50501, 
		//생명의확산 =  50502, 
		//폭주기관차 =  50503, 
		//성지순례 =  50504, 
		//화이트브레스 =  50505, 
		//블랙브레스 =  50506, 
		//지옥의손길 =  50507, 
		//데빌허그 =  50508, 
		//칼날서리 =  50509, 
		//철옹성 =  50510, 

		// Personas
		HamletsAnguish = 51001,
		ClaudiussConspiracy = 51002,
		OpheliasTears = 51003,
		RomeosConfession = 51004,
		TybaltsFencingSkills = 51005,
		JulietsFeelings = 51006,
		ShylocksStep = 51007,

		MerrowSmash = 52000,
		MerrowRisingDragon = 52001,
		MerrowTidalWave = 52002,

		// Music
		PlayingInstrument = 10003,
		Composing = 10005,
		MusicalKnowledge = 10006,
		EnthrallingPerformance = 30015,
		Dischord = 53001,
		BattlefieldOverture = 53101,
		Lullaby = 53102,
		Vivace = 53103,
		EnduringMelody = 53104,
		HarvestSong = 53105,
		MarchSong = 53106,
		Song = 53200,

		// Puppetry
		ControlMarionette = 54001,
		PierrotMarionette = 54081,
		ColossusMarionette = 54082,
		Act2ThresholdCutter = 54101,
		Act1IncitingIncident = 54102,
		Act4RisingAction = 54103,
		Act6Crisis = 54104,
		Act7ClimacticCrash = 54105,
		Act9InvigoratingEncore = 54106,
		Act2ThresholdCutterAI = 54151,
		Act1IncitingIncidentAI = 54152,
		Act4RisingActionAI = 54153,
		Act6CrisisAI = 54154,
		Act7ClimacticCrashAI = 54155,
		Act9InvigoratingEncoreAI = 54156,
		WirePull = 54201,
		PuppetsSnare = 54202,

		// Life Part 2
		SheepShearing = 55001,
		Mining = 55002,
		EggGathering = 55003,
		MushroomGathering = 55004,
		Milking = 55005,
		Harvesting = 55006,
		Hoeing = 55007,
		CommerceMastery = 55100,
		TransformationMastery = 56001,

		// Guns
		DualGunMastery = 54302,
		FlashLauncher = 54303,
		GrappleShot = 54304,
		BulletSlide = 54305,
		ShootingRush = 54306,
		BulletStorm = 54307,
		Reload = 54401,
		WayOfTheGun = 54402,

		// Ninja
		ShurikenMastery = 26000,
		ShurikenCharge = 26001,
		KunaiStorm = 26002,
		ShadowBind = 26003,
		ShadowCloak = 26004,
		ExplosiveKunai = 26005,
		Smokescreen = 26006,
		SakuraAbyss = 26007,

		// Divine Knights
		ShieldOfTrust = 46001,
		CelestialSpike = 46002,
		JudgmentBlade = 46003,

		// GM
		PickupItemGMSkill = 65001,
		SuperWindmill = 65002,
		BlockWorldGMSkill = 65003,
		AdministrativePicking2 = 65006,
	}

	public enum PlayingQuality
	{
		VeryBad = 0,
		Bad = 1,
		Good = 2,
		VeryGood = 3
	}

	public enum SharpMindStatus
	{
		None = 0,
		Loading = 1,
		Loaded = 2,
		Cancelling = 6
	}

	/// <summary>
	/// A skill's type, not to be confused with its category.
	/// </summary>
	/// <remarks>
	/// Value "SkillType" in client. The specific purpose of this value is unknown,
	/// all we know is that skills with the type "BroadcastStartStop" are special,
	/// in that their Start/Stop packets are broadcasted, to activate special effects.
	/// </remarks>
	public enum SkillType
	{
		None = -1,
		Unk1 = 0,
		Combat = 1,
		RangedCombat = 2,
		RangedCombat2 = 3,
		Production = 4,
		MaterialProduction = 5,
		Enchanting = 6,
		BroadcastStartStop = 7,
		StartStop = 8,
		Passive = 10,
	}

	/// <summary>
	/// Method for catching something while fishing.
	/// </summary>
	public enum FishingMethod : byte
	{
		Auto,
		Manual,
	}

	/// <summary>
	/// Defines the message displayed when requesting a fishing action.
	/// </summary>
	public enum CatchSize : byte
	{
		/// <summary>
		/// Something's caught on the hook...
		/// </summary>
		Something = 0,

		/// <summary>
		/// This feels like a big one...
		/// </summary>
		BigOne = 1,

		/// <summary>
		/// A small catch...
		/// </summary>
		SmallCatch = 255,
	}

	/// <summary>
	/// Categories for production skills.
	/// </summary>
	public enum ProductionCategory : short
	{
		/// <summary>
		/// Spinning Wheel
		/// </summary>
		Spinning = 1,

		/// <summary>
		/// Loom
		/// </summary>
		Weaving = 2,

		/// <summary>
		/// Furnace
		/// </summary>
		Refining = 3,

		/// <summary>
		/// Tir Windmill
		/// </summary>
		Milling = 4,

		/// <summary>
		/// Potion Making skill
		/// </summary>
		PotionMaking = 5,

		/// <summary>
		/// Handicraft skill
		/// </summary>
		Handicraft = 6,

		/// <summary>
		/// Magic Craft skill? Magic Cauldrons? (I don't speak G14+)
		/// </summary>
		MagicCraft = 16,
	}
}
