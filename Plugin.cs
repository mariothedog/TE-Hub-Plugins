using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.IO;

namespace TEHub
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
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

            // Config
            Config.config = Config.Read(Config.configPath);
            if (!File.Exists(Config.configPath))
            {
                Config.config.Write(Config.configPath);
            }

            // Hub Events
            new HubEvent("The Arctic Circle", 6, 0, 0, "thearcticcircle", "arcticcircle", "theac", "ac");
            new HubEvent("TBR", 6, 0, 0,  "tbr");
        }

        private void AddCommands()
        {
            Commands.ChatCommands.Add(new Command("hub.help", HubCommands.HubHelp, "hubhelp") { HelpText = "Return a list of all the TE Hub commands." });

            Commands.ChatCommands.Add(new Command("hub.join", HubCommands.JoinGame, "join") { HelpText = "Join an event." });
            Commands.ChatCommands.Add(new Command("hub.leave", HubCommands.LeaveGame, "leave") { HelpText = "Leave an event." });

            Commands.ChatCommands.Add(new Command("hub.admin.forcejoinall", HubCommands.ForceJoinAll, "forcejoinall") { HelpText = "Force everyone to join an event." });
            Commands.ChatCommands.Add(new Command("hub.admin.forcejoin", HubCommands.ForceJoin, "forcejoin") { HelpText = "Force a player to join an event." });

            Commands.ChatCommands.Add(new Command("hub.admin.reload.config", HubCommands.ReloadConfig, "reloadconfig") { HelpText = "Write to and reload the config." });
        }

        private void AddHooks()
        {
            ServerApi.Hooks.ServerLeave.Register(this, HubHooks.OnServerLeave);
            ServerApi.Hooks.GameUpdate.Register(this, HubHooks.OnGameUpdate);
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