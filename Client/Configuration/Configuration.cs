using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Configuration
{
    class Configuration
    {
        public ConfigurationObject lightObject, heavyObject, wall, player, flat, bomb;
        public static ConfigurationObject LightObject, HeavyObject, Wall, Player, Flat, Bomb;

        public static void Load(string configFileName)
        {
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(new StreamReader(configFileName).ReadToEnd());
            LightObject = config.lightObject;
            HeavyObject = config.heavyObject;
            Wall = config.wall;
            Player = config.player;
            Flat = config.flat;
            Bomb = config.bomb;
        }
    }
}
