using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SnakeGame

{
    /// <summary>
    /// Class representing a World object which stores all the walls, snakes, and powerups in the current world state
    /// </summary>
    public class World
    {
        //Helpful things for a world to have saved
        ArrayList walls = new ArrayList();
        ArrayList powerups = new ArrayList();

        float worldSize = 2000;

        //Dictionary used for snakes so that they can be quickly found using their ID
        Dictionary<int, Snake> snakes = new Dictionary<int, Snake>();

        // An object used in the Lock that prevents drawing and Adding to the world from happening at the same time
        object worldState = 0;

        // A dictionary of snake IDs and a count of framesSinceDeath for a snake, which are used to animate the explosions
        // that play when a snake dies
        Dictionary<int, int> framesSinceDeath = new Dictionary<int, int>();

        // A dictionary of snake IDs and when the snake joined, represented by an order number.
        Dictionary<int, int> snakeOrder = new Dictionary<int, int>();

        // Amount of snakes drawn since the player joined.
        int snakesSinceJoined = 0;

        // Server side variables
        public int snakeSpeed = 3;

        public int startLength = 120;

        public int growth = 12;

        public int maxPowerups = 20;

        public int pDelay = 200;


        public World()
        {
        }

        public void Update()
        {

        }
        public void Clear()
        {
            ArrayList walls = new ArrayList();
            ArrayList powerups = new ArrayList();
            worldSize = 2000;
            Dictionary<int, Snake> snakes = new Dictionary<int, Snake>();
            object worldState = 0;
            Dictionary<int, int> framesSinceDeath = new Dictionary<int, int>();
            Dictionary<int, int> snakeOrder = new Dictionary<int, int>();
            snakesSinceJoined = 0;
        }

        /// <summary>
        /// Simple way to get the Dictionary containing the snakeOrder counters for all the snakes in the world
        /// </summary>
        public Dictionary<int, int> SnakeOrder
        {
            get { return snakeOrder; }
        }

        /// <summary>
        /// Simple way to get the Dictionary containing the framesSinceDeath counters for all the snakes in the world
        /// </summary>
        public Dictionary<int, int> FramesSinceDeath
        {
            get { return framesSinceDeath; }
        }

        /// <summary>
        /// Way to get the WorldState object so that both the Controller and the View can get it and use it as a lock
        /// </summary>
        public object WorldState
        {
            get { return worldState; }
        }

        /// <summary>
        /// Way to get and set the Size of the World so the view knows how big to draw it based on server input
        /// </summary>
        public float WorldSize
        {
            get { return worldSize; }
            set { worldSize = value; }
        }

        /// <summary>
        /// Method that allows the controller to add walls from the server to the world
        /// </summary>
        /// <param name="wall"></param>
        public void addWall(Wall wall)
        {
            walls.Add(wall);
        }
        /// <summary>
        /// Way to get the ArrayList of walls in the world so they can be drawn
        /// </summary>
        public ArrayList Walls
        {
            get { return walls; }
        }

        /// <summary>
        /// Method that allows the controller to add powerups from the server to the world
        /// </summary>
        /// <param name="powerup"></param>
        public void addPowerup(Powerup powerup)
        {
            powerups.Add(powerup);
        }
        /// <summary>
        /// Way to get the ArrayList of Powerups in the world so they can be drawn
        /// </summary>
        public ArrayList Powerups
        {
            get { return powerups; }
        }

        /// <summary>
        /// Method that allows for the controller to add snakes from the server to the world
        /// </summary>
        /// <param name="snake"></param>
        public void addSnake(Snake snake)
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

            // Adds a snake to the dictionary saving them all, unless player has disconnected.
            if (!snake.DC)
                snakes.Add(snake.ID, snake);

            // Adds a snake to the order dictionary saving them all, if it isn't in there already.
            if (!snakeOrder.ContainsKey(snake.ID))
            {
                snakeOrder.Add(snake.ID, snakesSinceJoined);
                snakesSinceJoined++;
            }
        }
        /// <summary>
        /// Way to get the Dictionary of snakes so that a snake can be found when needed
        /// </summary>
        public Dictionary<int, Snake> Snakes
        {
            get { return snakes; }
        }

        /// <summary>
        /// Method to remove all currently saved powerups so that the current ones from the server can be drawn,
        /// prevents the list of powerups from becoming too large
        /// </summary>
        public void RemovePowerups()
        {
            powerups.Clear();
        }
    }
}