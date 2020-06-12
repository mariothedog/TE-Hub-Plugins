using Newtonsoft.Json;
using System.IO;

namespace TEHub
{
    public abstract class Config
    {
        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
