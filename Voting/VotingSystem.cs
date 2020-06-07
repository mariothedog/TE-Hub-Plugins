using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TEHub.Voting;
using TShockAPI;

namespace TEHub
{
    public class VotingSystem
    {
        public static List<VotingSystem> ongoingVotes = new List<VotingSystem>();

        readonly public string question;
        readonly public string tieMessage;
        readonly public double voteLengthMS;
        readonly public HubEvent linkedEvent;
        readonly public OptionInfo[] options;
        readonly public List<TSPlayer> voters = new List<TSPlayer>();

        readonly private Timer voteTimer = new Timer();

        public VotingSystem(string question, string tieMessage, double voteLengthMS, HubEvent linkedEvent, params OptionInfo[] options)
        {
            this.question = question;

            this.tieMessage = tieMessage;

            this.linkedEvent = linkedEvent;

            this.voteLengthMS = voteLengthMS;
            voteTimer.Interval = voteLengthMS;
            voteTimer.Elapsed += (sender, elapsedArgs) => Stop();

            this.options = options;
        }

        public void Start()
        {
            ongoingVotes.Add(this);

            string optionsFormatted = "";
            for (int i = 0; i < options.Length; i++)
            {
                optionsFormatted += string.Format("{0} - {1}\n", i + 1, options[i].option);
            }
            optionsFormatted = optionsFormatted.Trim();

            double voteLengthSeconds = Math.Round(voteLengthMS / 1000);

            TShock.Utils.Broadcast(string.Format("A vote has started! {0} seconds remaining!", voteLengthSeconds), Color.Orange);
            TShock.Utils.Broadcast("Please use /vote to participate!", Color.Orange);
            TShock.Utils.Broadcast(question, Color.Orange);
            TShock.Utils.Broadcast(optionsFormatted, Color.Aqua);

            voteTimer.Start();
        }

        public void Stop()
        {
            voteTimer.Stop();

            ongoingVotes.Remove(this);

            IEnumerable<OptionInfo> mostVotedOptions = options.Where(a => a.votes == options.Max(a2 => a2.votes));

            if (mostVotedOptions.Count() > 1)
            {
                TShock.Utils.Broadcast(tieMessage, Color.Teal);
                return;
            }

            OptionInfo winningOption = mostVotedOptions.First();

            string resultsFormatted = "";
            foreach (OptionInfo optionInfo in options)
            {
                resultsFormatted += string.Format("{0} - {1} vote{2}\n", optionInfo.option, optionInfo.votes, optionInfo.votes == 1 ? "" : "s");
            }
            resultsFormatted = resultsFormatted.Trim();

            TShock.Utils.Broadcast(resultsFormatted, Color.Aqua);
            TShock.Utils.Broadcast(string.Format("The winning option was \"{0}\" with {1} vote{2}!", winningOption.option, winningOption.votes, winningOption.votes == 1 ? "" : "s"), Color.Orange);

            if (winningOption.methodUponWin == null)
            {
                return;
            }

            winningOption.methodUponWin();
        }

        public static VotingSystem GetVotingSystem(int voteID)
        {
            return ongoingVotes.ElementAtOrDefault(voteID);
        }

        public bool AddVote(TSPlayer voter, int optionID)
        {
            if (optionID < 0 || optionID >= options.Length)
            {
                return false;
            }

            options[optionID].votes++;

            voters.Add(voter);

            return true;
        }

        public bool HasEveryoneVoted()
        {
            if (linkedEvent == null)
            {
                return false;
            }

            if (linkedEvent.tSPlayers.Count == voters.Count)
            {
                return true;
            }

            return false;
        }
    }
}
