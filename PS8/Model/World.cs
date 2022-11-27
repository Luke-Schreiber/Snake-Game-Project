using System.Collections;
using System.Xml.Linq;

namespace SnakeGame

{
    public static class World
    {
        static ArrayList walls = new ArrayList();
        static ArrayList powerups = new ArrayList();
        static ArrayList snakes = new ArrayList();

        public static void addWall(Wall wall)
        {
            walls.Add(wall);
        }
        public static void addPowerup(Powerup powerup)
        {
            powerups.Add(powerup);
        }
        public static void addSnake(Snake snake)
        {
            snakes.Add(snakes);
        }
    }
}