using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using TerrariaApi.Server;

namespace TEHub
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        readonly Dictionary<string, List<TSPlayer>> playersInEvents = new Dictionary<string, List<TSPlayer>>()
        {
            { "arcticcircle", new List<TSPlayer>() },
            { "tbr", new List<TSPlayer>() }
        };

        public override string Author => "Terrævents";
        public override string Name => "Terrævents Hub";
        public override string Description => "A variety of plugins for the Terrævents Hub.";
        public override Version Version => new Version(1, 0, 0, 0);
		
        public Plugin(Main game) : base (game)
        {
        } 
		
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("hub.join", JoinGame, "join"));
            Commands.ChatCommands.Add(new Command("hub.leave", LeaveGame, "leave"));

            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
        }
		
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
            }
        }

        private void JoinGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            foreach (List<TSPlayer> eventPlayerList in playersInEvents.Values)
            {
                bool alreadyInEvent = eventPlayerList.Any(tSP => tSP.Name == tSPlayer.Name);
                if (alreadyInEvent)
                {
                    tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <Arctic Circle/TBR>");
                    return;
                }
            }

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <Arctic Circle/TBR>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            List<TSPlayer> eventPlayersList = new List<TSPlayer>();
            bool eventExists = playersInEvents.TryGetValue(eventName, out eventPlayersList);
            if (!eventExists)
            {
                tSPlayer.SendErrorMessage("That event does not exist! The available events are: The Arctic Circle and TBR.");
                return;
            }

            eventPlayersList.Add(tSPlayer);

            string eventNameFormatted = Util.CapitalizeEachWord(string.Join(" ", args.Parameters));
            tSPlayer.SendSuccessMessage(string.Format("You successfully joined {0}!", eventNameFormatted));
        }

        private void LeaveGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            List<TSPlayer> eventPlayerInList = null;

            foreach (List<TSPlayer> eventPlayerList in playersInEvents.Values)
            {
                bool inEvent = eventPlayerList.Any(tSP => tSP.Name == tSPlayer.Name);
                if (inEvent)
                {
                    eventPlayerInList = eventPlayerList;
                }
            }

            if (eventPlayerInList == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            eventPlayerInList.Remove(tSPlayer);

            tSPlayer.SendSuccessMessage("You were successfully removed from the event!");
        }
        
        // Kick players out of events if they leave the game
        private void OnServerLeave(LeaveEventArgs args)
        {
            TSPlayer tSPlayer = TShock.Players[args.Who];

            List<TSPlayer> eventPlayerInList = null;

            foreach (List<TSPlayer> eventPlayerList in playersInEvents.Values)
            {
                bool inEvent = eventPlayerList.Any(tSP => tSP.Name == tSPlayer.Name);
                if (inEvent)
                {
                    eventPlayerInList = eventPlayerList;
                }
            }

            if (eventPlayerInList == null)
            {
                return;
            }

            eventPlayerInList.Remove(tSPlayer);
        }

        private void OnGameUpdate(EventArgs args)
        {
            /*foreach (KeyValuePair<string, List<TSPlayer>> playersInEvent in playersInEvents)
            {
                Console.WriteLine(playersInEvent.Key);

                foreach (TSPlayer tSP in playersInEvent.Value)
                {
                    Console.WriteLine(tSP.Name);
                }

                Console.WriteLine(" ");
            }*/
        }
    }
}