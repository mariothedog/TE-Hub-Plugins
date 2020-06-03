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

            if (HubEvent.GetPlayerEvent(tSPlayer.Name) != null)
            {
                tSPlayer.SendErrorMessage("You're already in an event! Please use /leave first!");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                tSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /join <Arctic Circle/TBR>");
                return;
            }

            string eventName = string.Join("", args.Parameters).ToLower();

            HubEvent.AddPlayerToEvent(tSPlayer, eventName);

            string eventNameFormatted = Util.CapitalizeEachWord(string.Join(" ", args.Parameters));
            tSPlayer.SendSuccessMessage(string.Format("You successfully joined {0}!", eventNameFormatted));
        }

        public static void LeaveGame(CommandArgs args)
        {
            TSPlayer tSPlayer = args.Player;

            HubEvent hubEvent = HubEvent.RemovePlayerFromEvent(tSPlayer);

            if (hubEvent == null)
            {
                tSPlayer.SendErrorMessage("You are not in an event!");
                return;
            }

            tSPlayer.SendSuccessMessage("You were successfully removed from " + hubEvent.eventName + "!");
            return;
        }
    }
}
