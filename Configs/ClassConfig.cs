using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TEHub.EventClasses;
using TShockAPI;

namespace TEHub.Configs
{
    public class ClassConfig : Config
    {
        public static ClassConfig config;

        public static string configPath = Path.Combine(TShock.SavePath, "classesconfig.json");

        public List<EventClass> eventClasses = new List<EventClass>();

        public static ClassConfig Read(string path)
        {
            if (!File.Exists(path))
            {
                return new ClassConfig();
            }

            return JsonConvert.DeserializeObject<ClassConfig>(File.ReadAllText(path));
        }
    }
}
