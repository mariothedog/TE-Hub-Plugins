﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TEHub.Configs;
using TEHub.EventClasses;
using TEHub.Extensions;
using TEHub.Teams;
using TEHub.Voting;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace TEHub
{
    public class HubEvent
    {
        [JsonIgnore]
        public bool ongoingCountdown = false;
        [JsonIgnore]
        public bool started = false;
        [JsonIgnore]
        public bool resetPending = false;

        readonly public string eventName;
        readonly public string[] useNames;

        readonly public EventTeam[] teams;

        [JsonIgnore]
        readonly public List<TSPlayer> tSPlayers = new List<TSPlayer>();

        [JsonIgnore]
        public bool canChooseClasses = true;
        [JsonIgnore]
        public Dictionary<string, EventClass> tSPlayersWithAClass = new Dictionary<string, EventClass>();

        readonly public int
            minPlayersForStart,
            teleportPlayersPosX, teleportPlayersPosY,
            originalMapTopLeftPosX, originalMapTopLeftPosY,
            originalMapBottomRightPosX, originalMapBottomRightPosY,
            playableMapTopLeftPosX, playableMapTopLeftPosY;

        readonly private CountdownTimer countdownTimer = new CountdownTimer();

        readonly public double countdownLengthMS;

        readonly public int[] unbreakableBlocks;

        readonly public MapInfo[] maps;

        private double secondsLeftLastBroadcast;

        public HubEvent(string eventName, string[] useNames = null,
            EventTeam[] teams = null,
            int minPlayersForStart = 0, double countdownLengthMS = 0,
            int teleportPlayersPosX = 0, int teleportPlayersPosY = 0,
            int originalMapTopLeftPosX = 0, int originalMapTopLeftPosY = 0,
            int originalMapBottomRightPosX = 0, int originalMapBottomRightPosY = 0,
            int playableMapTopLeftPosX = 0, int playableMapTopLeftPosY = 0,
            int[] unbreakableBlocks = null,
            MapInfo[] maps = null)
        {
            this.eventName = eventName;
            this.useNames = useNames;

            this.teams = teams;

            this.minPlayersForStart = minPlayersForStart;
            this.countdownLengthMS = countdownLengthMS;
            countdownTimer.Interval = countdownLengthMS;
            countdownTimer.Elapsed += (sender, elapsedArgs) => StartEvent();

            this.teleportPlayersPosX = teleportPlayersPosX;
            this.teleportPlayersPosY = teleportPlayersPosY;

            this.originalMapTopLeftPosX = originalMapTopLeftPosX;
            this.originalMapTopLeftPosY = originalMapTopLeftPosY;

            this.originalMapBottomRightPosX = originalMapBottomRightPosX;
            this.originalMapBottomRightPosY = originalMapBottomRightPosY;

            this.playableMapTopLeftPosX = playableMapTopLeftPosX;
            this.playableMapTopLeftPosY = playableMapTopLeftPosY;

            this.unbreakableBlocks = unbreakableBlocks;

            this.maps = maps;
        }

        public void StartEventCountdown()
        {
            ongoingCountdown = true;

            countdownTimer.Start();

            double secondsLeft = Math.Round(countdownTimer.TimeLeft / 1000);
            TShock.Utils.Broadcast("{0} is starting soon! {1} seconds remaining!".SFormat(eventName, secondsLeft), Color.Teal);
        }

        private void StartEvent()
        {
            countdownTimer.Stop();

            started = true;
            ongoingCountdown = false;

            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                // Give classes to those who didn't pick one in time
                if (!tSPlayersWithAClass.Keys.Contains(tSPlayer.Name))
                {
                    EventClass eventClass = Util.GetRandomClass(eventName);
                    tSPlayersWithAClass[tSPlayer.Name] = eventClass;
                    Util.DebugStuff(this);

                    tSPlayer.SendInfoMessage("You were given the random class of {0} as you did not pick one in time.".SFormat(eventClass.className));
                }

                tSPlayer.ForcePvP(true);
            }

            // Assign teams if the event has teams
            if (teams.Length > 1)
            {
                List<TSPlayer> tSPlayersAlreadyInATeam = new List<TSPlayer>();
                foreach (EventTeam eventTeam in teams)
                {
                    foreach (TSPlayer tSPlayer in eventTeam.tSPlayers)
                    {
                        tSPlayersAlreadyInATeam.Add(tSPlayer);

                        TeleportToBase(tSPlayer);
                    }
                }

                List<TSPlayer> tSPlayersWithoutATeam = tSPlayers.Except(tSPlayersAlreadyInATeam).ToList();

                // Note that the minPlayersInTeam variable is an integer so the value of the division will be truncated
                int minPlayersInTeam = tSPlayers.Count() / teams.Length;

                foreach (EventTeam eventTeam in teams)
                {
                    while (eventTeam.tSPlayers.Count() < minPlayersInTeam)
                    {
                        TSPlayer tSPlayer = tSPlayersWithoutATeam[Plugin.UnifiedRandom.Next(tSPlayersWithoutATeam.Count())];
                        eventTeam.AddToTeam(tSPlayer);
                        tSPlayersWithoutATeam.Remove(tSPlayer);

                        TeleportToBase(tSPlayer);
                    }
                }

                foreach (EventTeam eventTeam in teams.OrderBy(k => Plugin.UnifiedRandom.Next()))
                {
                    if (tSPlayersWithoutATeam.Count() == 0)
                    {
                        break;
                    }

                    TSPlayer tSPlayer = tSPlayersWithoutATeam[0];
                    eventTeam.AddToTeam(tSPlayer);
                    tSPlayersWithoutATeam.RemoveAt(0);

                    TeleportToBase(tSPlayer);
                }
            }
            else if (teams.Length == 1)
            {
                EventTeam eventTeam = teams[0];

                foreach (TSPlayer tSPlayer in tSPlayers)
                {
                    if (GetTeamPlayerIn(tSPlayer) == null)
                    {
                        eventTeam.AddToTeam(tSPlayer);
                    }
                }
            }

            foreach (KeyValuePair<string, EventClass> tSPlayerWithAClass in tSPlayersWithAClass)
            {
                TSPlayer tSPlayer = TSPlayer.FindByNameOrID(tSPlayerWithAClass.Key).First(); // This .First() may cause some issues
                Util.GiveClass(tSPlayer, tSPlayerWithAClass.Value);
            }

            canChooseClasses = false;

            TSPlayer.Server.SetTime(true, 0.0);

            // Start map vote
            List<OptionInfo> mapOptions = new List<OptionInfo>();
            foreach (MapInfo mapInfo in maps)
            {
                mapOptions.Add(new OptionInfo(mapInfo.name, () => UnlockMap(mapInfo.name)));
            }

            VotingSystem votingSystem = new VotingSystem("What map should be used?",
                        30000,
                        this,
                        UnlockRandomMap,
                        mapOptions.ToArray());
            votingSystem.Start();
        }

        public void TeleportAllPlayersToSpawn()
        {
            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                TeleportToSpawn(tSPlayer);
            }
        }

        public void TeleportToSpawn(TSPlayer tSPlayer)
        {
            tSPlayer.Teleport(teleportPlayersPosX, teleportPlayersPosY);
        }

        public void UnlockMap(string mapName)
        {
            MapInfo mapInfo = maps.Where(m => m.name == mapName).FirstOrDefault();

            Wiring.HitSwitch(mapInfo.switchTileX, mapInfo.switchTileY);
            NetMessage.SendData((int)PacketTypes.HitSwitch, -1, -1, NetworkText.Empty, mapInfo.switchTileX, mapInfo.switchTileY);

            if (mapInfo.name == "Caverns")
            {
                Wiring.HitSwitch(6586, 422);
                TShock.Utils.Broadcast("The lava has started flowing!", Color.Orange);
            }
        }

        public void UnlockRandomMap()
        {
            TShock.Utils.Broadcast("A random map was chosen!", Color.Aqua);

            int mapIndex = Plugin.UnifiedRandom.Next(maps.Length);
            MapInfo map = maps[mapIndex];
            string mapName = map.name;

            UnlockMap(mapName);
        }

        /// <summary>
        /// Teleports the tSPlayer to the team's base. If team is null, the player will be teleported to the base of the team they reside in.
        /// </summary>
        /// <param name="tSPlayer"></param>
        /// <param name="team"></param>
        /// <returns>True if the teleportation was successful, false if otherwise.</returns>
        public bool TeleportToBase(TSPlayer tSPlayer, EventTeam team = null)
        {
            if (team == null)
            {
                EventTeam eventTeam = GetTeamPlayerIn(tSPlayer);

                if (eventTeam == null)
                {
                    return false;
                }

                return tSPlayer.Teleport(eventTeam.basePosX, eventTeam.basePosY); ;
            }

            return tSPlayer.Teleport(team.basePosX, team.basePosY);
        }

        public void GameUpdate()
        {
            if (ongoingCountdown)
            {
                CountdownUpdate();
                return;
            }
            else if (started)
            {
                MainUpdate();
            }
        }

        /// <summary>
        /// Game updates that occur while the countdown is still active.
        /// </summary>
        private void CountdownUpdate()
        {
            double secondsLeft = Math.Round(countdownTimer.TimeLeft / 1000);

            if (secondsLeftLastBroadcast - secondsLeft >= 1 && (secondsLeft < 10 || secondsLeft % 10 == 0))
            {
                if (secondsLeft == 0)
                {
                    TShock.Utils.Broadcast("{0} is starting now!".SFormat(eventName), Color.Orange);
                }
                else
                {
                    TShock.Utils.Broadcast("{0} is starting soon! {1} seconds remaining!".SFormat(eventName, secondsLeft), Color.Teal);
                }
            }

            secondsLeftLastBroadcast = secondsLeft;
        }

        /// <summary>
        /// Game updates that occur while the event has actually started.
        /// </summary>
        private void MainUpdate()
        {
            // TODO
        }

        public bool ResetMap()
        {
            if (!Commands.ChatCommands.Any(c => c.HasAlias("/point1")))
            {
                return false;
            }

            Commands.HandleCommand(TSPlayer.Server, "//point1 {0} {1}".SFormat(originalMapTopLeftPosX, originalMapTopLeftPosY));
            Commands.HandleCommand(TSPlayer.Server, "//point2 {0} {1}".SFormat(originalMapBottomRightPosX, originalMapBottomRightPosY));
            Commands.HandleCommand(TSPlayer.Server, "//copy");
            Commands.HandleCommand(TSPlayer.Server, "//point1 {0} {1}".SFormat(playableMapTopLeftPosX, playableMapTopLeftPosY));
            Commands.HandleCommand(TSPlayer.Server, "//paste");

            return true;
        }

        public static HubEvent GetEvent(string name)
        {
            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
            {
                if (hubEvent.useNames.Contains(name))
                {
                    return hubEvent;
                }
            }

            return null;
        }

        public static HubEvent GetEventPlayerIn(TSPlayer tSPlayer)
        {
            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
            {
                if (hubEvent.tSPlayers.Any(tSP => tSP.Name == tSPlayer.Name))
                {
                    return hubEvent;
                }
            }

            return null;
        }

        public EventTeam GetTeam(string teamName)
        {
            foreach (EventTeam eventTeam in teams)
            {
                if (eventTeam.useNames.Contains(teamName))
                {
                    return eventTeam;
                }
            }

            return null;
        }

        public EventTeam GetTeamPlayerIn(TSPlayer tSPlayer)
        {
            foreach (EventTeam eventTeam in teams)
            {
                if (eventTeam.tSPlayers.Any(tSP => tSP.Name == tSPlayer.Name))
                {
                    return eventTeam;
                }
            }

            return null;
        }

        public void ResetEvent()
        {
            resetPending = false;
            started = false;

            // Remove all players who have left the server
            tSPlayers.RemoveAll(tSP => tSP == null || !tSP.Active);

            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                if (tSPlayer.Dead)
                {
                    continue;
                }

                tSPlayer.ResetPlayer();
            }

            tSPlayersWithAClass.Clear();
            canChooseClasses = true;

            teams.ForEach(t => t.KickAll());

            VotingSystem.ongoingVotes.FindAll(v => v.linkedEvent == this).ForEach(v => v.Cancel());

            TeleportAllPlayersToSpawn();

            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                tSPlayer.ForcePvP(false);
                tSPlayer.SendInfoMessage("Your PvP has been forced off.");
            }

            for (int i = 0; i < Main.maxItems; i++) // Clear dropped items - Credit to the TShock Clear command
            {
                if (Main.item[i].active)
                {
                    Main.item[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                }
            }

            for (int i = 0; i < Main.maxProjectiles; i++) // Clear all projectiles
            {
                if (Main.projectile[i].active)
                {
                    Main.projectile[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", i);
                }
            }

            if (!ResetMap())
            {
                TShock.Log.ConsoleError("The ResetMap method was used but the WorldEdit plugin was not found!");
            }
        }

        public static List<HubEvent> GetOngoingEvents()
        {
            return HubConfig.config.HubEvents.FindAll(hubEvent => hubEvent.started || hubEvent.ongoingCountdown);
        }
    }
}
