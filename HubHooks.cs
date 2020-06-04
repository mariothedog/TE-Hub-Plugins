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
            // Auto start the game if there are enough players.
            // TODO.
        }
    }
}
