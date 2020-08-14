using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using TEHub.Configs;
using TEHub.EventClasses;
using TEHub.Extensions;
using TEHub.Teams;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;

namespace TEHub
{
    public static class HubHooks
    {
        public static void OnPlayerSpawn(object sender, GetDataHandlers.SpawnEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            Util.tSPlayersThatDied.Add(tSPlayer);

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);
            if (hubEvent == null)
            {
                return;
            }

            if (args.SpawnContext == PlayerSpawnContext.SpawningIntoWorld)
            {
                EventTeam eventTeam = hubEvent.GetTeamPlayerIn(tSPlayer);

                if (eventTeam == null)
                {
                    return;
                }

                eventTeam.SetInGameTeam(tSPlayer);
            }
        }

        public static void OnGameUpdate(EventArgs _)
        {
            foreach (TSPlayer tSPlayer in TShock.Players)
            {
                if (tSPlayer == null || !tSPlayer.Active || !tSPlayer.IsLoggedIn || !Util.tSPlayersThatDied.Contains(tSPlayer) || tSPlayer.Dead)
                {
                    continue;
                }

                HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);
                if (hubEvent == null)
                {
                    tSPlayer.ResetPlayer();
                    tSPlayer.SetAccessory(0, ItemID.ObsidianHorseshoe, (byte)Util.ItemPrefix.Lucky);
                }
                else
                {
                    if (hubEvent.TeleportToBase(tSPlayer))
                    {
                        tSPlayer.ForcePvP(true);
                    }
                    else
                    {
                        hubEvent.TeleportToSpawn(tSPlayer);

                        tSPlayer.ResetPlayer();
                        tSPlayer.SetAccessory(0, ItemID.ObsidianHorseshoe, (byte)Util.ItemPrefix.Lucky);
                    }
                }

                Util.tSPlayersThatDied.Remove(tSPlayer);
            }

            // Teleport spectating players to their targets
            foreach (TSPlayer tSPlayer in Util.spectatingPlayersToTargets.Keys)
            {
                TSPlayer target = Util.spectatingPlayersToTargets[tSPlayer];
                Vector2 position = target.TPlayer.position;
                tSPlayer.TeleportNoDust(position);
                tSPlayer.SendTileSquare((int)(position.X/16), (int)(position.Y/16));
            }

            // Call the GameUpdate method of each ongoing event
            List<HubEvent> ongoingEvents = HubEvent.GetOngoingEvents();
            foreach (HubEvent ongoingEvent in ongoingEvents)
            {
                ongoingEvent.GameUpdate();
            }

            // Automatically start any events that have enough players and have not already started
            foreach (HubEvent hubEvent in HubConfig.config.HubEvents.Except(ongoingEvents))
            {
                if (hubEvent.tSPlayers.Count >= hubEvent.minPlayersForStart)
                {
                    TShock.Utils.Broadcast("{0} has reached the minimum players to start!".SFormat(hubEvent.eventName), Color.Teal);
                    hubEvent.StartEventCountdown();
                }
            }
        }

        public static void OnPlayerTeamChange(object sender, GetDataHandlers.PlayerTeamEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);

            if (hubEvent == null)
            {
                return;
            }

            EventTeam eventTeam = hubEvent.GetTeamPlayerIn(tSPlayer);

            if (eventTeam == null)
            {
                return;
            }

            tSPlayer.Disconnect("Kicked for switching teams!");
        }

        public static void OnTogglePvp(object sender, GetDataHandlers.TogglePvpEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            args.Handled = true;

            tSPlayer.TPlayer.hostile = !args.Pvp;
            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, NetworkText.Empty, tSPlayer.Index);

            tSPlayer.SendErrorMessage("You are not allowed to toggle PvP!");
        }

        // Prevent breaking certain blocks based on the event
        public static void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            int tileX = args.X;
            int tileY = args.Y;
            
            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);
            if (hubEvent == null || hubEvent.unbreakableBlocks.Length == 0)
            {
                return;
            }

            ITile tile = Main.tile[tileX, tileY];

            int mapWidth = hubEvent.originalMapBottomRightPosX - hubEvent.originalMapTopLeftPosX;
            int mapHeight = hubEvent.originalMapBottomRightPosY - hubEvent.originalMapTopLeftPosY;

            // If the tile is not within the playable map area and is supposed to be an unbreakable block
            if (tileX < hubEvent.playableMapTopLeftPosX || tileX > hubEvent.playableMapTopLeftPosX + mapWidth ||
                tileY < hubEvent.playableMapTopLeftPosY || tileY > hubEvent.playableMapTopLeftPosY + mapHeight ||
                !hubEvent.unbreakableBlocks.Contains(tile.type))
            {
                return;
            }

            tSPlayer.SendTileSquare(tileX, tileY, 4);
            args.Handled = true;

            // TODO: Send the error message only if the player's pickaxe would normally be able to mine the tile
            //tSPlayer.SendErrorMessage("You are not allowed to edit that block!");
        }

        #region Prevent Sharing/Disposing of Class Items/Protected Items
        public static void OnItemDrop(object sender, GetDataHandlers.ItemDropEventArgs args)
        {
            // Check if the player is picking up an item
            if (args.ID != 400)
            {
                return;
            }

            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);
            if (hubEvent != null && (!hubEvent.tSPlayersWithAClass.TryGetValue(tSPlayer.Name, out EventClass eventClass) || eventClass.HasItem(args.Type)))
            {
                return;
            }

            args.Handled = true;

            tSPlayer.GiveItem(args.Type, args.Stacks, args.Prefix);

            tSPlayer.SendErrorMessage("You are not allowed to drop that item!");
        }

        public static void OnChestItemChange(object sender, GetDataHandlers.ChestItemEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);
            if (!hubEvent.tSPlayersWithAClass.TryGetValue(tSPlayer.Name, out EventClass eventClass) || eventClass.HasItem(args.Type))
            {
                return;
            }

            args.Handled = true;

            // Remove the item from the chest
            Main.chest[args.ID].item[args.Slot] = new Item();
            tSPlayer.SendData(PacketTypes.ChestItem, "", args.ID, args.Slot, args.Stacks, args.Prefix, args.Type);

            tSPlayer.GiveItem(args.Type, args.Stacks, args.Prefix);

            tSPlayer.SendErrorMessage("You are not allowed to that item in chests!");
        }

        public static void OnPlayerSlot(object sender, GetDataHandlers.PlayerSlotEventArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);

            Player player = tSPlayer.TPlayer;

            if (player.trashItem.netID > 0)
            {
                player.trashItem.netDefaults(0);

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index);

                tSPlayer.GiveItem(args.Type, args.Stack, args.Prefix);
            }
            else if ((hubEvent == null || !hubEvent.started) && args.Type == ItemID.ObsidianHorseshoe)
            {
                Util.PerformActionOnItemInSlot(player, args.Slot, (item) => item.netDefaults(0));

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, args.Slot);

                tSPlayer.SetAccessory(0, ItemID.ObsidianHorseshoe, (int)Util.ItemPrefix.Lucky);
            }
        }
        #endregion
    }
}
