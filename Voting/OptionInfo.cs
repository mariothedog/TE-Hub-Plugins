using System;

namespace TEHub.Voting
{
    public class OptionInfo
    {
        public string option;
        public Action methodUponWin;
        public string winMessage;
        public int votes = 0;

        public OptionInfo(string option, string winMessage, Action methodUponWin = null)
        {
            this.option = option;
            this.winMessage = winMessage;
            this.methodUponWin = methodUponWin;
        }
    }
}
