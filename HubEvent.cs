﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TShockAPI;

namespace TEHub
{
    public class HubEvent
    {
        public static List<HubEvent> eventList = new List<HubEvent>();

        private bool ongoingCountdown = false;
        private bool started = false;

        // Note that public fields will be added to the config.

        readonly public string eventName;
        readonly public string[] useNames;
        readonly public List<TSPlayer> tSPlayers = new List<TSPlayer>();

        readonly public int minPlayersForStart;

        readonly public int teleportPlayersPosX;
        readonly public int teleportPlayersPosY;

        readonly private EventCountdownTimer countdownTimer = new EventCountdownTimer();

        public HubEvent(string eventName, int minPlayersForStart, double countdownLengthMS, int teleportPlayersPosX, int teleportPlayersPosY, params string[] useNames)
        {
            this.eventName = eventName;
            this.useNames = useNames;
            this.minPlayersForStart = minPlayersForStart;
            countdownTimer.Interval = countdownLengthMS;
            countdownTimer.Elapsed += (sender, elapsedArgs) => StartEvent(sender, elapsedArgs);
            this.teleportPlayersPosX = teleportPlayersPosX;
            this.teleportPlayersPosY = teleportPlayersPosY;

            eventList.Add(this);
        }

        public void StartEventCountdown()
        {
            ongoingCountdown = true;

            countdownTimer.Start();

            double secondsLeft = Math.Round(countdownTimer.TimeLeft / 1000);
            TShock.Utils.Broadcast(string.Format("{0} is starting soon! {1} seconds remaining!", eventName, secondsLeft), Color.Orange);
        }

        private void StartEvent(object sender, ElapsedEventArgs elapsedArgs)
        {
            started = true;
            ongoingCountdown = false;

            foreach (TSPlayer tSPlayer in tSPlayers)
            {
                tSPlayer.Teleport(teleportPlayersPosX, teleportPlayersPosY);

                // Remove player from the waiting list.
                tSPlayers.Remove(tSPlayer);
            }
        }

        public void GameUpdate()
        {
            if (ongoingCountdown)
            {
                CountdownUpdate();
                return;
            }

            MainUpdate();
        }

        double secondsLeftLastBroadcast;

        /// <summary>
        /// Game updates that occur while the countdown is still active.
        /// </summary>
        private void CountdownUpdate()
        {
            double secondsLeft = Math.Round(countdownTimer.TimeLeft / 1000);

            if (secondsLeftLastBroadcast - secondsLeft >= 1 && (secondsLeft < 10 || secondsLeft % 10 == 0))
            {
                TShock.Utils.Broadcast(string.Format("{0} is starting soon! {1} seconds remaining!", eventName, secondsLeft), Color.Orange);
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

        public static HubEvent GetOngoingEvent()
        {
            foreach (HubEvent hubEvent in eventList)
            {
                if (hubEvent.started || hubEvent.ongoingCountdown)
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
