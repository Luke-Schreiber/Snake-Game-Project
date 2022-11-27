using System.Collections;
using System.Xml.Linq;

namespace SnakeGame

{
    public static class World
    {
        static ArrayList walls = new ArrayList();
        static ArrayList powerups = new ArrayList();
        static ArrayList snakes = new ArrayList();
        static float worldSize = 2000;

        public static void setSize(float size)
        {
            worldSize = size;
        }
        public static float WorldSize
        {
            get { return worldSize; }
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
            snakes.Add(snakes);
        }
        public static ArrayList Snakes
        {
            get { return snakes; }
        }
    }
}