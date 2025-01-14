﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills
{
	public static class SkillHelper
	{
		/// <summary>
		/// Reduces damage by target's defense and protection.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		public static void HandleDefenseProtection(Creature target, ref float damage, bool defense = true, bool protection = true)
		{
			if (defense)
				damage = Math.Max(1, damage - target.Defense);
			if (protection && damage > 1)
				damage = Math.Max(1, damage - (damage * target.Protection));
		}

		/// <summary>
		/// Reduces damage by target's magic defense and protection.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		public static void HandleMagicDefenseProtection(Creature target, ref float damage, bool defense = true, bool protection = true)
		{
			if (defense)
				damage = Math.Max(1, damage - target.MagicDefense);
			if (protection && damage > 1)
				damage = Math.Max(1, damage - (damage * target.MagicProtection));
		}

		/// <summary>
		/// Reduces weapon's durability and increases its proficiency.
		/// Only updates weapon type items that are not null.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="weapon"></param>
		public static void UpdateWeapon(Creature attacker, Creature target, params Item[] weapons)
		{
			if (attacker == null)
				return;

			var rnd = RandomProvider.Get();

			foreach (var weapon in weapons.Where(a => a != null && a.IsTrainableWeapon))
			{
				// Durability
				if (!ChannelServer.Instance.Conf.World.NoDurabilityLoss)
				{
					var reduce = rnd.Next(1, 30);

					// Half dura loss if blessed
					if (weapon.IsBlessed)
						reduce = Math.Max(1, reduce / 2);

					weapon.Durability -= reduce;
					Send.ItemDurabilityUpdate(attacker, weapon);
				}

				// Proficiency
				// Only if the weapon isn't broken and the target is not "Weakest".
				if (weapon.Durability != 0 && attacker != null && attacker.GetPowerRating(target) >= PowerRating.Weak)
				{
					short prof = 0;

					if (attacker.Age >= 10 && attacker.Age <= 12)
						prof = 48;
					else if (attacker.Age >= 13 && attacker.Age <= 19)
						prof = 60;
					else
						prof = 72;

					weapon.Proficiency += prof;

					Send.ItemExpUpdate(attacker, weapon);
				}
			}
		}

		/// <summary>
		/// Inflicts injuries to target, based on attacker's Injury
		/// properties and the given damage.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		public static void HandleInjury(Creature attacker, Creature target, float damage)
		{
			if (attacker == null || target == null || damage == 0)
				return;

			var rnd = RandomProvider.Get();
			var min = attacker.InjuryMin;
			var max = Math.Max(min, attacker.InjuryMax);
			if (max == 0)
				return;

			var rndInjure = rnd.Next(min, max + 1);
			if (rndInjure == 0)
				return;

			var injure = damage * (rndInjure / 100f);

			target.Injuries += injure;
		}
	}
}
