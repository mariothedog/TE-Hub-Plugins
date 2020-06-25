using System;
using System.Timers;

namespace TEHub
{
    class CountdownTimer : Timer
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
