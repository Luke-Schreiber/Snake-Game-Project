using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [DataContract(Namespace = "")]
    internal class GameSettings
    {

        [DataMember(Order = 0)]
        public int? FramesPerShot;
        [DataMember(Order = 1)]
        public int? MSPerFrame;
        [DataMember(Order = 2)]
        public int? RespawnRate;
        [DataMember(Order = 3)]
        public int? UniverseSize;
        [DataMember(Order = 4)]
        public List<Wall>? Walls;

        public GameSettings()
        { }

    }
}
