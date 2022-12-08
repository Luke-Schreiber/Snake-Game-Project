using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Class representing Snake objects with the fields provided and opted in as JsonProperties
    /// </summary>
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

        /// <summary>
        /// Default constructor for Json
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playerID"></param>
        public Snake(string playerName, long playerID)
        {
            snake = (int)playerID;
            name = playerName;
            body = new List<Vector2D>();
            dir = new Vector2D();
            score = 0;
            died = false;
            alive = false;
            dc = false;
            join = false;
        }


        /// <summary>
        /// Simple way to get a snakes PlayerID
        /// </summary>
        public bool DC
        {
            get { return dc; }
        }

        /// <summary>
        /// Simple way to get a snakes PlayerID
        /// </summary>
        public int ID
        {
            get { return snake; }
        }

        /// <summary>
        /// Simple way to tell if a snake has died this fram
        /// </summary>
        public bool Died
        {
            get { return died; }
        }

        /// <summary>
        /// Simple way to tell if a snake is alive
        /// </summary>
        public bool Alive
        {
            get { return alive; }
        }

        /// <summary>
        /// Simple way to get a snakes list of body vertices
        /// </summary>
        public List<Vector2D> Body
        {
            get { return body; }
        }

        /// <summary>
        /// Simple way to get a the player name associated with a snake
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Simple way to get the score of a snake
        /// </summary>
        public int Score
        {
            get { return score; }
        }
    }
}
