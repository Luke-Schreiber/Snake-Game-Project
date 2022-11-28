using System.Collections;
using System.Xml.Linq;

namespace SnakeGame

{
    /// <summary>
    /// Class representing a World object which stores all the walls, snakes, and powerups in the current world state
    /// </summary>
    public static class World
    {
        //Helpful things for a world to have saved
        static ArrayList walls = new ArrayList();
        static ArrayList powerups = new ArrayList();
        static float worldSize = 2000;

        //Dictionary used for snakes so that they can be quickly found using their ID
        static Dictionary<int, Snake> snakes = new Dictionary<int, Snake>();

        // An object used in the Lock that prevents drawing and Adding to the world from happening at the same time
        static object worldState = 0;

        // A dictionary of snake IDs and a count of framesSinceDeath for a snake, which are used to animate the explosions
        // that play when a snake dies
        static Dictionary<int, int> framesSinceDeath = new Dictionary<int, int>();

        /// <summary>
        /// Simple way to get the Dictionary containing the framesSinceDeath counters for all the snakes in the world
        /// </summary>
        public static Dictionary<int, int> FramesSinceDeath
        {
            get { return framesSinceDeath; }
        }

        /// <summary>
        /// Way to get the WorldState object so that both the Controller and the View can get it and use it as a lock
        /// </summary>
        public static object WorldState
        {
            get { return worldState; }
        }

        /// <summary>
        /// Way to get and set the Size of the World so the view knows how big to draw it based on server input
        /// </summary>
        public static float WorldSize
        {
            get { return worldSize; }
            set { worldSize = value; }
        }

        /// <summary>
        /// Method that allows the controller to add walls from the server to the world
        /// </summary>
        /// <param name="wall"></param>
        public static void addWall(Wall wall)
        {
            walls.Add(wall);
        }
        /// <summary>
        /// Way to get the ArrayList of walls in the world so they can be drawn
        /// </summary>
        public static ArrayList Walls
        {
            get { return walls; }
        }

        /// <summary>
        /// Method that allows the controller to add powerups from the server to the world
        /// </summary>
        /// <param name="powerup"></param>
        public static void addPowerup(Powerup powerup)
        {
            powerups.Add(powerup);
        }
        /// <summary>
        /// Way to get the ArrayList of Powerups in the world so they can be drawn
        /// </summary>
        public static ArrayList Powerups
        {
            get { return powerups; }
        }

        /// <summary>
        /// Method that allows for the controller to add snakes from the server to the world
        /// </summary>
        /// <param name="snake"></param>
        public static void addSnake(Snake snake)
        {
            // When a new snake arrives, adds it the the framesSinceDeath dictionary so that the death animation
            // can play when it dies
            if (!framesSinceDeath.ContainsKey(snake.ID))
                framesSinceDeath.Add(snake.ID, 1);

            // If a snake is already saved in the world, checks for its current state of life and updates the value
            // of its framesSinceDeath counter accordingly.
            // Also removes a snake from the Dictionary storing them so that its updated version can be added instead
            if (snakes.ContainsKey(snake.ID))
            {
                if (!snake.Alive)
                    framesSinceDeath[snake.ID]++;
                else if (framesSinceDeath.ContainsKey(snake.ID))            
                    framesSinceDeath[snake.ID] = 0;

                snakes.Remove(snake.ID);
            }

            // Adds a snake to the dictionary saving them all
            snakes.Add(snake.ID, snake);
        }
        /// <summary>
        /// Way to get the Dictionary of snakes so that a snake can be found when needed
        /// </summary>
        public static Dictionary<int, Snake> Snakes
        {
            get { return snakes; }
        }

        /// <summary>
        /// Method to remove all currently saved powerups so that the current ones from the server can be drawn,
        /// prevents the list of powerups from becoming too large
        /// </summary>
        public static void RemovePowerups()
        {
            powerups.Clear();
        }
    }
}