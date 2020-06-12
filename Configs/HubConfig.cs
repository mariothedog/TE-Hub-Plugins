using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace TEHub.Configs
{
    public class HubConfig : Config
    {
        public static HubConfig config;

        public static string configPath = Path.Combine(TShock.SavePath, "tehub.json");

        public List<HubEvent> HubEvents = new List<HubEvent>();

        public static HubConfig Read(string path)
        {
            if (!File.Exists(path))
            {
                return new HubConfig();
            }

            return JsonConvert.DeserializeObject<HubConfig>(File.ReadAllText(path));
        }
    }
}
