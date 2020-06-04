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

        readonly public int minPlayersForStart;

        readonly public int teleportPlayersPosX;
        readonly public int teleportPlayersPosY;

        public HubEvent(string eventName, int minPlayersForStart, int teleportPlayersPosX, int teleportPlayersPosY, params string[] useNames)
        {
            this.eventName = eventName;
            this.useNames = useNames;
            this.minPlayersForStart = minPlayersForStart;
            this.teleportPlayersPosX = teleportPlayersPosX;
            this.teleportPlayersPosY = teleportPlayersPosY;

            eventList.Add(this);
        }

        public void StartEvent()
        {
            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                tSPlayer.Teleport(teleportPlayersPosX, teleportPlayersPosY);
            }
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

        public static HubEvent GetEventPlayerIn(string playerName)
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

        public static void AddPlayerToEvent(TSPlayer tSPlayer, HubEvent hubEvent)
        {
            hubEvent.tSPlayers.Add(tSPlayer);
        }

        public static void RemovePlayerFromEvent(TSPlayer tSPlayer, HubEvent hubEvent)
        {
            hubEvent.tSPlayers.Remove(tSPlayer);
        }
    }
}
