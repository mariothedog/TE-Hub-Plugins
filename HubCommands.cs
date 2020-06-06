using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace TEHub
{
    public static class HubCommands
    {
        public static void HubHelp(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            string output = "";

            foreach (Command command in Commands.ChatCommands)
            {
                if (command.CommandDelegate.Method.DeclaringType == Type.GetType("TEHub.HubCommands"))
                {
                    output += string.Format("{0} - {1}", command.Name, command.HelpText);
                    output += "\n";
                }
            }
            output = output.Trim();

            tSPlayer.SendSuccessMessage(output);
        }

        public static void ReloadConfig(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            Config.config = Config.Read(Config.configPath);

            tSPlayer.SendSuccessMessage("The config was successfully reloaded.");
        }

        public static void DisplayEvents(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            foreach (HubEvent hubEvent in Config.config.HubEvents)
            {
                string players = string.Join(", ", hubEvent.tSPlayers.Select(tSP => tSP.Name)).Trim(' ', ',');
                tSPlayer.SendSuccessMessage(hubEvent.eventName + ": " + players);
            }
        }

        public static void JoinGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (HubEvent.GetEventPlayerIn(tSPlayer.Name) != null)
            {
                tSPlayer.SendErrorMessage("You're already in an event! Please use /leave first!");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <The Arctic Circle/TBR>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <The Arctic Circle/TBR>");
                return;
            }

            HubEvent.AddPlayerToEvent(tSPlayer, hubEvent);

            tSPlayer.SendSuccessMessage(string.Format("You successfully joined {0}!", hubEvent.eventName));
        }

        public static void LeaveGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer.Name);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            HubEvent.RemovePlayerFromEvent(tSPlayer, hubEvent);

            tSPlayer.SendSuccessMessage("You were successfully removed from " + hubEvent.eventName + "!");
            return;
        }

        public static void ForceJoinAll(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <The Arctic Circle/TBR>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("The event specified was not found!");
                return;
            }

            string players = "";

            foreach (TSPlayer tSP in TShock.Players)
            {
                if (tSP == null)
                {
                    continue;
                }

                // Check if player has already joined an event, and if so, remove them from it
                HubEvent playerHubEvent = HubEvent.GetEventPlayerIn(tSP.Name);
                if (playerHubEvent != null)
                {
                    HubEvent.RemovePlayerFromEvent(tSP, playerHubEvent);
                }

                HubEvent.AddPlayerToEvent(tSP, hubEvent);

                players += tSP.Name + ", ";
            }

            players = players.Trim(' ', ',');

            tSPlayer.SendSuccessMessage(string.Format("{0} was successfully added to {1}!", players, hubEvent.eventName));
        }

        public static void ForceJoin(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 2)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoin <Player Name> <The Arctic Circle/TBR>");
                return;
            }

            List<TSPlayer> tSPlayerTargets = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (tSPlayerTargets.Count == 0)
            {
                tSPlayer.SendErrorMessage("The player specified was not found!");
                return;
            }

            if (tSPlayerTargets.Count > 1)
            {
                tSPlayer.SendErrorMessage("Multiple players with that name were found!");
                return;
            }

            TSPlayer tSPlayerTarget = tSPlayerTargets.First();

            string eventName = string.Join("", args.Parameters.Skip(1)).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("The event specified was not found!");
                return;
            }

            // Check if player has already joined an event, and if so, remove them from it
            HubEvent playerHubEvent = HubEvent.GetEventPlayerIn(tSPlayerTarget.Name);
            if (playerHubEvent != null)
            {
                HubEvent.RemovePlayerFromEvent(tSPlayerTarget, playerHubEvent);
            }

            HubEvent.AddPlayerToEvent(tSPlayerTarget, hubEvent);

            tSPlayer.SendSuccessMessage(string.Format("{0} was successfully added to {1}!", tSPlayerTarget.Name, hubEvent.eventName));
        }
    
        public static void SpectatePlayer(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /spectate <Player Name>");
                return;
            }

            List<TSPlayer> tSPlayerTargets = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (tSPlayerTargets.Count == 0)
            {
                tSPlayer.SendErrorMessage("The player specified was not found!");
                return;
            }

            if (tSPlayerTargets.Count > 1)
            {
                tSPlayer.SendErrorMessage("Multiple players with that name were found!");
                return;
            }

            TSPlayer tSPlayerTarget = tSPlayerTargets.First();

            if (tSPlayer == tSPlayerTarget)
            {
                tSPlayer.SendErrorMessage("You can't spectate yourself!");
                return;
            }

            Util.spectatingPlayersToTargets.Remove(tSPlayer);
            Util.spectatingPlayersToTargets.Add(tSPlayer, tSPlayerTarget);

            Util.ResetPlayer(tSPlayer);

            tSPlayer.GodMode = true;

            player.active = false;
            NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, 0);

            tSPlayer.SendSuccessMessage("You are now spectating " + tSPlayerTarget.Name + "!");
        }

        public static void StopSpectating(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

            TSPlayer target = Util.spectatingPlayersToTargets[tSPlayer];

            Util.spectatingPlayersToTargets.Remove(tSPlayer);

            tSPlayer.GodMode = false;

            player.active = true;
            NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, 1);
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, args.Player.Index, null, args.Player.Index);
            NetMessage.SendData((int)PacketTypes.PlayerUpdate, -1, args.Player.Index, null, args.Player.Index);

            tSPlayer.SendSuccessMessage("You have stopped spectating " + target.Name + "!");
        }
    }
}
