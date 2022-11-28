using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Class representing PowerUp objects with the fields provided and opted in as JsonProperties
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty]
        int power;
        [JsonProperty]
        Vector2D loc;
        [JsonProperty]
        bool died;

        /// <summary>
        /// default constructor for Json
        /// </summary>
        public Powerup()
        {
            power = 0;
            loc = new Vector2D();
            died = false;
        }

        /// <summary>
        /// Simple way to get the location of a powerup
        /// </summary>
        public Vector2D Loc
        {
            get { return loc; }
        }

        /// <summary>
        /// Simple way to get if a PowerUp has died
        /// </summary>
        public bool Died
        {
            get { return died; }
        }
    }
}
