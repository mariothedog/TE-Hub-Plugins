using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using TEHub.Configs;
using TEHub.EventClasses;
using TEHub.Extensions;
using TEHub.Teams;
using TEHub.Voting;
using Terraria;
using TShockAPI;

namespace TEHub
{
    public static class HubCommands
    {
        public readonly static string[] allKeywords = new[] { "*", "all" };

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

        public static void ReadConfig(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubConfig.config = HubConfig.Read(HubConfig.configPath);
            if (!File.Exists(HubConfig.configPath))
            {
                HubConfig.config.Write(HubConfig.configPath);
            }

            ClassConfig.config = ClassConfig.Read(ClassConfig.configPath);
            if (!File.Exists(ClassConfig.configPath))
            {
                ClassConfig.config.Write(ClassConfig.configPath);
            }

            tSPlayer.SendSuccessMessage("The config was successfully read and the server has been updated!");
        }

        public static void WriteConfig(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubConfig.config.Write(HubConfig.configPath);
            ClassConfig.config.Write(ClassConfig.configPath);

            tSPlayer.SendSuccessMessage("The config was successfully written to and has been updated!");
        }

        public static void DisplayEvents(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
            {
                string players = string.Join(", ", hubEvent.tSPlayers.Select(tSP => tSP.Name)).Trim(' ', ',');
                tSPlayer.SendSuccessMessage("{0}: {1}", hubEvent.eventName, players);
            }
        }

        public static void JoinEvent(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (HubEvent.GetEventPlayerIn(tSPlayer) != null)
            {
                tSPlayer.SendErrorMessage("You're already in an event! Please use /leave first!");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <Event Name>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();
            HubEvent hubEvent = HubEvent.GetEvent(eventName);
            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("That event was not found!");
                return;
            }

            hubEvent.tSPlayers.Add(tSPlayer);

            hubEvent.TeleportToSpawn(tSPlayer);

            tSPlayer.SendSuccessMessage("You successfully joined {0}!", hubEvent.eventName);
        }

        public static void LeaveEvent(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            hubEvent.tSPlayers.Remove(tSPlayer);

            tSPlayer.SendSuccessMessage("You were successfully removed from {0}!", hubEvent.eventName);
            return;
        }

        public static void ForceJoin(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 2)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoin <Player Name> <Event Name>");
                return;
            }

            List<TSPlayer> tSPlayerTargets;
            if (allKeywords.Contains(args.Parameters[0]))
            {
                tSPlayerTargets = TShock.Players.ToList();
            }
            else
            {
                tSPlayerTargets = TSPlayer.FindByNameOrID(args.Parameters[0]);

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
            }

