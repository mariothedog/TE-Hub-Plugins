using Newtonsoft.Json;
using System.IO;
using TShockAPI;

namespace TEHub
{
    public class Config
    {
        public static Config config;

        public static string configPath = Path.Combine(TShock.SavePath, "tehub.json");

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            if (!File.Exists(path))
            {
                return new Config();
            }

            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}
