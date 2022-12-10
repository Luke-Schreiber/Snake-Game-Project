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
        public int FramesPerShot = 80;
        [DataMember(Order = 1)]
        public int MSPerFrame = 17;
        [DataMember(Order = 2)]
        public int RespawnRate = 300;
        [DataMember(Order = 3)]
        public int UniverseSize = 2000;
        [DataMember(Order = 4)]
        public List<Wall>? Walls;
        [DataMember(Order = 5)]
        public int snakeSpeed = 3;
        [DataMember(Order = 6)]
        public int startLength = 120;
        [DataMember(Order = 7)]
        public int growth = 12;
        [DataMember(Order = 8)]
        public int maxPowerups = 20;
        [DataMember(Order = 9)]
        public int pDelay = 200;


        public GameSettings()
        { }

    }
}
