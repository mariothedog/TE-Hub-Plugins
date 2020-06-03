using System.Collections.Generic;
using TShockAPI;

namespace TEHub
{
    public static class Variables
    {
        public readonly static Dictionary<string, List<TSPlayer>> playersInEvents = new Dictionary<string, List<TSPlayer>>()
        {
            { "arcticcircle", new List<TSPlayer>() },
            { "tbr", new List<TSPlayer>() }
        };
    }
}
