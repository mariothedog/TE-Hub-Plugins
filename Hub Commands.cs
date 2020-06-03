using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace TEHub
{
    public static class HubCommands
    {
        public static void JoinGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            foreach (List<TSPlayer> eventPlayerList in Variables.playersInEvents.Values)
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
            bool eventExists = Variables.playersInEvents.TryGetValue(eventName, out eventPlayersList);
            if (!eventExists)
            {
                tSPlayer.SendErrorMessage("That event does not exist! The available events are: The Arctic Circle and TBR.");
                return;
            }

            eventPlayersList.Add(tSPlayer);

            string eventNameFormatted = Util.CapitalizeEachWord(string.Join(" ", args.Parameters));
            tSPlayer.SendSuccessMessage(string.Format("You successfully joined {0}!", eventNameFormatted));
        }

        public static void LeaveGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

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
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            eventPlayerInList.Remove(tSPlayer);

            tSPlayer.SendSuccessMessage("You were successfully removed from the event!");
        }
    }
}
