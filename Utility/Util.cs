using System.Collections.Generic;
using System.Linq;
using TEHub.Configs;
using TEHub.EventClasses;
using TEHub.Extensions;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace TEHub
{
    public static class Util
    {
        public static Dictionary<TSPlayer, TSPlayer> spectatingPlayersToTargets = new Dictionary<TSPlayer, TSPlayer>();

        public static List<TSPlayer> tSPlayersThatDied = new List<TSPlayer>();

        // Credit to @ggggg243
        public enum ItemPrefix
        {
            None, Large, Massive, Dangerous, Savage, Sharp, Pointy, Tiny, Terrible, Small, Dull, Unhappy, Bulky, Shameful, Heavy, Light, Sighted,
            Rapid, Hasty, Intimidating, Deadly, Staunch, Awful, Lethargic, Arkward, Powerful, Mystic, Adept, Masterful, Inept, Ignorant, Deranged, Intense, Taboo,
            Celestial, Furious, Keen, Superior, Forceful, Broken, Damaged, Shoddy, Quick, Deadly2, Agile, Nimble, Murderous, Slow, Sluggish, Lazy, Annoying, Nasty,
            Manic, Hurtful, Strong, Unpleasant, Weak, Ruthless, Frenzying, Godly, Demonic, Zealous, Hard, Guarding, Armored, Warding, Arcane, Precise, Lucky, Jagged,
            Spiked, Angry, Menacing, Brisk, Fleeting, Hasty2, Quick2, Wild, Rash, Intrepid, Violent, Legendary, Unreal, Mythical
        };

        // Credit to @ggggg243
        public enum ItemSlot
        {
            InvRow1Slot1, InvRow1Slot2, InvRow1Slot3, InvRow1Slot4, InvRow1Slot5, InvRow1Slot6, InvRow1Slot7, InvRow1Slot8, InvRow1Slot9, InvRow1Slot10,
            InvRow2Slot1, InvRow2Slot2, InvRow2Slot3, InvRow2Slot4, InvRow2Slot5, InvRow2Slot6, InvRow2Slot7, InvRow2Slot8, InvRow2Slot9, InvRow2Slot10,
            InvRow3Slot1, InvRow3Slot2, InvRow3Slot3, InvRow3Slot4, InvRow3Slot5, InvRow3Slot6, InvRow3Slot7, InvRow3Slot8, InvRow3Slot9, InvRow3Slot10,
            InvRow4Slot1, InvRow4Slot2, InvRow4Slot3, InvRow4Slot4, InvRow4Slot5, InvRow4Slot6, InvRow4Slot7, InvRow4Slot8, InvRow4Slot9, InvRow4Slot10,
            InvRow5Slot1, InvRow5Slot2, InvRow5Slot3, InvRow5Slot4, InvRow5Slot5, InvRow5Slot6, InvRow5Slot7, InvRow5Slot8, InvRow5Slot9, InvRow5Slot10,
            CoinSlot1, CoinSlot2, CoinSlot3, CoinSlot4, AmmoSlot1, AmmoSlot2, AmmoSlot3, AmmoSlot4, HandSlot,
            ArmorHeadSlot, ArmorBodySlot, ArmorLeggingsSlot, AccessorySlot1, AccessorySlot2, AccessorySlot3, AccessorySlot4, AccessorySlot5, AccessorySlot6, HiddenAccessorySlot6,
            VanityHeadSlot, VanityBodySlot, VanityLeggingsSlot, SocialAccessorySlot1, SocialAccessorySlot2, SocialAccessorySlot3, SocialAccessorySlot4, SocialAccessorySlot5, SocialAccessorySlot6, HiddenSocialAccessorySlot7,
            DyeHeadSlot, DyeBodySlot, DyeLeggingsSlot, DyeAccessorySlot1, DyeAccessorySlot2, DyeAccessorySlot3, DyeAccessorySlot4, DyeAccessorySlot5, DyeAccessorySlot6, HiddenDyeAccessorySlot7,
            EquipmentSlot1, EquipmentSlot2, EquipmentSlot3, EquipmentSlot4, EquipmentSlot5,
            DyeEquipmentSlot1, DyeEquipmentSlot2, DyeEquipmentSlot3, DyeEquipmentSlot4, DyeEquipmentSlot5
        };

        public enum InventoryLengths
        {
            Main = 50,
            Coins = 4,
            Ammo = 4,
            Armor = 3,
            Accessories = 6,
            // AllAccessories includes the hidden 7th accessory slot
            AllAccessories = 7,
            MiscEquips = 5
        }

        public enum TeamColors
        {
            White,
            Red,
            Green,
            Blue,
            Yellow,
            Pink
        }

        public static EventClass GetRandomClass(string eventName)
        {
            List<EventClass> eventClasses = ClassConfig.config.eventClasses[eventName];
            int classIndex = Plugin.UnifiedRandom.Next(eventClasses.Count);
            return eventClasses[classIndex];
        }

        public static void GiveClass(TSPlayer tSPlayer, EventClass eventClass)
        {
            Player player = tSPlayer.TPlayer;

            tSPlayer.ResetPlayer(eventClass.maxHealth, eventClass.maxMana);

            // Main items
            for (int i = 0; i < (int)InventoryLengths.Main; i++)
            {
                if (!eventClass.items.TryGetValue(i, out ClassItem classItem))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(classItem.name).First();
                item.stack = classItem.stack;
                item.prefix = (byte)TShock.Utils.GetPrefixByIdOrName(classItem.prefix).First();

                player.inventory[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, i);
            }

            // Coins
            for (int i = 0; i < (int)InventoryLengths.Coins; i++)
            {
                if (!eventClass.coins.TryGetValue(i, out ClassItem classItem))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(classItem.name).First();
                item.stack = classItem.stack;
                item.prefix = (byte)TShock.Utils.GetPrefixByIdOrName(classItem.prefix).First();

                player.inventory[(int)ItemSlot.CoinSlot1 + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.CoinSlot1 + i);
            }

            // Ammo
            for (int i = 0; i < (int)InventoryLengths.Ammo; i++)
            {
                if (!eventClass.ammo.TryGetValue(i, out ClassItem classItem))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(classItem.name).First();
                item.stack = classItem.stack;
                item.prefix = (byte)TShock.Utils.GetPrefixByIdOrName(classItem.prefix).First();

                player.inventory[(int)ItemSlot.AmmoSlot1 + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.AmmoSlot1 + i);
            }

            // Armor
            for (int i = 0; i < (int)InventoryLengths.Armor; i++)
            {
                if (!eventClass.armor.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.armor[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.ArmorHeadSlot + i);
            }

            // Accessories
            for (int i = 0; i < (int)InventoryLengths.Accessories; i++)
            {
                if (!eventClass.accessories.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.armor[(int)InventoryLengths.Armor + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.AccessorySlot1 + i);
            }

            // Armor Vanity
            for (int i = 0; i < (int)InventoryLengths.Armor; i++)
            {
                if (!eventClass.armorVanity.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.armor[(int)InventoryLengths.Armor + (int)InventoryLengths.AllAccessories + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.VanityHeadSlot + i);
            }

            // Accessory Vanity
            for (int i = 0; i < (int)InventoryLengths.Accessories; i++)
            {
                if (!eventClass.accessoryVanity.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.armor[(int)InventoryLengths.Armor + (int)InventoryLengths.AllAccessories + (int)InventoryLengths.Armor + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.SocialAccessorySlot1 + i);
            }

            // Armor Dyes
            for (int i = 0; i < (int)InventoryLengths.Armor; i++)
            {
                if (!eventClass.armorDyes.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.dye[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.DyeHeadSlot + i);
            }

            // Accessory Dyes
            for (int i = 0; i < (int)InventoryLengths.Accessories; i++)
            {
                if (!eventClass.accessoryDyes.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.dye[(int)InventoryLengths.Armor + i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.DyeAccessorySlot1 + i);
            }

            // Misc Equips
            for (int i = 0; i < (int)InventoryLengths.MiscEquips; i++)
            {
                if (!eventClass.miscEquips.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.miscEquips[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.EquipmentSlot1 + i);
            }

            // Misc Equip Dyes
            for (int i = 0; i < (int)InventoryLengths.MiscEquips; i++)
            {
                if (!eventClass.miscEquipDyes.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.miscDyes[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.DyeEquipmentSlot1 + i);
            }
        }
    }
}
