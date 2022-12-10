using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Class representing Wall objects with the fields provided and opted in as JsonProperties
    /// </summary>
    [DataContract(Namespace = "")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        [DataMember(Order = 0)]
        private int ID;
        [JsonProperty]
        [DataMember(Order = 1)]
        Vector2D p1;
        [JsonProperty]
        [DataMember(Order = 2)]
        Vector2D p2;
        
        /// <summary>
        /// Default constructor for Json
        /// </summary>
        public Wall()
        {
            ID = 0;
            p1 = new Vector2D();
            p2 = new Vector2D();
        }

        /// <summary>
        /// Simple way to get the the first point needed to draw a line of walls
        /// </summary>
        public Vector2D P1 {
            get { return p1; }
        }
        /// <summary>
        /// Simple way to get the second point needed to draw a line of walls
        /// </summary>
        public Vector2D P2
        {
            get { return p2; }
        }
        /// <summary>
        /// Simple way to get the second point needed to draw a line of walls
        /// </summary>
        public int id
        {
            get { return ID; }
        }
    }
}
