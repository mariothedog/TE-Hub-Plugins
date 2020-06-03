using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace TEHub
{
    public class HubEvent
    {
        public static List<HubEvent> eventList = new List<HubEvent>();

        readonly public string eventName;
        readonly public string[] useNames;
        readonly public List<TSPlayer> tSPlayers = new List<TSPlayer>();

        public HubEvent(string eventName, params string[] useNames)
        {
            this.eventName = eventName;
            this.useNames = useNames;

            eventList.Add(this);
        }

        public static HubEvent GetEvent(string name)
        {
            foreach (HubEvent hubEvent in eventList)
            {
                if (hubEvent.useNames.Contains(name))
                {
                    return hubEvent;
                }
            }

            return null;
        }

        public static HubEvent GetPlayerEvent(string playerName)
        {
            foreach (HubEvent hubEvent in eventList)
            {
                if (hubEvent.tSPlayers.Any(tSP => tSP.Name == playerName))
                {
                    return hubEvent;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the player to the event specified.
        /// Returns true if it was successful, and false if otherwise.
        /// </summary>
        /// <param name="tSPlayer"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public static bool AddPlayerToEvent(TSPlayer tSPlayer, string eventName)
        {
            HubEvent hubEvent = GetEvent(eventName);
            if (hubEvent == null)
            {
                return false;
            }

            hubEvent.tSPlayers.Add(tSPlayer);
            return true;
        }

        /// <summary>
        /// Removes the player from the event they are currently line.
        /// Returns the HubEvent that the player was removed from if this was successful, and null if otherwise.
        /// </summary>
        /// <param name="tSPlayer"></param>
        /// <returns></returns>
        public static HubEvent RemovePlayerFromEvent(TSPlayer tSPlayer)
        {
            HubEvent hubEvent = GetPlayerEvent(tSPlayer.Name);
            if (hubEvent == null)
            {
                return null;
            }

            hubEvent.tSPlayers.Remove(tSPlayer);

            hubEvent.tSPlayers.Add(tSPlayer);
            return hubEvent;
        }
    }
}
