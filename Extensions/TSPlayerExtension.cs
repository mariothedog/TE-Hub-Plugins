using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace TEHub.Extensions
{
    public static class TSPlayerExtension
    {
		public static void TeleportNoDust(this TSPlayer tSPlayer, Vector2 pos)
		{
			tSPlayer.SendTileSquare((int)(pos.X / 16), (int)(pos.Y / 16), 15);

			tSPlayer.TPlayer.position = pos;

			NetMessage.SendData((int)PacketTypes.Teleport, -1, -1, NetworkText.Empty, 0, tSPlayer.TPlayer.whoAmI, pos.X, pos.Y, -1);
		}

        public static void ForcePvP(this TSPlayer tSPlayer, bool on)
        {
            tSPlayer.TPlayer.hostile = on;
            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, NetworkText.Empty, tSPlayer.Index);
        }

        // Credit to: https://tshock.co/xf/index.php?resources/character-reset-ssc.4/
        #region Reset Player
        public static void ResetPlayer(this TSPlayer player)
        {
            if (Main.ServerSideCharacter)
            {
                ResetStats(player);
                ResetInventory(player);
                ResetQuests(player);
                ResetBanks(player);
            }
            else
            {
                TShock.Log.ConsoleError("The ResetPlayer method was called but SSC isn't enabled on this server!");
            }
        }

        public static void ResetPlayer(this TSPlayer player, int startHealth, int startMana)
        {
            if (Main.ServerSideCharacter)
            {
                ResetStats(player, startHealth, startMana);
                ResetInventory(player);
                ResetQuests(player);
                ResetBanks(player);
            }
            else
            {
                TShock.Log.ConsoleError("The ResetPlayer method was called but SSC isn't enabled on this server!");
            }
        }

        public static void ResetStats(this TSPlayer player)
        {
            player.TPlayer.statLife = Plugin.startHealth;
            player.TPlayer.statLifeMax = Plugin.startHealth;
            player.TPlayer.statMana = Plugin.startMana;
            player.TPlayer.statManaMax = Plugin.startMana;

            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            NetMessage.SendData((int)PacketTypes.PlayerInfo, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }

        public static void ResetStats(this TSPlayer player, int startHealth, int startMana)
        {
            player.TPlayer.statLife = startHealth;
            player.TPlayer.statLifeMax = startHealth;
            player.TPlayer.statMana = startMana;
            player.TPlayer.statManaMax = startMana;

            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            NetMessage.SendData((int)PacketTypes.PlayerInfo, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }

        public static void ResetInventory(this TSPlayer player)
        {
            ClearInventory(player);

            int slot = 0;
            Item give;
            foreach (NetItem item in Plugin.StarterItems)
            {
                give = TShock.Utils.GetItemById(item.NetId);
                give.stack = item.Stack;
                give.prefix = item.PrefixId;

                if (player.InventorySlotAvailable)
                {
                    player.TPlayer.inventory[slot] = give;
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, slot);
                    slot++;
                }
            }
        }

        private static void ClearInventory(this TSPlayer player) //The inventory clearing method from ClearInvSSC
        {
            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventorySlots) //Main Inventory
                {
                    player.TPlayer.inventory[i].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots) //Armor&Accessory slots
                {
                    var index = i - NetItem.InventorySlots;
                    player.TPlayer.armor[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots) //Dye Slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
                    player.TPlayer.dye[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots) //Misc Equip slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                    player.TPlayer.miscEquips[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                    player.TPlayer.miscDyes[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots) //piggy Bank
                {
                    //var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots);
                    //player.TPlayer.bank.item[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots) //safe Bank
                {
                    //var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots);
                    //player.TPlayer.bank2.item[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots) //Defender's Forge
                {
                    //var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots);
                    //player.TPlayer.bank3.item[index].netDefaults(0);
                }
                else
                {
                    player.TPlayer.trashItem.netDefaults(0);
                }
            }

            for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots); k++) //clear all slots excluding bank slots, bank slots cleared in ResetBanks method
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }

            var trashSlot = NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)trashSlot, 0f, 0f, 0); //trash slot

            for (int k = 0; k < Player.maxBuffs; k++)
            {
                player.TPlayer.buffType[k] = 0;
            }

            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)trashSlot, 0f, 0f, 0);

            for (int k = 0; k < Player.maxBuffs; k++)
            {
                player.TPlayer.buffType[k] = 0;
            }

            NetMessage.SendData((int)PacketTypes.PlayerInfo, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }

        public static void ResetQuests(this TSPlayer player)
        {
            player.TPlayer.anglerQuestsFinished = 0;

            NetMessage.SendData((int)PacketTypes.NumberOfAnglerQuestsCompleted, -1, -1, NetworkText.Empty, player.Index);
            NetMessage.SendData((int)PacketTypes.NumberOfAnglerQuestsCompleted, player.Index, -1, NetworkText.Empty, player.Index);
        }

        public static void ResetBanks(this TSPlayer player)
        {
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                player.TPlayer.bank.item[k].netDefaults(0);
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                player.TPlayer.bank2.item[k].netDefaults(0);
            }
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                player.TPlayer.bank3.item[k].netDefaults(0);
            }

            for (int k = NetItem.MaxInventory - (NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots) - 1; k < NetItem.MaxInventory; k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
            for (int k = NetItem.MaxInventory - (NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots) - 1; k < NetItem.MaxInventory; k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
        }
        #endregion

        // TODO - Make this work for any slot
        public static void SetAccessory(this TSPlayer tSPlayer, int slot, int itemID, byte prefix = (byte)Util.ItemPrefix.None, int stack = 1)
        {
            Player player = tSPlayer.TPlayer;

            Item item = TShock.Utils.GetItemById(itemID);
            item.prefix = prefix;
            item.stack = stack;
            player.armor[(int)Util.InventoryLengths.Armor + slot] = item;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)Util.ItemSlot.AccessorySlot1 + slot);
        }
    }
}
