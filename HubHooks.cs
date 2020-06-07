using Microsoft.Xna.Framework;
using System;
using TEHub.Extensions;
using TEHub.Voting;
using TerrariaApi.Server;
using TShockAPI;

namespace TEHub
{
    public static class HubHooks
    {
        public static void OnServerLeave(LeaveEventArgs args)
        {
            // Kick players out of events if they leave the game.

            TSPlayer tSPlayer = TShock.Players[args.Who];

            HubEvent hubEvent = HubEvent.GetEventPlayerIn(tSPlayer.Name);

            if (hubEvent == null)
            {
                return;
            }

            HubEvent.RemovePlayerFromEvent(tSPlayer, hubEvent);

            Util.spectatingPlayersToTargets.Remove(tSPlayer);
        }

        public static void OnGameUpdate(EventArgs _)
        {
            // Teleport spectating players to their targets.
            foreach (TSPlayer tSPlayer in Util.spectatingPlayersToTargets.Keys)
            {
                TSPlayer target = Util.spectatingPlayersToTargets[tSPlayer];
                Vector2 position = target.TPlayer.position;
                tSPlayer.TeleportNoDust(position);
            }

            // Check if an event has already started.
            HubEvent startedEvent = HubEvent.GetOngoingEvent();
            if (startedEvent != null)
            {
                startedEvent.GameUpdate();
                return;
            }

            // Check if there is already an ongoing vote.
            if (VotingSystem.ongoingVotes.Count > 0)
            {
                return;
            }

            // Start the event countdown vote if there are enough players.
            foreach (HubEvent hubEvent in Config.config.HubEvents)
            {
                if (hubEvent.tSPlayers.Count >= hubEvent.minPlayersForStart)
                {
                    Console.WriteLine("EEEEEEEEEEEEEEEEEEEEE EVENT STARTED!!!");

                    VotingSystem votingSystem = new VotingSystem("Should the game start?",
                        "The vote has tied and will now be restarted.",
                        60000,
                        hubEvent,
                        new OptionInfo("Yes", "Yes wins.", hubEvent.StartEventCountdown),
                        new OptionInfo("No", "No wins.", hubEvent.DeclineStart));
                    votingSystem.Start();
                }
            }
        }
    }
}
