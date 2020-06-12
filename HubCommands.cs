using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEHub.Configs;
using TEHub.EventClasses;
using TEHub.Extensions;
using TEHub.Voting;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using static TEHub.Util;

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

            tSPlayer.SendSuccessMessage("The config was successfully reloaded.");
        }

        public static void DisplayEvents(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
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
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <Event Name>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent hubEvent = HubEvent.GetEvent(eventName);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <Event Name>");
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
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <Event Name>");
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
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoin <Player Name> <Event Name>");
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

        public static void AddEvent(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /addevent <Event Name>");
                return;
            }

            string eventName = args.Parameters[0];

            HubConfig.config.HubEvents.Add(new HubEvent(eventName,
                null,
                0,
                0,
                0, 0,
                0, 0,
                0, 0,
                0, 0));

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
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcejoinall <Event Name>");
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

            bool multipleOngoingVotes = VotingSystem.ongoingVotes.Count() > 1;

            int voteID = 1;

            if (args.Parameters.Count < (multipleOngoingVotes ? 2 : 1) || !int.TryParse(args.Parameters[0], out int optionID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcevote <Option ID>" + (multipleOngoingVotes ? " <Vote ID>" : ""));
                return;
            }

            if (multipleOngoingVotes && !int.TryParse(args.Parameters[1], out voteID))
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /forcevote <Option ID> <Vote ID>");
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
            tSPlayer.SendSuccessMessage(string.Format("You successfully forcevoted for \"{0}\" in \"{1}\"!", optionInfo.option, votingSystem.question));

            TShock.Utils.Broadcast(string.Format("{0} forcevoted and so the vote will conclude early.", tSPlayer.Name), Color.DarkOrange);

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
                        optionInfos.ToArray());

            tSPlayer.SendSuccessMessage("The vote was successfully created!");

            votingSystem.Start();
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
                        60000,
                        hubEvent,
                        new OptionInfo("Yes", hubEvent.StartEventCountdown),
                        new OptionInfo("No"));
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
    
        public static void AddClass(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

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
                eventName = hubEvent.eventName,
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

                if (i <= (int)ItemSlot.InvRow5Slot10)
                {
                    eventClass.items[i] = classItem;
                }
                else if (i <= (int)ItemSlot.CoinSlot4)
                {
                    int index = i - (int)ItemSlot.CoinSlot1;
                    eventClass.coins[index] = classItem;
                }
                else
                {
                    int index = i - (int)ItemSlot.AmmoSlot1;
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

                if (i < (int)InventoryLengths.Armor)
                {
                    eventClass.armor[i] = item.Name;
                }
                else if (i < (int)InventoryLengths.Armor + (int)InventoryLengths.Accessories)
                {
                    int index = i - (int)InventoryLengths.Armor;
                    eventClass.accessories[index] = item.Name;
                }
                else if (i < (int)InventoryLengths.Armor + (int)InventoryLengths.AllAccessories + (int)InventoryLengths.Armor)
                {
                    int index = i - ((int)InventoryLengths.Armor + (int)InventoryLengths.AllAccessories);
                    eventClass.armorVanity[index] = item.Name;
                }
                else
                {
                    int index = i - ((int)InventoryLengths.Armor + (int)InventoryLengths.AllAccessories + (int)InventoryLengths.Armor);
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

                if (i < (int)InventoryLengths.Armor)
                {
                    eventClass.armorDyes[i] = item.Name;
                }
                else
                {
                    int index = i - (int)InventoryLengths.Armor;
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

            ClassConfig.config.eventClasses.Add(eventClass);

            tSPlayer.SendSuccessMessage("The " + className + " class was successfully added!");
        }

        public static void ChooseClass(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;
            Player player = tSPlayer.TPlayer;

            if (!TShock.ServerSideCharacterConfig.Enabled)
            {
                tSPlayer.SendErrorMessage("SSC is not enabled so choosing classes has been disabled.");
                return;
            }

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer.Name);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            IEnumerable<EventClass> eventClasses = ClassConfig.config.eventClasses.Where(c => c.eventName == hubEvent.eventName);

            if (eventClasses.Count() < 1)
            {
                tSPlayer.SendErrorMessage("This event has no classes.");
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

                tSPlayer.SendInfoMessage(string.Format("The available classes for {0} are: {1}.", hubEvent.eventName, classesFormatted));

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

                tSPlayer.SendInfoMessage(string.Format("Try \"/chooseclass [c/FFFF00: {0}] instead.\"", classesFormatted));
                return;
            }

            if (classes.Count() > 1)
            {
                tSPlayer.SendErrorMessage("Multiple classes with that name were found!");
                return;
            }

            EventClass chosenClass = classes.First();

            tSPlayer.ResetPlayer(chosenClass.maxHealth, chosenClass.maxMana);

            // Main items
            for (int i = 0; i < (int)InventoryLengths.Main; i++)
            {
                if (!chosenClass.items.TryGetValue(i, out ClassItem classItem))
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
                if (!chosenClass.coins.TryGetValue(i, out ClassItem classItem))
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
                if (!chosenClass.ammo.TryGetValue(i, out ClassItem classItem))
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
                if (!chosenClass.armor.TryGetValue(i, out string itemName))
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
                if (!chosenClass.accessories.TryGetValue(i, out string itemName))
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
                if (!chosenClass.armorVanity.TryGetValue(i, out string itemName))
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
                if (!chosenClass.accessoryVanity.TryGetValue(i, out string itemName))
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
                if (!chosenClass.armorDyes.TryGetValue(i, out string itemName))
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
                if (!chosenClass.accessoryDyes.TryGetValue(i, out string itemName))
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
                if (!chosenClass.miscEquips.TryGetValue(i, out string itemName))
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
                if (!chosenClass.miscEquipDyes.TryGetValue(i, out string itemName))
                {
                    continue;
                }
                Item item = TShock.Utils.GetItemByIdOrName(itemName).First();

                player.miscDyes[i] = item;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, tSPlayer.Index, (int)ItemSlot.DyeEquipmentSlot1 + i);
            }

            tSPlayer.SendSuccessMessage(string.Format("{0} class chosen!", chosenClass.className));
        }
    }
}
