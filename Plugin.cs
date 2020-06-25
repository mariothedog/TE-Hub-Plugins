using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.IO;
using System.Collections.Generic;
using TEHub.Configs;
using Terraria.Utilities;

namespace TEHub
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public static readonly UnifiedRandom UnifiedRandom = new UnifiedRandom();

        public static List<NetItem> StarterItems = new List<NetItem>();
        public static int startHealth, startMana;

        public override string Author => "Terrævents";
        public override string Name => "Terrævents Hub";
        public override string Description => "A variety of plugins for the Terrævents Hub.";
        public override Version Version => new Version(1, 0, 0, 0);
		
        public Plugin(Main game) : base (game)
        {

        }
		
        public override void Initialize()
        {
            AddCommands();

            AddHooks();

            SetSSCDefaults();

            // Config
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
        }

        private void AddCommands()
        {
            // Help
            Commands.ChatCommands.Add(new Command("hub.player.help", HubCommands.HubHelp, "hubhelp") { HelpText = "Returns a list of all the TE Hub commands." });

            // Config
            Commands.ChatCommands.Add(new Command("hub.admin.config.read", HubCommands.ReadConfig, "readconfig") { HelpText = "Reads the config." });
            Commands.ChatCommands.Add(new Command("hub.admin.config.write", HubCommands.WriteConfig, "writeconfig") { HelpText = "Writes to the config." });

            // Join/Leave
            Commands.ChatCommands.Add(new Command("hub.player.event.join", HubCommands.JoinEvent, "join") { HelpText = "Join the event specified." });
            Commands.ChatCommands.Add(new Command("hub.player.event.leave", HubCommands.LeaveEvent, "leave") { HelpText = "Leave the event you are currently in." });
            Commands.ChatCommands.Add(new Command("hub.admin.event.force.join", HubCommands.ForceJoin, "forcejoin") { HelpText = "Force a player to join an event." });
            Commands.ChatCommands.Add(new Command("hub.admin.event.force.leave", HubCommands.ForceLeave, "forceleave") { HelpText = "Force a player to leave the event they are currently in." });

            // Spectate
            Commands.ChatCommands.Add(new Command("hub.player.spectate", HubCommands.SpectatePlayer, "spectate") { HelpText = "Allows you to a spectate a player specified." });

            // Voting
            Commands.ChatCommands.Add(new Command("hub.admin.vote.create", HubCommands.CreateVote, "createvote") { HelpText = "Creates a poll." });
            Commands.ChatCommands.Add(new Command("hub.admin.vote.end", HubCommands.EndVote, "endvote") { HelpText = "Allows you to forcibly end a poll." });
            Commands.ChatCommands.Add(new Command("hub.player.vote.participate", HubCommands.Vote, "vote") { HelpText = "Allows you to vote in a poll." });
            
            // Events
            Commands.ChatCommands.Add(new Command("hub.player.event.display", HubCommands.DisplayEvents, "displayevents") { HelpText = "Returns a list of the events and the players who have joined." });
            Commands.ChatCommands.Add(new Command("hub.admin.event.add", HubCommands.AddEvent, "addevent") { HelpText = "Adds an event." });
            Commands.ChatCommands.Add(new Command("hub.admin.event.start", HubCommands.ForceStartEvent, "start") { HelpText = "Forcibly starts an event." });

            // Rounds
            Commands.ChatCommands.Add(new Command("hub.admin.event.end", HubCommands.EndRound, "endround") { HelpText = "Ends the current round." });

            // Classes
            Commands.ChatCommands.Add(new Command("hub.admin.classes.add", HubCommands.AddClass, "addclass") { HelpText = "Adds a new class." });
            Commands.ChatCommands.Add(new Command("hub.player.classes.choose", HubCommands.ChooseClass, "chooseclass", "class", "cc") { HelpText = "Allows you to choose a class." });

            // Other
            Commands.ChatCommands.Add(new Command("hub.admin.util.getpos", HubCommands.GetPos, "getpos") { HelpText = "Returns your current position." });
            Commands.ChatCommands.Add(new Command("hub.admin.util.resetmap", HubCommands.ResetMap, "resetmap") { HelpText = "Resets the map of the event specified back to its original state." });
        }

        private void AddHooks()
        {
            // ServerApi
            ServerApi.Hooks.GameUpdate.Register(this, HubHooks.OnGameUpdate);

            // GetDataHandlers
            GetDataHandlers.PlayerTeam += HubHooks.OnPlayerTeamChange;
            GetDataHandlers.TogglePvp += HubHooks.OnTogglePvp;
            GetDataHandlers.TileEdit += HubHooks.OnTileEdit;
            GetDataHandlers.ItemDrop += HubHooks.OnItemDrop;
            GetDataHandlers.ChestItemChange += HubHooks.OnChestItemChange;
            GetDataHandlers.PlayerSpawn += HubHooks.OnPlayerSpawn;
            GetDataHandlers.PlayerSlot += HubHooks.OnPlayerSlot;
        }

        private void SetSSCDefaults()
        {
            if (Main.ServerSideCharacter)
            {
                StarterItems = TShock.ServerSideCharacterConfig.StartingInventory;

                startHealth = Math.Max(Math.Min(TShock.ServerSideCharacterConfig.StartingHealth, 500), 100);

                startMana = Math.Max(Math.Min(TShock.ServerSideCharacterConfig.StartingMana, 200), 20);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Hooks
                // ServerApi
                ServerApi.Hooks.GameUpdate.Deregister(this, HubHooks.OnGameUpdate);
                // GetDataHandlers
                GetDataHandlers.PlayerTeam -= HubHooks.OnPlayerTeamChange;
                GetDataHandlers.TogglePvp -= HubHooks.OnTogglePvp;
                GetDataHandlers.TileEdit -= HubHooks.OnTileEdit;
                GetDataHandlers.ItemDrop -= HubHooks.OnItemDrop;
                GetDataHandlers.ChestItemChange -= HubHooks.OnChestItemChange;

                // Configs
                HubConfig.config.Write(HubConfig.configPath);
                ClassConfig.config.Write(ClassConfig.configPath);
            }
        }
    }
}