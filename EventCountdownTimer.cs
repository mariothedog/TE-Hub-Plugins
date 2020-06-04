using System;
using System.Timers;

namespace TEHub
{
    class EventCountdownTimer : Timer
    {
        private DateTime endTime;

        public new void Start()
        {
            endTime = DateTime.Now.AddMilliseconds(Interval);
            base.Start();
        }

        public double TimeLeft
        {
            get
            {
                return (endTime - DateTime.Now).TotalMilliseconds;
            }
        }
    }
}
