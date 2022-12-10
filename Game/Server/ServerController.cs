using NetworkUtil;
using Newtonsoft.Json;
using Server;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SnakeGame
{
    /// <summary>
    /// Console application that runs a server for snake playing
    /// </summary>
    class Server
    {
        private Dictionary<long, SocketState> clients;
        private World serverWorld;
        private GameSettings? settings;
        private string wallSettings;
        private List<long> disconnectedPlayers = new List<long>();

        /// <summary>
        /// Main method that makes a new instance of a server and calls a method to start running it along with a frame loop
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer();
            server.FrameLoop();
        }

        /// <summary>
        /// default server constructor, initializes important instance variables
        /// </summary>
        public Server()
        {
            clients = new Dictionary<long, SocketState>();
            serverWorld = new World();
            wallSettings = "";
        }

        /// <summary>
        /// Starts essential server processes, such as listening for clients
        /// </summary>
        public void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewPlayerConnected, 11000);

            // code to deserialize the settings file
            DataContractSerializer ser = new DataContractSerializer(typeof(GameSettings));
            XmlReader reader;
            try
            {
                reader = XmlReader.Create("settings.xml");
            }
            catch(Exception)
            {
                reader = XmlReader.Create("../../../settings.xml");
            }

            settings = ser.ReadObject(reader) as GameSettings;

            // stringbuilds the the string to send to clients for startup information
            StringBuilder buildWalls = new StringBuilder();
            foreach (Wall w in settings!.Walls!)
            {
                buildWalls.Append(JsonConvert.SerializeObject(w) + "\n");
                serverWorld.addWall(w);
            }
            wallSettings = buildWalls.ToString();

            // saves important settings file information into serverWorld so that it can be used there
            serverWorld.snakeSpeed = settings.snakeSpeed;
            serverWorld.startLength = settings.startLength;
            serverWorld.growth = settings.growth;
            serverWorld.maxPowerups = settings.maxPowerups;
            serverWorld.pDelay = settings.pDelay;
            serverWorld.respawnRate = settings.RespawnRate;
            serverWorld.MSPerFrame = settings.MSPerFrame;

            Console.WriteLine("Server is running");
        }

        /// <summary>
        /// Handler for when a player tries to connect
        /// </summary>
        /// <param name="state"></param>
        private void NewPlayerConnected(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                return;
            }

            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            state.OnNetworkAction = Handshake;

            Networking.GetData(state);
        }

        /*
         * Receive player name - this is a delegate that implements the server's part of the initial handshake.
         * Make a new Snake with the given name and a new unique ID (recommend using the SocketState's ID).
         * Then change the callback to a method that handles command requests from the client.
         * Then send the startup info to the client. Then add the client's socket to a list of all clients.
         * Then ask the client for data.
         * Note: it is important that the server sends the startup info before adding the client to the list of all clients.
         * This guarantees that the startup info is sent before any world info.
         * Remember that the server is running a loop on a separate thread that may send world info to the list of clients at any time.
         */
        private void Handshake(SocketState state)
        {
            // handles if a client disconnected before handshake
            if (state.ErrorOccurred)
            {
                Console.WriteLine("Client " + state.ID + " disconnected");
                lock (serverWorld)
                {
                    clients.Remove(state.ID);
                }
                return;
            }

            // creates a snake for the new player
            string playerName = state.GetData().Replace("\n", "");
            state.RemoveData(0, state.GetData().Length);
            Snake newSnake = new Snake(playerName, state.ID);
            newSnake.join = true;

            state.OnNetworkAction = CommandRequest;

            // sets up the startUp information string that a player needs when they connect and sends it to them
            string startUp = "" + state.ID + "\n" + settings!.UniverseSize + "\n" + wallSettings;
            state.TheSocket.Send(Encoding.ASCII.GetBytes(startUp.ToString()));

            // add the players snake to the world and adds them to the list of clients, locked to prevent race conditions
            lock (serverWorld)
            {
                serverWorld.addSnake(newSnake);
                clients[state.ID] = state;
            }

            Console.WriteLine("Player: " + playerName + " has connected");

            Networking.GetData(state);
        }

        /// <summary>
        /// Handler for when the player sends a command request 
        /// </summary>
        /// <param name="state"></param>
        private void CommandRequest(SocketState state)
        {
            // if they disconnect before this process somehow, this handles it
            if (state.ErrorOccurred)
            {
                Console.WriteLine("Client " + state.ID + " disconnected");
                lock (serverWorld)
                {
                    clients.Remove(state.ID);
                    serverWorld.Snakes.Remove((int)state.ID);
                }
                return;
            }

            // reads the player command and grabs their snake from the world for use
            string command = state.GetData().Replace("\n", ""); ;
            state.RemoveData(0, state.GetData().Length);
            Snake playerSnake = serverWorld.Snakes[(int)state.ID];

            // bool used to check that the head segment of the snake is not shorter than a snakes width, used to prevent 180s
            bool currentSegCheck = (playerSnake.body.Last() - playerSnake.body[playerSnake.body.Count - 2]).Length() < 10;

            // series of if statements that check what direction the user wants to move in, the first nested if makes sure that this 
            // is an allowed direction based on how they were moving  before. The next ones are for ignoring commands that would cause a 180
            if (command == "{\"moving\":\"left\"}")
            {
                
                if (!playerSnake.dir.Equals(new Vector2D(-1,0)) && !playerSnake.dir.Equals(new Vector2D(1, 0)))
                {
                    if (playerSnake.body.Count == 2)
                    {
                        playerSnake.dir = new Vector2D(-1, 0);
                        playerSnake.dirChanged = true;
                    }

                    else if (!(currentSegCheck && (playerSnake.body[playerSnake.body.Count - 2].X - playerSnake.body[playerSnake.body.Count - 3].X > 0)))
                    {
                        playerSnake.dir = new Vector2D(-1, 0);
                        playerSnake.dirChanged = true;
                    }
                }
            }
            else if (command == "{\"moving\":\"right\"}")
            {
                if (!playerSnake.dir.Equals(new Vector2D(-1, 0)) && !playerSnake.dir.Equals(new Vector2D(1, 0)))
                {
                    if (playerSnake.body.Count == 2)
                    {
                        playerSnake.dir = new Vector2D(1, 0);
                        playerSnake.dirChanged = true;
                    }

                    else if (!(currentSegCheck && (playerSnake.body[playerSnake.body.Count - 2].X - playerSnake.body[playerSnake.body.Count - 3].X < 0)))
                    {
                        playerSnake.dir = new Vector2D(1, 0);
                        playerSnake.dirChanged = true;
                    }
                }
            }
            else if (command == "{\"moving\":\"down\"}")
            {
                if (!playerSnake.dir.Equals(new Vector2D(0, -1)) && !playerSnake.dir.Equals(new Vector2D(0, 1)))
                {
                    if (playerSnake.body.Count == 2)
                    {
                        playerSnake.dir = new Vector2D(0,1);
                        playerSnake.dirChanged = true;
                    }

                    else if (!(currentSegCheck && (playerSnake.body[playerSnake.body.Count - 2].Y - playerSnake.body[playerSnake.body.Count - 3].Y < 0)))
                    {
                        playerSnake.dir = new Vector2D(0,1);
                        playerSnake.dirChanged = true;
                    }
                }

            }
            else if (command == "{\"moving\":\"up\"}")
            {
                if (!playerSnake.dir.Equals(new Vector2D(0, -1)) && !playerSnake.dir.Equals(new Vector2D(0,1)))
                {
                    if (playerSnake.body.Count == 2)
                    {
                        playerSnake.dir = new Vector2D(0,-1);
                        playerSnake.dirChanged = true;
                    }

                    else if (!(currentSegCheck && (playerSnake.body[playerSnake.body.Count - 2].Y - playerSnake.body[playerSnake.body.Count - 3].Y > 0)))
                    {
                        playerSnake.dir = new Vector2D(0,-1);
                        playerSnake.dirChanged = true;
                    }
                }
            }

            Networking.GetData(state);
        }

        /// <summary>
        /// Started in main, loops infinitely so the server does not close. Updates frames based on their duration as provided in settings
        /// </summary>
        private void FrameLoop()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (true)
            {
                while(watch.ElapsedMilliseconds < settings!.MSPerFrame)
                {}
                watch.Restart();
                Update();
            }
            
        }

        /// <summary>
        /// Updates the world based on movements and other happenings, and sends it to all clients
        /// </summary>
        private void Update()
        {
            // locked to prevent race conditions
            lock (serverWorld)
            {
                // if a player disconnected last frame, remove them from the list of clients and remove their snake from the world
                for(int i = 0; i < disconnectedPlayers.Count; i++)
                {
                    clients.Remove(disconnectedPlayers[i]);
                    serverWorld.Snakes.Remove((int)disconnectedPlayers[i]);
                }
                disconnectedPlayers.Clear();

                // Update the world
                serverWorld.Update();
            
            // Send powerups and snakes to each client
                foreach (SocketState client in clients.Values)
                {

                    //powerups
                    foreach (Powerup p in serverWorld.Powerups)
                    {
                        // if sending to a client fails, add them to disconnected players and stop trying to send to them
                        if (!Networking.Send(client.TheSocket, JsonConvert.SerializeObject(p) + "\n"))
                        {
                            if(!disconnectedPlayers.Contains(client.ID))
                                disconnectedPlayers.Add(client.ID);
                            break;
                        }

                    }
                    //snakes
                    foreach (var s in serverWorld.Snakes)
                    {
                        // if sending to a client fails, add them to disconnected players and stop trying to send to them
                        // also tells the world that the player has disconnected this frame, that it is dead, and that is not alive
                        if (!Networking.Send(client.TheSocket, JsonConvert.SerializeObject(s.Value) + "\n"))
                        {
                            serverWorld.Snakes[(int)client.ID].died = true;
                            serverWorld.Snakes[(int)client.ID].alive = false;
                            serverWorld.Snakes[(int)client.ID].dc = true;
                            if (!disconnectedPlayers.Contains(client.ID))
                                disconnectedPlayers.Add(client.ID);
                            break;
                        }
                    }
                }
            }
        }
    }
}