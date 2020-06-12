using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TEHub.Configs;
using TShockAPI;

namespace TEHub
{
    public class HubEvent
    {
        private bool ongoingCountdown = false;
        private bool started = false;
        private bool declinedStart = false;

        readonly public string eventName;
        readonly public string[] useNames;

        [JsonIgnore]
        readonly public List<TSPlayer> tSPlayers = new List<TSPlayer>();

        readonly public int
            minPlayersForStart,
            teleportPlayersPosX, teleportPlayersPosY,
            originalMapTopLeftPosX, originalMapTopLeftPosY,
            originalMapBottomRightPosX, originalMapBottomRightPosY,
            playableMapTopLeftPosX, playableMapTopLeftPosY;

        readonly private CountdownTimer countdownTimer = new CountdownTimer();

        readonly public double countdownLengthMS;

        public HubEvent(string eventName, string[] useNames,
            int minPlayersForStart, double countdownLengthMS,
            int teleportPlayersPosX, int teleportPlayersPosY,
            int originalMapTopLeftPosX, int originalMapTopLeftPosY,
            int originalMapBottomRightPosX, int originalMapBottomRightPosY,
            int playableMapTopLeftPosX, int playableMapTopLeftPosY)
        {
            this.eventName = eventName;
            this.useNames = useNames;

            this.minPlayersForStart = minPlayersForStart;
            this.countdownLengthMS = countdownLengthMS;
            countdownTimer.Interval = countdownLengthMS;
            countdownTimer.Elapsed += (sender, elapsedArgs) => StartEvent(sender, elapsedArgs);

            this.teleportPlayersPosX = teleportPlayersPosX;
            this.teleportPlayersPosY = teleportPlayersPosY;

            this.originalMapTopLeftPosX = originalMapTopLeftPosX;
            this.originalMapTopLeftPosY = originalMapTopLeftPosY;

            this.originalMapBottomRightPosX = originalMapBottomRightPosX;
            this.originalMapBottomRightPosY = originalMapBottomRightPosY;

            this.playableMapTopLeftPosX = playableMapTopLeftPosX;
            this.playableMapTopLeftPosY = playableMapTopLeftPosY;
        }

        public void StartEventCountdown()
        {
            ongoingCountdown = true;

            countdownTimer.Start();

            double secondsLeft = Math.Round(countdownTimer.TimeLeft / 1000);
            TShock.Utils.Broadcast(string.Format("{0} is starting soon! {1} seconds remaining!", eventName, secondsLeft), Color.Teal);
        }

        public void DeclineStart()
        {
            declinedStart = true;
        }

        private void StartEvent(object sender, ElapsedEventArgs elapsedArgs)
        {
            countdownTimer.Stop();

            started = true;
            ongoingCountdown = false;
        }

        public void TeleportPlayerToSpawn(TSPlayer tSPlayer)
        {
            tSPlayer.Teleport(teleportPlayersPosX, teleportPlayersPosY);
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

        private double secondsLeftLastBroadcast;

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
                    TShock.Utils.Broadcast(string.Format("{0} is starting now!", eventName), Color.Orange);
                }
                else
                {
                    TShock.Utils.Broadcast(string.Format("{0} is starting soon! {1} seconds remaining!", eventName, secondsLeft), Color.Teal);
                }
            }

            secondsLeftLastBroadcast = secondsLeft;
        }

        /// <summary>
        /// Game updates that occur while the event has started.
        /// </summary>
        private void MainUpdate()
        {
            Console.WriteLine("Main Update");
        }

        public bool ResetMap()
        {
            if (!Commands.ChatCommands.Any(c => c.HasAlias("/point1")))
            {
                return false;
            }

            Commands.HandleCommand(TSPlayer.Server, string.Format("//point1 {0} {1}", originalMapTopLeftPosX, originalMapTopLeftPosY));
            Commands.HandleCommand(TSPlayer.Server, string.Format("//point2 {0} {1}", originalMapBottomRightPosX, originalMapBottomRightPosY));
            Commands.HandleCommand(TSPlayer.Server, "//copy");
            Commands.HandleCommand(TSPlayer.Server, string.Format("//point1 {0} {1}", playableMapTopLeftPosX, playableMapTopLeftPosY));
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

        public static HubEvent GetEventPlayerIn(string playerName)
        {
            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
            {
                if (hubEvent.tSPlayers.Any(tSP => tSP.Name == playerName))
                {
                    return hubEvent;
                }
            }

            return null;
        }

        public static HubEvent GetOngoingEvent()
        {
            foreach (HubEvent hubEvent in HubConfig.config.HubEvents)
            {
                if (hubEvent.started || hubEvent.ongoingCountdown || hubEvent.declinedStart)
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
