using System.Collections;
using System.Xml.Linq;

namespace SnakeGame

{
    public static class World
    {
        static ArrayList walls = new ArrayList();
        static ArrayList powerups = new ArrayList();
        static Dictionary<int, Snake> snakes = new Dictionary<int, Snake>();
        static float worldSize = 2000;
        static object worldState = 0;
        static Dictionary<int, int> framesSinceDeath = new Dictionary<int, int>();

        public static Dictionary<int, int> FramesSinceDeath
        {
            get { return framesSinceDeath; }
        }

        public static object WorldState
        {
            get { return worldState; }
        }

        public static float WorldSize
        {
            get { return worldSize; }
            set { worldSize = value; }
        }
        public static void addWall(Wall wall)
        {
            walls.Add(wall);
        }
        public static ArrayList Walls
        {
            get { return walls; }
        }
        public static void addPowerup(Powerup powerup)
        {
            powerups.Add(powerup);
        }
        public static ArrayList Powerups
        {
            get { return powerups; }
        }
        public static void addSnake(Snake snake)
        {
            if (!framesSinceDeath.ContainsKey(snake.ID))
                framesSinceDeath.Add(snake.ID, 1);

            if (snakes.ContainsKey(snake.ID))
            {
                if (!snake.Alive)
                    framesSinceDeath[snake.ID]++;
                else if (framesSinceDeath.ContainsKey(snake.ID))            
                    framesSinceDeath[snake.ID] = 0;

                snakes.Remove(snake.ID);
            }
            snakes.Add(snake.ID, snake);
        }
        public static Dictionary<int, Snake> Snakes
        {
            get { return snakes; }
        }
        public static void RemovePowerups()
        {
            powerups.Clear();
        }
    }
}