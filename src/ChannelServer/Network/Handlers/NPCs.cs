﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using System.Text.RegularExpressions;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Data;
using Aura.Channel.World.Entities;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Inventory;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Request to talk to an NPC.
		/// </summary>
		/// <example>
		/// 0001 [0010F0000000032A] Long   : 4767482418037546
		/// </example>
		[PacketHandler(Op.NpcTalkStart)]
		public void NpcTalkStart(ChannelClient client, Packet packet)
		{
			var npcEntityId = packet.GetLong();

			// Check creature
			var creature = client.GetCreatureSafe(packet.Id);

			// Check lock
			if (!creature.Can(Locks.TalkToNpc))
			{
				Log.Debug("TalkToNpc locked for '{0}'.", creature.Name);
				Send.NpcTalkStartR_Fail(creature);
				return;
			}

			// Check NPC
			var target = ChannelServer.Instance.World.GetNpc(npcEntityId);
			if (target == null)
			{
				throw new ModerateViolation("Tried to talk to non-existant NPC 0x{0:X}", npcEntityId);
			}

			// Special NPC requirements
			// The Soulstream version of Nao and Tin are only available
			// in the Soulstream, and in there we can't check range.
			var bypassDistanceCheck = false;
			var disallow = false;
			if (npcEntityId == MabiId.Nao || npcEntityId == MabiId.Tin)
			{
				bypassDistanceCheck = creature.Temp.InSoulStream;
				disallow = !creature.Temp.InSoulStream;
			}

			// Some special NPCs require special permission.
			if (disallow)
			{
				throw new ModerateViolation("Tried to talk to NPC 0x{0:X} ({1}) without permission.", npcEntityId, target.Name);
			}

			// Check script
			if (target.ScriptType == null)
			{
				Send.NpcTalkStartR_Fail(creature);

				Log.Warning("NpcTalkStart: Creature '{0}' tried to talk to NPC '{1}', that doesn't have a script.", creature.Name, target.Name);
				return;
			}

			// Check distance
			if (!bypassDistanceCheck && (creature.RegionId != target.RegionId || target.GetPosition().GetDistance(creature.GetPosition()) > 1000))
			{
				Send.MsgBox(creature, Localization.Get("You're too far away."));
				Send.NpcTalkStartR_Fail(creature);

				Log.Warning("NpcTalkStart: Creature '{0}' tried to talk to NPC '{1}' out of range.", creature.Name, target.Name);
				return;
			}

			// Respond
			Send.NpcTalkStartR(creature, npcEntityId);

			// Start NPC dialog
			client.NpcSession.StartTalk(target, creature);
		}

		/// <summary>
		/// Sent when "End Conversation" button is clicked.
		/// </summary>
		/// <remarks>
		/// Not every "End Conversation" button is the same. Some send this,
		/// others, like the one you get while the keywords are open,
		/// send an "@end" response to Select instead.
		/// </remarks>
		/// <example>
		/// 001 [0010F00000000003] Long   : 4767482418036739
		/// 002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.NpcTalkEnd)]
		public void NpcTalkEnd(ChannelClient client, Packet packet)
		{
			var npcId = packet.GetLong();
			var unkByte = packet.GetByte();

			// Check creature
			var creature = client.GetCreatureSafe(packet.Id);

			// Check session
			if (!client.NpcSession.IsValid(npcId) && creature.Temp.CurrentShop == null)
			{
				Log.Warning("Player '{0}' tried ending invalid NPC session.", creature.Name);

				// Don't return, there's no harm in closing a dialog,
				// and the player could get stuck because of a bug or
				// something.
				//return;
			}

			client.NpcSession.Clear();
			creature.Temp.CurrentShop = null;

			Send.NpcTalkEndR(creature, npcId);
		}

		/// <summary>
		/// Sent whenever a button, other than "Continue", is pressed
		/// while the client is in "SelectInTalk" mode.
		/// </summary>
		/// <example>
		/// 001 [................] String : <result session='1837'><this type="character">4503599627370498</this><return type="string">@end</return></result>
		/// 002 [........0000072D] Int    : 1837
		/// </example>
		[PacketHandler(Op.NpcTalkSelect)]
		public void NpcTalkSelect(ChannelClient client, Packet packet)
		{
			var result = packet.GetString();
			var sessionid = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check session
			if (!client.NpcSession.IsValid())
			{
				// We can't throw a violation here because the client sends
				// NpcTalkSelect *after* NpcTalkEnd if you click the X in Eiry
				// while a list is open... maybe on other occasions as well,
				// so let's make it a debug msg, to not confuse admins.

				Log.Debug("NpcTalkSelect: Player '{0}' sent NpcTalkSelect for an invalid NPC session.", creature.Name);
				return;
			}

			// Check result string
			var match = Regex.Match(result, "<return type=\"string\">(?<result>[^<]*)</return>");
			if (!match.Success)
			{
				throw new ModerateViolation("Invalid NPC talk selection: {0}", result);
			}

			var response = match.Groups["result"].Value;

			// Cut @input "prefix" added for <input> element.
			if (response.StartsWith("@input"))
				response = response.Substring(7).Trim();

			// TODO: Do another keyword check, in case modders bypass the
			//   actual check below.

			// Check conversation state
			if (client.NpcSession.Script.ConversationState != ConversationState.Select)
				Log.Debug("Received Select without being in Select mode ({0}).", client.NpcSession.Script.GetType().Name);

			// Continue dialog
			client.NpcSession.Script.Resume(response);
		}

		/// <summary>
		/// Sent when selecting a keyword, to check the validity.
		/// </summary>
		/// <remarks>
		/// Client blocks until the server answers it.
		/// Failing it unblocks the client and makes it not send Select,
		/// effectively ignoring the keyword click.
		/// </remarks>
		/// <example>
		/// 001 [................] String : personal_info
		/// </example>
		[PacketHandler(Op.NpcTalkKeyword)]
		public void NpcTalkKeyword(ChannelClient client, Packet packet)
		{
			var keyword = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check session
			client.NpcSession.EnsureValid();

			// Check keyword
			if (!creature.Keywords.Has(keyword))
			{
				Send.NpcTalkKeywordR_Fail(creature);
				Log.Warning("NpcTalkKeyword: Player '{0}' tried using keyword '{1}', without knowing it.", creature.Name, keyword);
				return;
			}

			Send.NpcTalkKeywordR(creature, keyword);
		}

		/// <summary>
		/// Sent when buying an item from a regular NPC shop.
		/// </summary>
		/// <example>
		/// 0001 [005000CBB3152F26] Long   : 22518873019723558
		/// 0002 [..............00] Byte   : 0
		/// 0003 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.NpcShopBuyItem)]
		public void NpcShopBuyItem(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var targetPocket = packet.GetByte(); // 0:cursor, 1:inv
			var unk = packet.GetByte(); // storage gold?

			var creature = client.GetCreatureSafe(packet.Id);

			// Check session
			// Not compatible with remote shop access, unless we bind it to NPCs.
			//client.NpcSession.EnsureValid();

			// Check open shop
			if (creature.Temp.CurrentShop == null)
			{
				throw new ModerateViolation("Tried to buy an item with a null shop.");
			}

			// Get item
			// In theory someone could buy an item without it being visible
			// to him, but he would need the current entity id that
			// changes on each restart. It's unlikely to ever be a problem.
			var item = creature.Temp.CurrentShop.GetItem(entityId);
			if (item == null)
			{
				Log.Warning("NpcShopBuyItem: Item '{0:X16}' doesn't exist in shop.", entityId);
				goto L_Fail;
			}

			// Determine which payment method to use, the same way the client
			// does to display them. Points > Stars > Ducats > Gold.
			var paymentMethod = PaymentMethod.Gold;
			if (item.OptionInfo.StarPrice > 0)
				paymentMethod = PaymentMethod.Stars;
			if (item.OptionInfo.DucatPrice > 0)
				paymentMethod = PaymentMethod.Ducats;
			if (item.OptionInfo.PointPrice > 0)
				paymentMethod = PaymentMethod.Points;

			// Get buying price
			var price = int.MaxValue;
			switch (paymentMethod)
			{
				case PaymentMethod.Gold: price = item.OptionInfo.Price; break;
				case PaymentMethod.Stars: price = item.OptionInfo.StarPrice; break;
				case PaymentMethod.Ducats: price = item.OptionInfo.DucatPrice; break;
				case PaymentMethod.Points: price = item.OptionInfo.PointPrice; break;
			}

			// The client expects the price for a full stack to be sent
			// in the ItemOptionInfo, so we have to calculate the actual price here.
			if (item.Data.StackType == StackType.Stackable)
				price = (int)(price / (float)item.Data.StackMax * item.Amount);

			// Check currency
			var canPay = false;
			switch (paymentMethod)
			{
				case PaymentMethod.Gold: canPay = (creature.Inventory.Gold >= price); break;
				case PaymentMethod.Stars: canPay = (creature.Inventory.Stars >= price); break;
				case PaymentMethod.Ducats: canPay = false; break; // TODO: Implement ducats.
				case PaymentMethod.Points: canPay = (creature.Points >= price); break;
			}

			if (!canPay)
			{
				switch (paymentMethod)
				{
					case PaymentMethod.Gold: Send.MsgBox(creature, Localization.Get("Insufficient amount of gold.")); break;
					case PaymentMethod.Stars: Send.MsgBox(creature, Localization.Get("Insufficient amount of stars.")); break;
					case PaymentMethod.Ducats: Send.MsgBox(creature, Localization.Get("Insufficient amount of ducats.")); break;
					case PaymentMethod.Points: Send.MsgBox(creature, Localization.Get("You don't have enough Pon.\nYou will need to buy more.")); break;
				}

				goto L_Fail;
			}

			// Buy, adding item, and removing currency
			var success = false;

			// Cursor
			if (targetPocket == 0)
				success = creature.Inventory.Add(item, Pocket.Cursor);
			// Inventory
			else if (targetPocket == 1)
				success = creature.Inventory.Add(item, false);

			if (success)
			{
				// Reset gold price if payment method wasn't gold, as various
				// things depend on the gold price, like repair prices.
				// If any payment method but gold was used, the gold price
				// would be 0.
				if (paymentMethod != PaymentMethod.Gold)
					item.ResetGoldPrice();

				// Reduce
				switch (paymentMethod)
				{
					case PaymentMethod.Gold: creature.Inventory.Gold -= price; break;
					case PaymentMethod.Stars: creature.Inventory.Stars -= price; break;
					case PaymentMethod.Ducats: break; // TODO: Implement ducats.
					case PaymentMethod.Points: creature.Points -= price; break;
				}
			}

			// Response
			Send.NpcShopBuyItemR(creature, success);
			return;

		L_Fail:
			Send.NpcShopBuyItemR(creature, false);
		}

		/// <summary>
		/// Sent when selling an item from the inventory to a regular NPC shop.
		/// </summary>
		/// <example>
		/// 0001 [005000CBB3154E13] Long   : 22518873019731475
		/// 0002 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.NpcShopSellItem)]
		public void NpcShopSellItem(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var unk = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check session
			// Not compatible with remote shop access, unless we bind it to NPCs.
			//client.NpcSession.EnsureValid();

			// Check open shop
			if (creature.Temp.CurrentShop == null)
			{
				throw new ModerateViolation("Tried to sell something with current shop being null");
			}

			// Get item
			var item = creature.Inventory.GetItemSafe(entityId);

			// Check for Pon, the client doesn't let you sell items that were
			// bought with them.
			if (item.OptionInfo.PointPrice != 0)
			{
				Send.MsgBox(creature, Localization.Get("You cannot sell items bought by Pon at the shop."));
				goto L_End;
			}

			// Calculate selling price
			var sellingPrice = item.OptionInfo.SellingPrice;
			if (item.Data.StackType == StackType.Sac)
			{
				// Add costs of the items inside the sac
				sellingPrice += (int)((item.Info.Amount / (float)item.Data.StackItem.StackMax) * item.Data.StackItem.SellingPrice);
			}
			else if (item.Data.StackType == StackType.Stackable)
			{
				// Individuel price for this stack
				sellingPrice = (int)((item.Amount / (float)item.Data.StackMax) * sellingPrice);
			}

			// Remove item from inv
			if (!creature.Inventory.Remove(item))
			{
				Log.Warning("NpcShopSellItem: Failed to remove item '{0:X16}' from '{1}'s inventory.", entityId, creature.Name);
				goto L_End;
			}

			// Add gold
			creature.Inventory.AddGold(sellingPrice);

			// Respond in any case, to unlock the player
		L_End:
			Send.NpcShopSellItemR(creature);
		}

		/// <summary>
		/// Sent when clicking on close button in bank.
		/// </summary>
		/// <remarks>
		/// Doesn't lock the character if response isn't sent.
		/// </remarks>
		/// <example>
		/// 0001 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.CloseBank)]
		public void CloseBank(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			Send.CloseBankR(creature);
		}

		/// <summary>
		/// Sent when selecting which bank tabs to display (human, elf, giant).
		/// </summary>
		/// <remarks>
		/// This packet is only sent when enabling Elf or Giant, it's not sent
		/// on deactivating them, and not for Human either.
		/// It's to request data that was not sent initially,
		/// i.e. send only Human first and Elf and Giant when ticked.
		/// The client only requests those tabs once.
		/// </remarks>
		/// <example>
		/// 0001 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.RequestBankTabs)]
		public void RequestBankTabs(ChannelClient client, Packet packet)
		{
			var race = (BankTabRace)packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			// Fall back to human when race invalid
			if (race < BankTabRace.Human || race > BankTabRace.Giant)
				race = BankTabRace.Human;

			Send.OpenBank(creature, client.Account.Bank, race);
		}

		/// <summary>
		/// Sent when depositing gold in the bank.
		/// </summary>
		/// <example>
		/// 0001 [........00000014] Int    : 20
		/// </example>
		[PacketHandler(Op.BankDepositGold)]
		public void BankDepositGold(ChannelClient client, Packet packet)
		{
			var amount = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check creature's gold
			if (creature.Inventory.Gold < amount)
				throw new ModerateViolation("BankDepositGold: '{0}' ({1:X16}) tried to deposit more than he has.", creature.Name, creature.EntityId);

			// Check bank max gold
			var goldMax = Math.Min((long)int.MaxValue, client.Account.Characters.Count * (long)ChannelServer.Instance.Conf.World.BankGoldPerCharacter);
			if ((long)client.Account.Bank.Gold + amount > goldMax)
			{
				Send.MsgBox(creature, Localization.Get("The maximum amount of gold you may store in the bank is {0:n0}."), goldMax);
				Send.BankDepositGoldR(creature, false);
				return;
			}

			// Transfer gold
			creature.Inventory.RemoveGold(amount);
			client.Account.Bank.AddGold(creature, amount);

			// Response
			Send.BankDepositGoldR(creature, true);
		}

		/// <summary>
		/// Sent when withdrawing gold from the bank.
		/// </summary>
		/// <example>
		/// 0001 [..............00] Byte   : 0
		/// 0002 [........00000014] Int    : 20
		/// </example>
		[PacketHandler(Op.BankWithdrawGold)]
		public void BankWithdrawGold(ChannelClient client, Packet packet)
		{
			var createCheck = packet.GetBool();
			var withdrawAmount = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Add fee for checks
			var removeAmount = withdrawAmount;
			if (createCheck)
				removeAmount += withdrawAmount / 20; // +5%

			// Check bank gold
			if (client.Account.Bank.Gold < removeAmount)
			{
				// Don't throw a violation, it's possible to accidentally
				// bypass the client side check, in which case someone would
				// be banned wrongfully.
				//throw new ModerateViolation("BankWithdrawGold: '{0}' ({1}) tried to withdraw more than he has ({2}/{3}).", creature.Name, creature.EntityIdHex, removeAmount, client.Account.Bank.Gold);

				Send.MsgBox(creature, Localization.Get("Unable to pay the fee, Insufficient balance."));
				Send.BankWithdrawGoldR(creature, false);
				return;
			}

			// Add gold to inventory if no check
			if (!createCheck)
			{
				creature.Inventory.AddGold(withdrawAmount);
			}
			// Add check item to creature's cursor pocket if check
			else
			{
				var item = Item.CreateCheck(withdrawAmount);

				// Try to add check to cursor
				if (!creature.Inventory.Add(item, Pocket.Cursor))
				{
					// This shouldn't happen.
					Log.Debug("BankWithdrawGold: Unable to add check to cursor.");

					Send.BankWithdrawGoldR(creature, false);
					return;
				}
			}

			// Remove gold from bank
			client.Account.Bank.RemoveGold(creature, removeAmount);

			// Response
			Send.BankWithdrawGoldR(creature, true);
		}

		/// <summary>
		/// Sent when putting an item into the bank.
		/// </summary>
		/// <example>
		/// 0001 [005000CA6F3EE634] Long   : 22518867586639412
		/// 0002 [................] String : Exec
		/// 0003 [........0000000B] Int    : 11
		/// 0004 [........00000004] Int    : 4
		/// </example>
		[PacketHandler(Op.BankDepositItem)]
		public void BankDepositItem(ChannelClient client, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var tabName = packet.GetString();
			var posX = packet.GetInt();
			var posY = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check premium
			if (!client.Account.PremiumServices.CanUseAllBankTabs && tabName != creature.Name)
			{
				// Unofficial
				Send.MsgBox(creature, Localization.Get("Inventory Plus is required to access other character's bank tabs."));
				Send.BankDepositItemR(creature, false);
				return;
			}

			// Deposit item
			// TODO: Handle different banks in different towns.
			var success = client.Account.Bank.DepositItem(creature, itemEntityId, "Global", tabName, posX, posY);

			Send.BankDepositItemR(creature, success);
		}

		/// <summary>
		/// Sent when taking an item out of the bank.
		/// </summary>
		/// <example>
		/// 0001 [................] String : Exec
		/// 0002 [005000CA6F3EE634] Long   : 22518867586639412
		/// </example>
		[PacketHandler(Op.BankWithdrawItem)]
		public void BankWithdrawItem(ChannelClient client, Packet packet)
		{
			var tabName = packet.GetString();
			var itemEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check premium
			if (!client.Account.PremiumServices.CanUseAllBankTabs && tabName != creature.Name)
			{
				// Unofficial
				Send.MsgBox(creature, Localization.Get("Inventory Plus is required to access other character's bank tabs."));
				Send.BankWithdrawItemR(creature, false);
				return;
			}

			// Withdraw item
			// TODO: Handle different banks in different towns.
			var success = client.Account.Bank.WithdrawItem(creature, tabName, itemEntityId);

			Send.BankWithdrawItemR(creature, success);
		}

		/// <summary>
		/// Sent to speak to ego weapon.
		/// </summary>
		/// <remarks>
		/// The only parameter we get is the race of the ego the client selects.
		/// It seems to start looking for egos at the bottom right of the inv.
		/// 
		/// The client can handle multiple egos, but it's really made for one.
		/// It only shows the correct aura if you have only one equipped
		/// and since it starts looking for the ego to talk to in the inventory
		/// you would have to equip the ego you *don't* want to talk to...
		/// 
		/// If you right click the ego to talk to a specific one you get the
		/// correct ego race, but it will still show the stats of the auto-
		/// selected one.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.NpcTalkEgo)]
		public void NpcTalkEgo(ChannelClient client, Packet packet)
		{
			var egoRace = (EgoRace)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var items = creature.Inventory.GetItems();

			// Stop if race is somehow invalid
			if (egoRace <= EgoRace.None || egoRace > EgoRace.CylinderF)
			{
				Log.Warning("NpcTalkEgo: Invalid ego race '{0}'.", egoRace);
				Send.ServerMessage(creature, Localization.Get("Invalid ego race."));
				Send.NpcTalkEgoR(creature, false, 0, null, null);
				return;
			}

			// Check multi-ego
			// TODO: We can implement multi-ego for the same ego race
			//   once we know how the client selects them.
			//   *Should* we implement that without proper support though?
			if (items.Count(item => item.EgoInfo.Race == egoRace) > 1)
			{
				Send.ServerMessage(creature, Localization.Get("Multiple egos of the same type are currently not supported."));
				Send.NpcTalkEgoR(creature, false, 0, null, null);
				return;
			}

			// Get weapon by race
			var weapon = items.FirstOrDefault(item => item.EgoInfo.Race == egoRace);
			if (weapon == null)
				throw new SevereViolation("Player tried to talk to an ego he doesn't have ({0})", egoRace);

			// Save reference for the NPC
			creature.Vars.Temp["ego"] = weapon;

			// Get NPC name by race
			var npcName = "ego_eiry";
			switch (egoRace)
			{
				case EgoRace.SwordM: npcName = "ego_male_sword"; break;
				case EgoRace.SwordF: npcName = "ego_female_sword"; break;
				case EgoRace.BluntM: npcName = "ego_male_blunt"; break;
				case EgoRace.BluntF: npcName = "ego_female_blunt"; break;
				case EgoRace.WandM: npcName = "ego_male_wand"; break;
				case EgoRace.WandF: npcName = "ego_female_wand"; break;
				case EgoRace.BowM: npcName = "ego_male_bow"; break;
				case EgoRace.BowF: npcName = "ego_female_bow"; break;
				case EgoRace.CylinderM: npcName = "ego_male_cylinder"; break;
				case EgoRace.CylinderF: npcName = "ego_female_cylinder"; break;
			}

			// Get display name
			var displayName = "Eiry";
			if (egoRace < EgoRace.EirySword || egoRace > EgoRace.EiryWind)
				displayName = string.Format(Localization.Get("{0} of {1}"), weapon.EgoInfo.Name, creature.Name);

			// Get NPC for dialog
			var npc = ChannelServer.Instance.World.GetNpc("_" + npcName);
			if (npc == null)
			{
				Log.Error("NpcTalkEgo: Ego NPC not found ({0})", npcName);
				Send.NpcTalkEgoR(creature, false, 0, null, null);
				return;
			}

			// Success
			Send.NpcTalkEgoR(creature, true, npc.EntityId, npcName, displayName);

			// Start dialog
			client.NpcSession.StartTalk(npc, creature);
		}
	}
}
