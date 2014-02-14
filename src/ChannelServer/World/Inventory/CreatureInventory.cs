﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
	/// <summary>
	/// Inventory for players
	/// </summary>
	/// <remarks>
	/// TODO: I'm dirty and unsafe, clean me up.
	/// </remarks>
	public class CreatureInventory
	{
		private const int DefaultWidth = 6;
		private const int DefaultHeight = 10;
		private const int MaxWidth = 32;
		private const int MaxHeight = 32;
		private const int GoldItemId = 2000;
		private const int GoldStackMax = 1000;

		private Creature _creature;
		private Dictionary<Pocket, InventoryPocket> _pockets;

		/// <summary>
		/// List of all items in this inventory.
		/// </summary>
		public IEnumerable<Item> Items
		{
			get
			{
				foreach (var pocket in _pockets.Values)
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;

				//var x = (from pocket in _pockets.Values.Where(a => a.Pocket.IsEquip())
				//         where pocket.Items.Any()
				//         select pocket into p
				//         from item in p.Items
				//         where item != null
				//         select item).ToArray();
			}
		}

		/// <summary>
		/// List of all items sitting in equipment pockets in this inventory.
		/// </summary>
		public IEnumerable<Item> Equipment
		{
			get
			{
				foreach (var pocket in _pockets.Values.Where(a => a.Pocket.IsEquip()))
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;
			}
		}

		/// <summary>
		/// List of all items in equipment slots, minus hair and face.
		/// </summary>
		public IEnumerable<Item> ActualEquipment
		{
			get
			{
				foreach (var pocket in _pockets.Values.Where(a => a.Pocket.IsEquip() && a.Pocket != Pocket.Hair && a.Pocket != Pocket.Face))
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;
			}
		}

		private WeaponSet _weaponSet;
		/// <summary>
		/// Sets or returns the selected weapon set.
		/// </summary>
		public WeaponSet WeaponSet
		{
			get { return _weaponSet; }
			set
			{
				_weaponSet = value;
				this.UpdateEquipReferences(Pocket.RightHand1, Pocket.LeftHand1, Pocket.Magazine1);
				Send.StatUpdate(_creature, StatUpdateType.Private,
					Stat.AttackMinBaseMod, Stat.AttackMaxBaseMod,
					Stat.WAttackMinBaseMod, Stat.WAttackMaxBaseMod,
					Stat.BalanceBaseMod, Stat.CriticalBaseMod,
					Stat.DefenseBaseMod, Stat.ProtectionBaseMod
				);
			}
		}

		/// <summary>
		/// Reference to the item currently equipped in the right hand.
		/// </summary>
		public Item RightHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the left hand.
		/// </summary>
		public Item LeftHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the magazine (eg arrows).
		/// </summary>
		public Item Magazine { get; protected set; }

		/// <summary>
		/// Returns the amount of gold items in the inventory.
		/// </summary>
		public int Gold
		{
			get { return this.Count(GoldItemId); }
		}

		public CreatureInventory(Creature creature)
		{
			_creature = creature;

			_pockets = new Dictionary<Pocket, InventoryPocket>();

			// Cursor, Temp, Quests
			this.Add(new InventoryPocketStack(Pocket.Temporary));
			this.Add(new InventoryPocketStack(Pocket.Quests));
			this.Add(new InventoryPocketSingle(Pocket.Cursor));

			// Equipment
			for (var i = Pocket.Face; i <= Pocket.Accessory2; ++i)
				this.Add(new InventoryPocketSingle(i));

			// Style
			for (var i = Pocket.ArmorStyle; i <= Pocket.RobeStyle; ++i)
				this.Add(new InventoryPocketSingle(i));
		}

		/// <summary>
		/// Adds pocket to inventory.
		/// </summary>
		/// <param name="inventoryPocket"></param>
		public void Add(InventoryPocket inventoryPocket)
		{
			if (_pockets.ContainsKey(inventoryPocket.Pocket))
				Log.Warning("Replacing pocket '{0}' in '{1}'s inventory.", inventoryPocket.Pocket, _creature);

			_pockets[inventoryPocket.Pocket] = inventoryPocket;
		}

		/// <summary>
		/// Adds main inventories (inv, personal, VIP). Call after creature's
		/// defaults (RaceInfo) have been loaded.
		/// </summary>
		public void AddMainInventory()
		{
			if (_creature.RaceData == null)
				Log.Warning("Race for creature '{0}' ({1}) not loaded before initializing main inventory.", _creature.Name, _creature.EntityIdHex);

			var width = (_creature.RaceData != null ? _creature.RaceData.InventoryWidth : DefaultWidth);
			if (width > MaxWidth)
			{
				width = MaxWidth;
				Log.Warning("AddMainInventory: Width exceeds max, using {0} instead.", MaxWidth);
			}

			var height = (_creature.RaceData != null ? _creature.RaceData.InventoryHeight : DefaultHeight);
			if (height > MaxHeight)
			{
				height = MaxHeight;
				Log.Warning("AddMainInventory: Height exceeds max, using {0} instead.", MaxHeight);
			}

			// TODO: Race check
			this.Add(new InventoryPocketNormal(Pocket.Inventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.PersonalInventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.VIPInventory, width, height));
		}

		/// <summary>
		/// Returns true if pocket exists in this inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public bool Has(Pocket pocket)
		{
			return _pockets.ContainsKey(pocket);
		}

		/// <summary>
		/// Returns item with the id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item GetItem(long entityId)
		{
			foreach (var pocket in _pockets.Values)
			{
				var item = pocket.GetItem(entityId);
				if (item != null)
					return item;
			}

			return null;
		}

		/// <summary>
		/// Returns item at the location, or null.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Item GetItemAt(Pocket pocket, int x, int y)
		{
			if (!this.Has(pocket))
				return null;

			return _pockets[pocket].GetItemAt(x, y);
		}

		// Handlers
		// ------------------------------------------------------------------

		/// <summary>
		/// Used from MoveItem handler.
		/// </summary>
		/// <remarks>
		/// The item is the one that's interacted with, the one picked up
		/// when taking it, the one being put into a packet when it's one
		/// the cursor. Colliding items switch places with it.
		/// </remarks>
		/// <param name="item">Item to move</param>
		/// <param name="target">Pocket to move it to</param>
		/// <param name="targetX"></param>
		/// <param name="targetY"></param>
		/// <returns></returns>
		public bool Move(Item item, Pocket target, byte targetX, byte targetY)
		{
			if (!this.Has(target))
				return false;

			var source = item.Info.Pocket;
			var amount = item.Info.Amount;

			Item collidingItem = null;
			if (!_pockets[target].TryAdd(item, targetX, targetY, out collidingItem))
				return false;

			// If amount differs (item was added to stack)
			if (collidingItem != null && item.Info.Amount != amount)
			{
				Send.ItemAmount(_creature, collidingItem);

				// Left overs, update
				if (item.Info.Amount > 0)
				{
					Send.ItemAmount(_creature, item);
				}
				// All in, remove from cursor.
				else
				{
					_pockets[item.Info.Pocket].Remove(item);
					Send.ItemRemove(_creature, item);
				}
			}
			else
			{
				// Remove the item from the source pocket
				_pockets[source].Remove(item);

				// Toss it in, it should be the cursor.
				if (collidingItem != null)
					_pockets[source].Add(collidingItem);

				Send.ItemMoveInfo(_creature, item, source, collidingItem);
			}

			this.UpdateInventory(item, source, target);

			return true;
		}

		/// <summary>
		/// Moving item between char and pet, used from handler.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="other"></param>
		/// <param name="target"></param>
		/// <param name="targetX"></param>
		/// <param name="targetY"></param>
		/// <returns></returns>
		public bool MovePet(Item item, Creature other, Pocket target, int targetX, int targetY)
		{
			if (!this.Has(target) || !other.Inventory.Has(target))
				return false;

			var source = item.Info.Pocket;
			var amount = item.Info.Amount;
			//var newItem = new Item(item);

			Item collidingItem = null;
			if (!other.Inventory._pockets[target].TryAdd(item, (byte)targetX, (byte)targetY, out collidingItem))
				return false;

			// If amount differs (item was added to stack)
			if (collidingItem != null && item.Info.Amount != amount)
			{
				Send.ItemAmount(other, collidingItem);

				// Left overs, update
				if (item.Info.Amount > 0)
				{
					Send.ItemAmount(_creature, item);
				}
				// All in, remove from cursor.
				else
				{
					_pockets[item.Info.Pocket].Remove(item);
					Send.ItemRemove(_creature, item);
				}
			}
			else
			{
				// Remove the item from the source pocket
				_pockets[source].Remove(item);
				Send.ItemRemove(_creature, item, source);

				if (collidingItem != null)
				{
					// Remove colliding item
					Send.ItemRemove(other, collidingItem, target);

					// Toss it in, it should be the cursor.
					_pockets[source].Add(collidingItem);
					Send.ItemNew(_creature, collidingItem);
				}

				Send.ItemNew(other, item);

				Send.ItemMoveInfo(_creature, item, source, collidingItem);
			}

			return true;
		}

		/// <summary>
		/// Tries to put item into the inventory, by filling stacks and adding it.
		/// If it was completely added to the inventory, it's removed
		/// from the region the inventory's creature is in.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool PickUp(Item item)
		{
			var originalAmount = item.Info.Amount;

			// Try stacks/sacs first
			if (item.Data.StackType == StackType.Stackable)
			{
				List<Item> changed;
				_pockets[Pocket.Inventory].FillStacks(item, out changed);
				this.UpdateChangedItems(changed);
			}

			// Add new items as long as needed
			while (item.Info.Amount > 0)
			{
				// Making a copy of the item, and generating a new temp id,
				// ensures that we can still remove the item from the ground
				// after moving it (region, x, y) to the pocket.
				// (TODO: Remove takes an id parameter, this can be solved
				//   properly, see pet invs.)
				// We also need the new id to prevent conflicts in the db
				// (SVN r67).

				var newStackItem = new Item(item);
				newStackItem.Info.Amount = Math.Min(item.Info.Amount, item.Data.StackMax);

				// Stop if no new items can be added (no space left)
				if (!_pockets[Pocket.Inventory].Add(newStackItem))
					break;

				Send.ItemNew(_creature, newStackItem);
				item.Info.Amount -= newStackItem.Info.Amount;
			}

			if (item.Info.Amount != originalAmount && _creature.IsPlayer)
			{
				ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature as PlayerCreature, item.Info.Id, (originalAmount - item.Info.Amount));
			}

			// Fail if not everything could be picked up.
			if (item.Info.Amount > 0)
				return false;

			// Remove from map if item is in inv 100%
			_creature.Region.RemoveItem(item);
			return true;
		}

		// Adding
		// ------------------------------------------------------------------

		/// <summary>
		/// Tries to add item to pocket. Returns false if the pocket
		/// doesn't exist or there was no space.
		/// </summary>
		public bool Add(Item item, Pocket pocket)
		{
			if (!_pockets.ContainsKey(pocket))
				return false;

			var success = _pockets[pocket].Add(item);
			if (success)
			{
				Send.ItemNew(_creature, item);
				this.UpdateEquipReferences(pocket);
			}

			return success;
		}

		/// <summary>
		/// Adds item to pocket at the position it currently has.
		/// Returns false if pocket doesn't exist.
		/// </summary>
		public bool InitAdd(Item item)
		{
			if (!_pockets.ContainsKey(item.Info.Pocket))
				return false;

			_pockets[item.Info.Pocket].AddUnsafe(item);
			this.UpdateEquipReferences(item.Info.Pocket);
			return true;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories, using the temp
		/// inv as fallback (if specified to do so). Returns false if
		/// there was no space.
		/// </summary>
		public bool Add(Item item, bool tempFallback)
		{
			bool success;

			lock (_pockets)
			{
				// Try inv
				success = _pockets[Pocket.Inventory].Add(item);

				// Try temp
				if (!success && tempFallback)
					success = _pockets[Pocket.Temporary].Add(item);
			}

			// Inform about new item
			if (success)
				Send.ItemNew(_creature, item);

			return success;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories,
		/// using temp as fallback. Unlike "Add" the item will be filled
		/// into stacks first, if possible, before calling Add.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback"></param>
		/// <returns></returns>
		public bool Insert(Item item, bool tempFallback)
		{
			if (item.Data.StackType == StackType.Stackable)
			{
				// Try stacks/sacs first
				List<Item> changed;
				lock (_pockets)
				{
					_pockets[Pocket.Inventory].FillStacks(item, out changed);
					this.UpdateChangedItems(changed);

					// Add new item stacks as long as needed.
					while (item.Info.Amount > item.Data.StackMax)
					{
						var newStackItem = new Item(item);
						newStackItem.Info.Amount = item.Data.StackMax;

						// Break if no new items can be added (no space left)
						if (!_pockets[Pocket.Inventory].Add(newStackItem))
							break;

						Send.ItemNew(_creature, newStackItem);
						item.Info.Amount -= item.Data.StackMax;
					}
				}

				if (item.Info.Amount == 0)
					return true;
			}

			return this.Add(item, tempFallback);
		}

		/// <summary>
		/// Adds a new item to the inventory.
		/// </summary>
		/// <remarks>
		/// For stackables and sacs the amount is capped at stack max.
		/// - If item is stackable, it will be added to existing stacks first.
		///   New stacks are added afterwards, if necessary.
		/// - If item is a sac it's simply added as one item.
		/// - If it's a normal item, it's added times the amount.
		/// </remarks>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Add(int itemId, int amount = 1)
		{
			var newItem = new Item(itemId);
			newItem.Amount = amount;

			if (newItem.Data.StackType == StackType.Stackable)
			{
				// Insert new stacks till amount is 0.
				int stackMax = newItem.Data.StackMax;
				do
				{
					var stackAmount = Math.Min(stackMax, amount);

					var stackItem = new Item(itemId);
					stackItem.Amount = stackAmount;

					var result = this.Insert(stackItem, true);
					if (!result)
						return false;

					amount -= stackAmount;
				}
				while (amount > 0);
			}
			else if (newItem.Data.StackType == StackType.Sac)
			{
				// Add sac item with amount once
				return this.Add(newItem, true);
			}
			else
			{
				// Add item x times
				for (int i = 0; i < amount; ++i)
				{
					if (!this.Add(new Item(itemId), true))
						return false;
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds new gold stacks to the inventory until the amount was added.
		/// Spare gold will simply be ignored.
		/// TODO: Add it to temp? Drop it?
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool AddGold(int amount)
		{
			//// Add gold, stack for stack
			//do
			//{
			//    var stackAmount = Math.Min(GoldStackMax, amount);
			//    this.Add(GoldItemId, stackAmount);
			//    amount -= stackAmount;
			//}
			//while (amount > 0);

			//return true;
			return this.Add(GoldItemId, amount);
		}

		// Removing
		// ------------------------------------------------------------------

		/// <summary>
		/// Removes item from inventory, if it is in it, and sends update packets.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(Item item)
		{
			lock (_pockets)
			{
				foreach (var pocket in _pockets.Values)
				{
					if (pocket.Remove(item))
					{
						Send.ItemRemove(_creature, item);

						this.UpdateInventory(item, item.Info.Pocket, Pocket.None);

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the amount of items with the id from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <remarks>
		/// Does not check amount before removing.
		/// </remarks>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Remove(int itemId, int amount = 1)
		{
			if (amount < 0)
				amount = 0;

			var changed = new List<Item>();

			lock (_pockets)
			{
				foreach (var pocket in _pockets.Values)
				{
					amount -= pocket.Remove(itemId, amount, ref changed);

					if (amount == 0)
						break;
				}
			}

			this.UpdateChangedItems(changed);

			return (amount == 0);
		}

		/// <summary>
		/// Removes the amount of gold from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool RemoveGold(int amount)
		{
			return this.Remove(GoldItemId, amount);
		}

		/// <summary>
		/// Reduces item's amount and sends the necessary update packets.
		/// Also removes the item, if it's not a sack and its amount reaches 0.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Decrement(Item item, ushort amount = 1)
		{
			if (!this.Has(item) || item.Info.Amount == 0 || item.Info.Amount < amount)
				return false;

			item.Info.Amount -= amount;

			if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
			{
				Send.ItemAmount(_creature, item);
			}
			else
			{
				this.Remove(item);
				Send.ItemRemove(_creature, item);
			}

			return true;
		}

		// Checks
		// ------------------------------------------------------------------

		/// <summary>
		/// Returns true uf the item exists in this inventory.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Has(Item item)
		{
			lock (_pockets)
				foreach (var pocket in _pockets.Values)
					if (pocket.Has(item))
						return true;

			return false;
		}

		/// <summary>
		/// Returns the amount of items with this id in the inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public int Count(int itemId)
		{
			var result = 0;

			lock (_pockets)
				foreach (var pocket in _pockets.Values)
					result += pocket.Count(itemId);

			return result;
		}

		/// <summary>
		/// Returns whether inventory contains the item in this amount.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Has(int itemId, int amount = 1)
		{
			return (this.Count(itemId) >= amount);
		}

		/// <summary>
		/// Returns whether inventory contains gold in this amount.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool HasGold(int amount)
		{
			return this.Has(GoldItemId, amount);
		}

		// Helpers
		// ------------------------------------------------------------------

		/// <summary>
		/// Checks and updates all equipment references for source and target.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void UpdateInventory(Item item, Pocket source, Pocket target)
		{
			this.CheckLeftHand(item, source, target);
			this.UpdateEquipReferences(source, target);
			this.CheckEquipMoved(item, source, target);
		}

		/// <summary>
		/// Sends amount update or remove packets for all items, depending on
		/// their amount.
		/// </summary>
		/// <param name="items"></param>
		private void UpdateChangedItems(IEnumerable<Item> items)
		{
			if (items == null)
				return;

			foreach (var item in items)
			{
				if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
					Send.ItemAmount(_creature, item);
				else
					Send.ItemRemove(_creature, item);
			}
		}

		/// <summary>
		/// Unequips item in left hand/magazine, if item in right hand is moved.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckLeftHand(Item item, Pocket source, Pocket target)
		{
			var pocketOfInterest = Pocket.None;

			if (source == Pocket.RightHand1 || source == Pocket.RightHand2)
				pocketOfInterest = source;
			if (target == Pocket.RightHand1 || target == Pocket.RightHand2)
				pocketOfInterest = target;

			if (pocketOfInterest == Pocket.None)
				return;

			// Check LeftHand first, switch to Magazine if it's empty
			var leftPocket = pocketOfInterest + 2; // Left Hand 1/2
			var leftItem = _pockets[leftPocket].GetItemAt(0, 0);
			if (leftItem == null)
			{
				leftPocket += 2; // Magazine 1/2
				leftItem = _pockets[leftPocket].GetItemAt(0, 0);

				// Nothing to remove
				if (leftItem == null)
					return;
			}

			// Try inventory first.
			// TODO: List of pockets stuff can be auto-moved to.
			var success = _pockets[Pocket.Inventory].Add(leftItem);

			// Fallback, temp inv
			if (!success)
				success = _pockets[Pocket.Temporary].Add(leftItem);

			if (success)
			{
				_pockets[leftPocket].Remove(leftItem);

				Send.ItemMoveInfo(_creature, leftItem, leftPocket, null);
				Send.EquipmentMoved(_creature, leftPocket);
			}
		}

		/// <summary>
		/// Updates quick access equipment refernces.
		/// </summary>
		/// <param name="toCheck"></param>
		private void UpdateEquipReferences(params Pocket[] toCheck)
		{
			var firstSet = (this.WeaponSet == WeaponSet.First);
			var updatedHands = false;

			foreach (var pocket in toCheck)
			{
				// Update all "hands" at once, easier.
				if (!updatedHands && pocket >= Pocket.RightHand1 && pocket <= Pocket.Magazine2)
				{
					this.RightHand = _pockets[firstSet ? Pocket.RightHand1 : Pocket.RightHand2].GetItemAt(0, 0);
					this.LeftHand = _pockets[firstSet ? Pocket.LeftHand1 : Pocket.LeftHand2].GetItemAt(0, 0);
					this.Magazine = _pockets[firstSet ? Pocket.Magazine1 : Pocket.Magazine2].GetItemAt(0, 0);

					// Don't do it twice.
					updatedHands = true;
				}
			}
		}

		/// <summary>
		/// Runs equipment updates if necessary.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckEquipMoved(Item item, Pocket source, Pocket target)
		{
			if (source.IsEquip())
				Send.EquipmentMoved(_creature, source);

			if (target.IsEquip())
				Send.EquipmentChanged(_creature, item);

			// Send stat update when moving equipment
			if (source.IsEquip() || target.IsEquip())
			{
				Send.StatUpdate(_creature, StatUpdateType.Private,
					Stat.AttackMinBaseMod, Stat.AttackMaxBaseMod,
					Stat.WAttackMinBaseMod, Stat.WAttackMaxBaseMod,
					Stat.BalanceBaseMod, Stat.CriticalBaseMod,
					Stat.DefenseBaseMod, Stat.ProtectionBaseMod
				);
			}
		}

		// Stat mods
		// TODO: Use the actual stat mods on equip/unequip.
		// ------------------------------------------------------------------

		/// <summary>
		/// Returns defense granted by equipment.
		/// </summary>
		/// <returns></returns>
		public int GetEquipmentDefense()
		{
			var result = 0;

			foreach (var pocket in _pockets.Values.Where(a => (a.Pocket >= Pocket.Armor && a.Pocket <= Pocket.Robe) || (a.Pocket >= Pocket.Accessory1 && a.Pocket <= Pocket.Accessory2)))
				foreach (var item in pocket.Items.Where(a => a != null))
					result += item.OptionInfo.Defense;

			return result;
		}

		/// <summary>
		/// Returns protection granted by equipment.
		/// </summary>
		/// <returns></returns>
		public int GetEquipmentProtection()
		{
			var result = 0;

			foreach (var pocket in _pockets.Values.Where(a => (a.Pocket >= Pocket.Armor && a.Pocket <= Pocket.Robe) || (a.Pocket >= Pocket.Accessory1 && a.Pocket <= Pocket.Accessory2)))
				foreach (var item in pocket.Items.Where(a => a != null))
					result += item.OptionInfo.Protection;

			return result;
		}
	}

	public enum WeaponSet : byte
	{
		First = 0,
		Second = 1,
	}
}
