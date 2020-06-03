using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Linq;

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
            // Commands
            Commands.ChatCommands.Add(new Command("hub.join", HubCommands.JoinGame, "join"));
            Commands.ChatCommands.Add(new Command("hub.leave", HubCommands.LeaveGame, "leave"));

            // Hooks
            ServerApi.Hooks.ServerLeave.Register(this, HubHooks.OnServerLeave);
            ServerApi.Hooks.GameUpdate.Register(this, HubHooks.OnGameUpdate);

            // Hub Events
            new HubEvent("The Arctic Circle", "arcticcircle", "ac");
            new HubEvent("TBR", "tbr");
        }
		
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, HubHooks.OnServerLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, HubHooks.OnGameUpdate);
            }
        }
    }
}