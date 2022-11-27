using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty]
        int power;
        [JsonProperty]
        Vector2D loc;
        [JsonProperty]
        bool died;

        public Powerup()
        {
            power = 0;
            loc = new Vector2D();
            died = false;
        }

        public Vector2D Loc
        {
            get { return loc; }
        }
    }
}
