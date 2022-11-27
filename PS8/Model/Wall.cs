using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty]
        int wall;
        [JsonProperty]
        Vector2D p1;
        [JsonProperty]
        Vector2D p2;

        public Wall()
        {
            wall = 0;
            p1 = new Vector2D();
            p2 = new Vector2D();
        }

        public Vector2D P1 {
            get { return p1; }
        }
        public Vector2D P2
        {
            get { return p2; }
        }
    }
}