            string eventName = string.Join("", args.Parameters.Skip(1)).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("The event specified was not found!");
                return;
            }

            List<TSPlayer> tSPlayersSuccessfullyJoined = new List<TSPlayer>();
            foreach (TSPlayer tSPlayerTarget in tSPlayerTargets)
            {
                if (tSPlayerTarget == null || !tSPlayerTarget.Active)
                {
                    tSPlayer.SendErrorMessage("A player failed to join.");
                    continue;
                }

                HubEvent playerHubEvent = HubEvent.GetEventPlayerIn(tSPlayerTarget);

                if (playerHubEvent == hubEvent) // Skip player if they are already in the event
                {
                    continue;
                }

                if (playerHubEvent != null) // Remove player from the event they are currently in if they are in one
                {
                    playerHubEvent.tSPlayers.Remove(tSPlayerTarget);
                }

                hubEvent.tSPlayers.Add(tSPlayerTarget);

                tSPlayersSuccessfullyJoined.Add(tSPlayerTarget);

                tSPlayerTarget.SendSuccessMessage("You were forcibly added to {0}!", hubEvent.eventName);
            }
            
            
            if (tSPlayersSuccessfullyJoined.Count == 0)
            {
                tSPlayer.SendSuccessMessage("Nobody was added to {0}.".SFormat(hubEvent.eventName));
                return;
            }

            tSPlayer.SendSuccessMessage("{0} was successfully added to {1}!".SFormat(string.Join(", ", tSPlayersSuccessfullyJoined.Select(p => p.Name)), hubEvent.eventName));
        }

        public static void ForceLeave(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forceleave <Player Name>");
                return;
            }

            List<TSPlayer> tSPlayerTargets;
            if (allKeywords.Contains(args.Parameters[0]))
            {
                tSPlayerTargets = TShock.Players.ToList();
            }
            else
            {
                tSPlayerTargets = TSPlayer.FindByNameOrID(args.Parameters[0]);

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
            }

            string eventName = string.Join("", args.Parameters.Skip(1)).ToLower();

            List<TSPlayer> tSPlayersSuccessfullyKicked = new List<TSPlayer>();
            foreach (TSPlayer tSPlayerTarget in tSPlayerTargets)
            {
                if (tSPlayerTarget == null || !tSPlayerTarget.Active)
                {
                    tSPlayer.SendErrorMessage("A player failed to leave.");
                    continue;
                }

                // Check if player has already joined an event, and if so, remove them from it
                HubEvent playerHubEvent = HubEvent.GetEventPlayerIn(tSPlayerTarget);

                if (playerHubEvent == null)
                {
                    continue;
                }

                playerHubEvent.tSPlayers.Remove(tSPlayer);

                tSPlayersSuccessfullyKicked.Add(tSPlayerTarget);

                tSPlayerTarget.SendSuccessMessage("You were forcibly removed from {0}!", playerHubEvent.eventName);
            }

            tSPlayer.SendSuccessMessage("{0} was successfully removed from their events!", string.Join(", ", tSPlayersSuccessfullyKicked.Select(p => p.Name)));
        }
    
        public static void SpectatePlayer(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = args.TPlayer;

            if (args.Parameters.Count < 1)
            {
                // Stop spectating
                if (!Util.spectatingPlayersToTargets.ContainsKey(tSPlayer))
                {
                    tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /spectate <Player Name>");
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

            Util.spectatingPlayersToTargets.Remove(tSPlayer); // If player is already spectating - For when someone wants to swap the player they're spectating with another player
            Util.spectatingPlayersToTargets.Add(tSPlayer, tSPlayerTarget);

            tSPlayer.ResetPlayer();

            tSPlayer.GodMode = true;

            player.active = false;
            NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, 0);

            tSPlayer.SendSuccessMessage("You are now spectating {0}!", tSPlayerTarget.Name);
        }
    
        public static void GetPos(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = args.TPlayer;

            tSPlayer.SendInfoMessage("You are at position X: {0}, Y: {1}!", player.position.X, player.position.Y);
        }

        public static void AddEvent(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /addevent <Event Name>");
                return;
            }

            string eventName = args.Parameters[0];

            HubConfig.config.HubEvents.Add(new HubEvent(eventName));

            tSPlayer.SendInfoMessage("The event has been successfully added! Please edit the config to customize it.");
        }

        public static void ResetMap(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /resetmap <Event Name>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("That event was not found!");
                return;
            }

            if (!hubEvent.ResetMap())
            {
                TShock.Log.ConsoleError("The ResetMap method was used but the WorldEdit plugin was not found!");
                tSPlayer.SendErrorMessage("The WorldEdit plugin is required to use this command!");
                return;
            }

            tSPlayer.SendSuccessMessage("The map was successfully reset.");
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
            tSPlayer.SendSuccessMessage("You successfully voted for \"{0}\" in \"{1}\".", optionInfo.option, votingSystem.question);
            TShock.Utils.Broadcast("{0} now has {1} vote{2}!".SFormat(optionInfo.option, optionInfo.votes, optionInfo.votes == 1 ? "" : "s"), Color.Orange);

            if (!votingSystem.HasEveryoneVoted())
            {
                return;
            }

            TShock.Utils.Broadcast("Everyone participating has voted and so the vote will conclude early.", Color.DarkOrange);

            votingSystem.Stop();
        }

        public static void EndVote(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            bool multipleOngoingVotes = VotingSystem.ongoingVotes.Count() > 1;

            int voteID = 1;

            if (args.Parameters.Count < (multipleOngoingVotes ? 2 : 1) || !int.TryParse(args.Parameters[0], out int optionID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /endvote <Option ID>" + (multipleOngoingVotes ? " <Vote ID>" : ""));
                return;
            }

            if (multipleOngoingVotes && !int.TryParse(args.Parameters[1], out voteID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /endvote <Option ID> <Vote ID>");
                return;
            }

            VotingSystem votingSystem = VotingSystem.GetVotingSystem(voteID - 1);

            if (votingSystem == null)
            {
                tSPlayer.SendErrorMessage("There is no ongoing vote with that vote ID!");
                return;
            }

            if (!votingSystem.AddVote(tSPlayer, optionID - 1))
            {
                tSPlayer.SendErrorMessage("There is no option with that ID!");
                return;
            }

            OptionInfo optionInfo = votingSystem.options[optionID - 1];
            tSPlayer.SendSuccessMessage("You successfully ended the \"{0}\" vote with \"{1}\" as the winner!", votingSystem.question, optionInfo.option);

            TShock.Utils.Broadcast("{0} has forcibly ended the vote.".SFormat(tSPlayer.Name), Color.DarkOrange);

            votingSystem.ForceStop(optionInfo);
        }

        public static void CreateVote(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 4)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /createvote <Question> <Vote Length in MS> <Option 1> <Option 2> <Optional/More Options...>");
                return;
            }

            string question = args.Parameters[0];

            if (!int.TryParse(args.Parameters[1], out int voteLengthMS))
            {
                tSPlayer.SendErrorMessage("The vote length must be a number!");
                return;
            }

            IEnumerable<string> options = args.Parameters.Skip(2);

            List<OptionInfo> optionInfos = new List<OptionInfo>();

            foreach (string option in options)
            {
                optionInfos.Add(new OptionInfo(option));
            }

            VotingSystem votingSystem = new VotingSystem(question,
                        voteLengthMS,
                        null,
                        null,
                        optionInfos.ToArray());

            tSPlayer.SendSuccessMessage("The vote was successfully created!");

            votingSystem.Start();
        }

        public static void ForceStartEvent(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent;

            if (args.Parameters.Count < 1)
            {
                hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);

                if (hubEvent == null)
                {
                    tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /start <Event Name>");
                    return;
                }
            }
            else
            {
                string eventName = string.Join("", args.Parameters).ToLower();

                hubEvent = HubEvent.GetEvent(eventName);

                if (hubEvent == null)
                {
                    tSPlayer.SendErrorMessage("That event was not found!");
                    return;
                }
            }

            if (hubEvent.ongoingCountdown || hubEvent.started)
            {
                tSPlayer.SendErrorMessage("That event has already started!");
                return;
            }

            tSPlayer.SendSuccessMessage("{0} was successfully started!", hubEvent.eventName);
            hubEvent.StartEventCountdown();
        }
    
        public static void AddClass(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = args.TPlayer;

            if (args.Parameters.Count < 2)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /addclass <Class Name> <Event Name>");
                return;
            }

            string eventName = string.Join("", args.Parameters.Skip(1)).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("That is not a valid event!");
                return;
            }

            string className = args.Parameters[0];

            EventClass eventClass = new EventClass
            {
                className = className,
                maxHealth = player.statLifeMax2,
                maxMana = player.statManaMax2,
            };

            // Main items, coins, and ammo
            for (int i = 0; i < NetItem.InventorySlots; i++)
            {
                Item item = player.inventory[i];
                if (item.netID == 0)
                {
                    continue;
                }

                ClassItem classItem = new ClassItem()
                {
                    name = item.Name,
                    stack = item.stack,
                    prefix = TShock.Utils.GetPrefixById(item.prefix)
                };

                if (i <= (int)Util.ItemSlot.InvRow5Slot10)
                {
                    eventClass.items[i] = classItem;
                }
                else if (i <= (int)Util.ItemSlot.CoinSlot4)
                {
                    int index = i - (int)Util.ItemSlot.CoinSlot1;
                    eventClass.coins[index] = classItem;
                }
                else
                {
                    int index = i - (int)Util.ItemSlot.AmmoSlot1;
                    eventClass.ammo[index] = classItem;
                }
            }

            // Armor, accessories, and their vanity
            for (int i = 0; i < NetItem.ArmorSlots; i++)
            {
                Item item = player.armor[i];
                if (item.netID == 0)
                {
                    continue;
                }

                if (i < (int)Util.InventoryLengths.Armor)
                {
                    eventClass.armor[i] = item.Name;
                }
                else if (i < (int)Util.InventoryLengths.Armor + (int)Util.InventoryLengths.Accessories)
                {
                    int index = i - (int)Util.InventoryLengths.Armor;
                    eventClass.accessories[index] = item.Name;
                }
                else if (i < (int)Util.InventoryLengths.Armor + (int)Util.InventoryLengths.AllAccessories + (int)Util.InventoryLengths.Armor)
                {
                    int index = i - ((int)Util.InventoryLengths.Armor + (int)Util.InventoryLengths.AllAccessories);
                    eventClass.armorVanity[index] = item.Name;
                }
                else
                {
                    int index = i - ((int)Util.InventoryLengths.Armor + (int)Util.InventoryLengths.AllAccessories + (int)Util.InventoryLengths.Armor);
                    eventClass.accessoryVanity[index] = item.Name;
                }
            }

            // Armor and accessory dyes
            for (int i = 0; i < NetItem.DyeSlots; i++)
            {
                Item item = player.dye[i];
                if (item.netID == 0)
                {
                    continue;
                }

                if (i < (int)Util.InventoryLengths.Armor)
                {
                    eventClass.armorDyes[i] = item.Name;
                }
                else
                {
                    int index = i - (int)Util.InventoryLengths.Armor;
                    eventClass.accessoryDyes[index] = item.Name;
                }
            }

            // MiscEquips
            for (int i = 0; i < NetItem.MiscEquipSlots; i++)
            {
                Item item = player.miscEquips[i];
                if (item.netID == 0)
                {
                    continue;
                }

                eventClass.miscEquips[i] = item.Name;
            }

            // MiscEquip Dyes
            for (int i = 0; i < NetItem.MiscEquipSlots; i++)
            {
                Item item = player.miscDyes[i];
                if (item.netID == 0)
                {
                    continue;
                }

                eventClass.miscEquipDyes[i] = item.Name;
            }

            ClassConfig.config.eventClasses[hubEvent.eventName].Add(eventClass);

            tSPlayer.SendSuccessMessage("The " + className + " class was successfully added!");
        }

        public static void ChooseClass(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = args.TPlayer;

            if (!TShock.ServerSideCharacterConfig.Enabled)
            {
                tSPlayer.SendErrorMessage("SSC is not enabled so choosing classes has been disabled.");
                return;
            }

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            if (!ClassConfig.config.eventClasses.TryGetValue(hubEvent.eventName, out List<EventClass> eventClasses) || eventClasses.Count < 1)
            {
                tSPlayer.SendErrorMessage("This event has no classes.");
                return;
            }

            if (!hubEvent.canChooseClasses)
            {
                tSPlayer.SendErrorMessage("You can no longer choose a class!");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /chooseclass <Class Name>");

                string classesFormatted = "";
                foreach (EventClass eventClass in eventClasses)
                {
                    classesFormatted += eventClass.className + ", ";
                }
                classesFormatted = classesFormatted.TrimEnd(',', ' ');

                tSPlayer.SendInfoMessage("The available classes for {0} are: {1}.", hubEvent.eventName, classesFormatted);

                return;
            }

            string className = args.Parameters[0].ToLower();

            IEnumerable<EventClass> classes = eventClasses.Where(c => c.className.ToLower() == className);

            if (classes.Count() < 1)
            {
                tSPlayer.SendErrorMessage("No classes with that name were found!");

                string classesFormatted = "";
                foreach (EventClass eventClass in eventClasses)
                {
                    classesFormatted += eventClass.className + ", ";
                }
                classesFormatted = classesFormatted.TrimEnd(',', ' ');

                tSPlayer.SendInfoMessage("Try \"/chooseclass [c/FFFF00:{0}] instead.\"", classesFormatted);
                return;
            }

            if (classes.Count() > 1)
            {
                tSPlayer.SendErrorMessage("Multiple classes with that name were found!");
                return;
            }

            EventClass chosenClass = classes.First();

            hubEvent.tSPlayersWithAClass[tSPlayer] = chosenClass;

            tSPlayer.SendSuccessMessage(string.Format("{0} class chosen!", chosenClass.className));
        }
    
        public static void EndRound(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 2)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /endround <Event Name> <Winning Team Name>");
                return;
            }

            string eventName = args.Parameters[0].ToLower();
            string winningTeamName = args.Parameters[1].ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("That event was not found!");
                return;
            }
            else if (hubEvent.resetPending)
            {
                tSPlayer.SendErrorMessage("That event is already restarting!");
                return;
            }
            else if (!hubEvent.started)
            {
                tSPlayer.SendErrorMessage("That event hasn't started!");
                return;
            }

            EventTeam winningTeam = hubEvent.GetTeam(winningTeamName);

            if (winningTeam == null)
            {
                tSPlayer.SendErrorMessage("That team was not found!");
                return;
            }

            TShock.Utils.Broadcast(string.Format("{0} TEAM HAS WON THE ROUND OF {1}!", winningTeam.name.ToUpper(), hubEvent.eventName.ToUpper()), Color.Aqua);

            // Delay so there is time for the explosives to kill players
            Timer timer = new Timer(2000)
            {
                AutoReset = false
            };
            timer.Elapsed += (sender, elapsedArgs) => hubEvent.ResetEvent();
            timer.Start();
        }
    }
}
