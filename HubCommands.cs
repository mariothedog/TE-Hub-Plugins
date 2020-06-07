using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEHub.Extensions;
using TEHub.Voting;
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
            if (!File.Exists(Config.configPath))
            {
                Config.config.Write(Config.configPath);
            }

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

            hubEvent.TeleportPlayerToSpawn(tSPlayer);

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

            tSPlayer.ResetPlayer();

            tSPlayer.GodMode = true;

            player.active = false;
            NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, 0);

            tSPlayer.SendSuccessMessage("You are now spectating " + tSPlayerTarget.Name + "!");
        }

        public static void StopSpectating(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

            if (!Util.spectatingPlayersToTargets.ContainsKey(tSPlayer))
            {
                tSPlayer.SendErrorMessage("You are not spectating!");
                return;
            }

            TSPlayer target = Util.spectatingPlayersToTargets[tSPlayer];

            Util.spectatingPlayersToTargets.Remove(tSPlayer);

            tSPlayer.GodMode = false;

            player.active = true;
            NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, 1);
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, args.Player.Index, null, args.Player.Index);
            NetMessage.SendData((int)PacketTypes.PlayerUpdate, -1, args.Player.Index, null, args.Player.Index);

            tSPlayer.SendSuccessMessage("You have stopped spectating " + target.Name + "!");
        }
    
        public static void GetPos(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

            tSPlayer.SendInfoMessage(string.Format("You are at position X: {0}, Y: {1}!", player.position.X, player.position.Y));
        }

        public static void AddEvent(CommandArgs args) // TODO
        {
            TSPlayer tSPlayer = args.Player;

            Config.config.HubEvents.Add(new HubEvent("The Arctic Circle",
                new [] { "thearcticcircle", "arcticcircle", "arcticcircle", "theac", "ac" },
                1,
                60000,
                102941, 13382,
                6799, 249,
                8355, 749,
                5031, 476));

            tSPlayer.SendInfoMessage("This command is a work in progress!");
        }

        public static void ResetMap(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /resetmap <The Arctic Circle/TBR>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <The Arctic Circle/TBR>");
                return;
            }

            if (hubEvent.ResetMap())
            {
                tSPlayer.SendSuccessMessage("The map was successfully reset.");
                return;
            }

            TShock.Log.ConsoleError("The ResetMap method was used but the WorldEdit plugin was not found!");
            tSPlayer.SendErrorMessage("The WorldEdit plugin is required to use this command!");
        }

        public static void Vote(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            bool multipleOngoingVotes = VotingSystem.ongoingVotes.Count() > 1;

            int voteID = 1;

            if (args.Parameters.Count < (multipleOngoingVotes ? 2 : 1) || !int.TryParse(args.Parameters[0], out int optionID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /vote <Option ID>" + (multipleOngoingVotes ? " <Vote ID>" : ""));
                return;
            }

            if (multipleOngoingVotes && !int.TryParse(args.Parameters[1], out voteID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /vote <Option ID> <Vote ID>");
                return;
            }

            VotingSystem votingSystem = VotingSystem.GetVotingSystem(voteID - 1);

            if (votingSystem == null)
            {
                tSPlayer.SendErrorMessage("There is no ongoing vote with that vote ID!");
                return;
            }

            if (votingSystem.voters.Contains(tSPlayer))
            {
                tSPlayer.SendErrorMessage("You have already voted!");
                return;
            }

            if (!votingSystem.AddVote(tSPlayer, optionID - 1))
            {
                tSPlayer.SendErrorMessage("There is no option with that ID!");
                return;
            }

            OptionInfo optionInfo = votingSystem.options[optionID - 1];
            tSPlayer.SendSuccessMessage(string.Format("You successfully voted for \"{0}\" in \"{1}\" \"{0}\" now has {2} vote{3}!", optionInfo.option, votingSystem.question, optionInfo.votes, optionInfo.votes == 1 ? "" : "s"));

            if (!votingSystem.HasEveryoneVoted())
            {
                return;
            }

            TShock.Utils.Broadcast("Everyone participating has voted and so the vote will conclude early.", Color.DarkOrange);

            votingSystem.Stop();
        }

        public static void ForceVote(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            tSPlayer.SendInfoMessage("This command is a work in progress!");
        }

        public static void CreateVote(CommandArgs args) // TODO
        {
            TSPlayer tSPlayer = args.Player;

            tSPlayer.SendInfoMessage("This command is a work in progress!");
        }

        public static void StartGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer.Name);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            tSPlayer.SendSuccessMessage("You successfully started a vote to start the event!");

            VotingSystem votingSystem = new VotingSystem("Should the game start?",
                        "The vote has tied and will now be restarted.",
                        60000,
                        hubEvent,
                        new OptionInfo("Yes", "Yes wins.", hubEvent.StartEventCountdown),
                        new OptionInfo("No", "No wins."));
            votingSystem.Start();
        }

        public static void ForceStartGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer.Name);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            tSPlayer.SendSuccessMessage("You successfully started the event!");

            TShock.Utils.Broadcast("An admin has forcibly started the event!", Color.Aquamarine);

            hubEvent.StartEventCountdown();
        }
    }
}
