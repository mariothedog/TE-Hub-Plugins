using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.IO;
using System.Collections.Generic;
using TEHub.Configs;

namespace TEHub
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
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
            Commands.ChatCommands.Add(new Command("hub.help", HubCommands.HubHelp, "hubhelp") { HelpText = "Returns a list of all the TE Hub commands." });

            // Config
            Commands.ChatCommands.Add(new Command("hub.admin.reload.config", HubCommands.ReloadConfig, "reloadconfig") { HelpText = "Reload the config." });

            // Join/Leave
            Commands.ChatCommands.Add(new Command("hub.join", HubCommands.JoinGame, "join") { HelpText = "Join the event specified.." });
            Commands.ChatCommands.Add(new Command("hub.leave", HubCommands.LeaveGame, "leave") { HelpText = "Leave the event you are currently in." });
            Commands.ChatCommands.Add(new Command("hub.admin.forcejoin", HubCommands.ForceJoin, "forcejoin") { HelpText = "Force a player to join an event." });
            Commands.ChatCommands.Add(new Command("hub.admin.forcejoin.all", HubCommands.ForceJoinAll, "forcejoinall") { HelpText = "Force everyone to join an event." });

            // Spectate
            Commands.ChatCommands.Add(new Command("hub.spectate", HubCommands.SpectatePlayer, "spectate") { HelpText = "Allows you to a spectate a player specified." });
            Commands.ChatCommands.Add(new Command("hub.spectate.stop", HubCommands.StopSpectating, "stopspectating") { HelpText = "Allows you to stop spectating." });

            // Voting
            Commands.ChatCommands.Add(new Command("hub.admin.createvote", HubCommands.CreateVote, "createvote") { HelpText = "Creates a poll." });
            Commands.ChatCommands.Add(new Command("hub.vote", HubCommands.Vote, "vote") { HelpText = "Allows you to vote in a poll." });
            
            // Events
            Commands.ChatCommands.Add(new Command("hub.displayevents", HubCommands.DisplayEvents, "displayevents") { HelpText = "Returns a list of the events and the players who have joined." });
            Commands.ChatCommands.Add(new Command("hub.admin.addevent", HubCommands.AddEvent, "addevent") { HelpText = "Adds an event." });
            Commands.ChatCommands.Add(new Command("hub.startevent", HubCommands.StartGame, "start") { HelpText = "Allows you to create a poll to start the event you are currently in." });
            Commands.ChatCommands.Add(new Command("hub.admin.forcestartevent", HubCommands.ForceStartGame, "forcestart") { HelpText = "Forcibly starts an event." });

            // Classes
            Commands.ChatCommands.Add(new Command("hub.admin.classes.add", HubCommands.AddClass, "addclass") { HelpText = "Adds a new class." });
            Commands.ChatCommands.Add(new Command("hub.classes.choose", HubCommands.ChooseClass, "chooseclass", "class", "cc") { HelpText = "Allows you to choose a class." });

            // Other
            Commands.ChatCommands.Add(new Command("hub.admin.getpos", HubCommands.GetPos, "getpos") { HelpText = "Returns your position." });
            Commands.ChatCommands.Add(new Command("hub.admin.resetmap", HubCommands.ResetMap, "resetmap") { HelpText = "Resets the map of the event specified back to its original state." });
        }

        private void AddHooks()
        {
            ServerApi.Hooks.ServerLeave.Register(this, HubHooks.OnServerLeave);
            ServerApi.Hooks.GameUpdate.Register(this, HubHooks.OnGameUpdate);
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
                ServerApi.Hooks.ServerLeave.Deregister(this, HubHooks.OnServerLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, HubHooks.OnGameUpdate);

                // Configs
                HubConfig.config.Write(HubConfig.configPath);

                ClassConfig.config.Write(ClassConfig.configPath);
            }
        }
    }
}