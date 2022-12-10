using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

        public int respawnRate = 300;

        public int MSPerFrame = 17;

        public int framesSinceSpawn = 0;

        public int randomRespawnRate = 0;

        private int powerUps = 0;
        
        // Constructor for the world
        public World()
        {
        }

        /// <summary>
        /// Updates the state of the world, updating snake positions, powerup positions, snake collisions and powerup collisions.
        /// Also updates deaths, wraparound, etc.
        /// </summary>
        public void Update()
        {
            
           
            // Powerups -----------------------

            // If increase the respawn frame time if less than random respawn generated time.
            if (framesSinceSpawn < randomRespawnRate)
                framesSinceSpawn++;
            
            // If we haven't maxed out, generate powerups.
            else if (powerups.Count < maxPowerups)
            {
                // Create new respawn generated time and reset frames/time since last spawn
                Random random = new Random();
                randomRespawnRate = (int) (random.NextDouble() * respawnRate);
                framesSinceSpawn = 0;

                // Create a new powerup, set its id etc.
                Powerup p = new Powerup();
                p.loc = FindSpace();
                addPowerup(p);
                p.power = powerUps;
                powerUps++;
            }

            // Remove a powerup from our array by adding to removed powerups list and iterating through it.
            ArrayList RemovePowerup = new ArrayList();

            foreach(Powerup p in powerups)
            {
                if (p.Died)
                    RemovePowerup.Add(p);
            }

            foreach(Powerup p in RemovePowerup)
            {
                powerups.Remove(p);
            }


            // Snakes ------------------------

            // Loop through every snake and do the following.
            foreach (var s in Snakes)
            {
                // If just joined, respawn the snake.
                if (s.Value.join)
                {
                    RespawnSnake(s.Value);
                    s.Value.join = false;
                }
                // If snake has died, then remove snake from world and respawn it.
                if (s.Value.Died)
                {
                    s.Value.died = false;
                }
                if (!s.Value.alive)
                {
                    // After the frame counter has reached the time to respawn, respawn.
                    if (framesSinceDeath[s.Key] == respawnRate)
                    {
                        RespawnSnake(s.Value);
                        framesSinceDeath[s.Key] = 0;
                    }
                    else
                        framesSinceDeath[s.Key]++;
                    continue;
                }

                // Loop through every powerup, kill it, increase our score, reset frames since eaten.
                foreach (Powerup p in powerups)
                {
                    if ((s.Value.body.Last() - p.loc).Length() <= 10)
                    {
                        p.died = true;
                        s.Value.score++;
                        s.Value.framesSinceEaten = 0;
                    }
                }


                // Loop through every wall to check if the current snake has collided.
                foreach (Wall w in Walls)
                {
                    // Check order of wall segments
                    if (w.P1.X < w.P2.X || w.P1.Y < w.P2.Y)
                    {
                        // Checks if snake head is within range of the wall area.
                        if ((s.Value.body.Last().X >= w.P1.X - 30 && s.Value.body.Last().X <= w.P2.X + 30) &&
                            (s.Value.body.Last().Y >= w.P1.Y - 30 && s.Value.body.Last().Y <= w.P2.Y + 30))
                        {
                            s.Value.died = true;
                            s.Value.alive = false;
                        }
                    }
                    else
                    {
                        // Checks if snake head is within range of the wall area.
                        if ((s.Value.body.Last().X >= w.P2.X - 30 && s.Value.body.Last().X <= w.P1.X + 30) &&
                            (s.Value.body.Last().Y >= w.P2.Y - 30 && s.Value.body.Last().Y <= w.P1.Y + 30))
                        {
                            s.Value.died = true;
                            s.Value.alive = false;
                        }
                    }
                }

                // For every other snake than ourself, see if we collide with it.
                foreach (var otherSnake in snakes)
                {
                    List<Vector2D> body = otherSnake.Value.body;

                    // If the snake is not itself, detects collisions.
                    if (otherSnake.Value != s.Value && otherSnake.Value.Alive) {
                        snakeCollision(s.Value, body);
                    }
                }

                // Creates a vector that is the velocity.
                Vector2D v = s.Value.dir * snakeSpeed;

                // If the current direction of snake has changed, add new segment.
                if (s.Value.dirChanged)
                {
                        s.Value.Body.Add(new Vector2D(s.Value.Body.Last().X, s.Value.Body.Last().Y) + v);
                        s.Value.dirChanged = false;
                }
                // Otherwise keep going straight.
                else
                {
                    s.Value.body[s.Value.body.Count - 1] += v;
                    s.Value.framesSinceDirChange++;
                }

                // Detects wraparounds at both the X and Y for the tail and the head vector -------------

                // X value head wraparound
                if (s.Value.Body.Last().X >= worldSize / 2 || s.Value.Body.Last().X <= -1 * worldSize / 2)
                {
                    s.Value.Body.Add(new Vector2D(s.Value.Body.Last().X * -1, s.Value.Body.Last().Y));
                    s.Value.Body.Add(new Vector2D(s.Value.Body.Last().X + v.X, s.Value.Body.Last().Y));
                    
                }
                // Y value head wraparound
                else if (s.Value.Body.Last().Y >= worldSize / 2 || s.Value.Body.Last().Y <= -1 * worldSize / 2)
                {
                    s.Value.Body.Add(new Vector2D(s.Value.Body.Last().X, s.Value.Body.Last().Y * -1));
                    s.Value.Body.Add(new Vector2D(s.Value.Body.Last().X, s.Value.Body.Last().Y + v.Y));
                }
                // X value tail wraparound
                if (s.Value.Body[0].X >= worldSize / 2 || s.Value.Body[0].X <= -1 * worldSize / 2)
                {
                    s.Value.body.RemoveAt(0);
                }
                // Y value tail wraparound
                else if (s.Value.Body[0].Y >= worldSize / 2 || s.Value.Body[0].Y <= -1 * worldSize / 2)
                {
                    s.Value.body.RemoveAt(0);
                }
                // --------------

                // Detects self collisions for the snake. For every segment in the snake, check if the current direction is in the opposite of another segment.
                for (int i = s.Value.Body.Count - 1; i >= 1; i--)
                {
                    // Current snake segment
                    Vector2D segment = (s.Value.Body[i] - s.Value.Body[i - 1]);
                    
                    // So long as there isn't a wraparound
                    if (!(segment.Length() >= WorldSize))
                    {
                        // Find the direction of this vector and check if it is the opposite of current direction.
                        segment.Normalize();
                        if (segment.X == s.Value.dir.X * -1 && segment.Y == s.Value.dir.Y * -1)
                        {
                            // Make a new list of vectors that can be collided into
                            List<Vector2D> smallerBody = new List<Vector2D>(s.Value.Body);
                            smallerBody.RemoveRange(i + 1, smallerBody.Count - 1 - (i));

                            // Try collisions with this new list.
                            snakeCollision(s.Value, smallerBody);
                            break;
                        }
                    }
                }
            
                // If tail catches up to segment, delete it, otherwise advance it.
                Vector2D tailDist = s.Value.body[1] - s.Value.body[0];
                if (Math.Abs(tailDist.X) <= snakeSpeed && Math.Abs(tailDist.Y) <= snakeSpeed)
                {
                    s.Value.body.RemoveAt(0);
                }
                else
                {
                    tailDist.Normalize();
                    if (s.Value.framesSinceEaten < growth)
                        s.Value.framesSinceEaten++;
                    else
                        s.Value.body[0] += tailDist * snakeSpeed;
                }
            }
        }

        /// <summary>
        /// Takes a snake and a body that the snake head can collide into. Kills snake if snake head collides into body segment.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="body"></param>
        private void snakeCollision(Snake s, List<Vector2D> body)
        {
            // For ever body vector check if the snake head is within bounds for the body vector segment.
            for (int i = 0; i < body.Count - 1; i++)
            {
                if (body[i].X < body[i + 1].X || body[i].Y < body[i + 1].Y)
                {
                    if ((s.body.Last().X >= body[i].X - 10 && s.body.Last().X <= body[i + 1].X + 10) &&
                    (s.body.Last().Y >= body[i].Y - 10 && s.body.Last().Y <= body[i + 1].Y + 10))
                    {
                        s.died = true;
                        s.alive = false;
                    }
                }
                else
                {
                    if ((s.body.Last().X >= body[i + 1].X - 10 && s.body.Last().X <= body[i].X + 10) &&
                    (s.body.Last().Y >= body[i + 1].Y - 10 && s.body.Last().Y <= body[i].Y + 10))
                    {
                        s.died = true;
                        s.alive = false;
                    }
                }

            }
        }

        /// <summary>
        /// Respawns the snake in a correct space with a random direction.
        /// </summary>
        /// <param name="s"></param>
        private void RespawnSnake(Snake s)
        {
            // Sets alive to true, resets body and score, and generates random correct space and direction.
            s.alive = true;
            Random random = new Random();
            int randDir = random.Next(0, 4);
            Vector2D head = FindSpace();
            s.body.Clear();
            s.score = 0;


            // Creates new snake and direction based on random number.
            if (randDir == 0)
            {
                s.dir = new Vector2D(1, 0);
                s.body.Add(new Vector2D(head.X - startLength, head.Y));
                s.body.Add(head);
            }
            else if (randDir == 1)
            {
                s.dir = new Vector2D(-1, 0);
                s.body.Add(new Vector2D(head.X + startLength, head.Y));
                s.body.Add(head);
            }
            else if (randDir == 2)
            {
                s.dir = new Vector2D(0, 1);
                s.body.Add(new Vector2D(head.X, head.Y - startLength));
                s.body.Add(head);
            }
            else if (randDir == 3)
            {
                s.dir = new Vector2D(0, -1);
                s.body.Add(new Vector2D(head.X, head.Y + startLength));
                s.body.Add(head);
            }
         
            
        }

        /// <summary>
        /// Finds a viable space to respawn.
        ///
        /// </summary>
        /// <returns></returns>
        private Vector2D FindSpace()
        {
            bool viableloc = false;
            int randX = 0;
            int randY = 0;

            // If a viable location is found return it.
            while (!viableloc)
            {
                // Creates new random coordinates within the world.
                Random random = new Random();
                randX = random.Next(-1* (((int)worldSize) / 2 - (50+startLength)), ((int)worldSize) / 2 - (50 + startLength)); 
                randY = random.Next(-1 * (((int)worldSize) / 2 - (50+startLength)), ((int)worldSize) / 2 - (50 + startLength));
                viableloc = true;

                // Checks if new point has spawned in walls. If so, not a viable loc and restart.
                foreach (Wall w in walls)
                {
                    if (w.P1.X < w.P2.X || w.P1.Y < w.P2.Y)
                    {
                        if ((randX >= w.P1.X - 30 && randX <= w.P2.X + 30) &&
                            (randY >= w.P1.Y - 30 && randY <= w.P2.Y + 30))
                        {
                            viableloc = false;
                        }
                    }
                    else
                    {
                        if ((randX >= w.P2.X - 30 && randX <= w.P1.X + 30) &&
                            (randY >= w.P2.Y - 30 && randY <= w.P1.Y + 30))
                        {
                            viableloc = false;
                        }
                    }
                }
            }

            // Return viable location.
            return new Vector2D(randX, randY);
        }

        /// <summary>
        /// Clears the world.
        /// </summary>
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