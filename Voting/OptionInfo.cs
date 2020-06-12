using System;

namespace TEHub.Voting
{
    public class OptionInfo
    {
        public string option;
        public Action methodUponWin;
        public int votes = 0;

        public OptionInfo(string option, Action methodUponWin = null)
        {
            this.option = option;
            this.methodUponWin = methodUponWin;
        }
    }
}
