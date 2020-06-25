using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TShockAPI;
using static TEHub.Util;

namespace TEHub.Teams
{
    public class EventTeam
    {
        public string name;
        public string[] useNames;
        public string teamColor;

        public int basePosX;
        public int basePosY;

        [JsonIgnore]
        public List<TSPlayer> tSPlayers = new List<TSPlayer>();

        public void AddToTeam(TSPlayer tSPlayer)
        {
            tSPlayers.Add(tSPlayer);

            SetInGameTeam(tSPlayer);
        }

        public void SetInGameTeam(TSPlayer tSPlayer)
        {
            TeamColors team = (TeamColors)Enum.Parse(typeof(TeamColors), teamColor);
            tSPlayer.SetTeam((int)team);

            tSPlayer.SendSuccessMessage(string.Format("You were placed in the {0} team!", team));
        }

        public void KickAll()
        {
            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                tSPlayer.SetTeam((int)TeamColors.White);
                tSPlayer.SendSuccessMessage("Your team was reset.");
            }

            tSPlayers.Clear();
        }
    }
}
