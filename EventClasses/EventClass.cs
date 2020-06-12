using System.Collections.Generic;

namespace TEHub.EventClasses
{
    public class EventClass
    {
        public string eventName;
        public string className;
        public int maxHealth;
        public int maxMana;

        public Dictionary<int, ClassItem> items = new Dictionary<int, ClassItem>();
        public Dictionary<int, ClassItem> coins = new Dictionary<int, ClassItem>();
        public Dictionary<int, ClassItem> ammo = new Dictionary<int, ClassItem>();
        public Dictionary<int, string> armor = new Dictionary<int, string>();
        public Dictionary<int, string> accessories = new Dictionary<int, string>();
        public Dictionary<int, string> armorVanity = new Dictionary<int, string>();
        public Dictionary<int, string> accessoryVanity = new Dictionary<int, string>();
        public Dictionary<int, string> armorDyes = new Dictionary<int, string>();
        public Dictionary<int, string> accessoryDyes = new Dictionary<int, string>();
        public Dictionary<int, string> miscEquips = new Dictionary<int, string>();
        public Dictionary<int, string> miscEquipDyes = new Dictionary<int, string>();
    }
}
