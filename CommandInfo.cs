using System.Collections.Generic;
using TShockAPI;

namespace TEHub
{
    public class CommandInfo
    {
        public string permissions;
        public string[] names;

        public CommandInfo(string permissions, CommandDelegate cmd, params string[] names)
        {
            this.permissions = permissions;
            this.names = names;

            Commands.ChatCommands.Add(new Command(permissions, cmd, names));
        }
    }
}
