using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            List<TSPlayer> eventPlayerInList = null;

            foreach (List<TSPlayer> eventPlayerList in Variables.playersInEvents.Values)
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

        public static void OnGameUpdate(EventArgs args)
        {
            // Auto start the game if there are enough players.
            // TODO.
        }
    }
}
