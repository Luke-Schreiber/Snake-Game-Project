using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        [JsonProperty]
        int snake;
        [JsonProperty]
        string name;
        [JsonProperty]
        List<Vector2D> body;
        [JsonProperty]
        Vector2D dir;
        [JsonProperty]
        int score;
        [JsonProperty]
        bool died;
        [JsonProperty]
        bool alive;
        [JsonProperty]
        bool dc;
        [JsonProperty]
        bool join;

        public Snake()
        {
            snake = 0;
            name = "";
            body = new List<Vector2D>();
            dir = new Vector2D();
            score = 0;
            died = false;
            alive = false;
            dc = false;
            join = false;
        }
    }
}
