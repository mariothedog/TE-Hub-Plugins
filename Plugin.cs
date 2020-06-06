using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.IO;
using System.Collections.Generic;

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
            Config.config = Config.Read(Config.configPath);
            if (!File.Exists(Config.configPath))
            {
                Config.config.Write(Config.configPath);
            }
        }

        private void AddCommands()
        {
            // General
            Commands.ChatCommands.Add(new Command("hub.help", HubCommands.HubHelp, "hubhelp") { HelpText = "Returns a list of all the TE Hub commands." });

            Commands.ChatCommands.Add(new Command("hub.displayevents", HubCommands.DisplayEvents, "displayevents") { HelpText = "Returns a list of the events and the players who have joined." });

            Commands.ChatCommands.Add(new Command("hub.join", HubCommands.JoinGame, "join") { HelpText = "Join an event." });
            Commands.ChatCommands.Add(new Command("hub.leave", HubCommands.LeaveGame, "leave") { HelpText = "Leave an event." });

            Commands.ChatCommands.Add(new Command("hub.spectate", HubCommands.SpectatePlayer, "spectate") { HelpText = "Allows you to a spectate a player specified." });
            Commands.ChatCommands.Add(new Command("hub.spectate.stop", HubCommands.StopSpectating, "stopspectating") { HelpText = "Allows you to stop spectating." });

            // Admin
            Commands.ChatCommands.Add(new Command("hub.admin.reload.config", HubCommands.ReloadConfig, "reloadconfig") { HelpText = "Reload the config." });

            Commands.ChatCommands.Add(new Command("hub.admin.forcejoin.all", HubCommands.ForceJoinAll, "forcejoinall") { HelpText = "Force everyone to join an event." });
            Commands.ChatCommands.Add(new Command("hub.admin.forcejoin", HubCommands.ForceJoin, "forcejoin") { HelpText = "Force a player to join an event." });

            Commands.ChatCommands.Add(new Command("hub.admin.getpos", HubCommands.GetPos, "getpos") { HelpText = "Returns your position." });

            Commands.ChatCommands.Add(new Command("hub.admin.addevent", HubCommands.AddEvent, "addevent") { HelpText = "Adds an event." });

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
                ServerApi.Hooks.ServerLeave.Deregister(this, HubHooks.OnServerLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, HubHooks.OnGameUpdate);

                Config.config.Write(Config.configPath);
            }
        }
    }
}