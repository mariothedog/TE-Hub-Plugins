using System;
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

            HubEvent.RemovePlayerFromEvent(tSPlayer, hubEvent);
        }

        public static void OnGameUpdate(EventArgs args)
        {
            // Check if an event has already started.
            HubEvent startedEvent = HubEvent.GetOngoingEvent();
            if (startedEvent != null)
            {
                startedEvent.GameUpdate();
                return;
            }

            // Start the event countdown if there are enough players.
            foreach (HubEvent hubEvent in HubEvent.eventList)
            {
                if (hubEvent.tSPlayers.Count >= hubEvent.minPlayersForStart)
                {
                    hubEvent.StartEventCountdown();
                }
            }
        }
    }
}
