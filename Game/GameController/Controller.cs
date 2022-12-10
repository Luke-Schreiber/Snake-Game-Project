using NetworkUtil;
using System.Net.Security;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SnakeGame;
public static class Controller
{
    private static SocketState? Server;
    private static string PlayerName = "player";
    private static int PlayerID;
    public static World World = new World();
    public static event Action? UpdateFrame;
    public static event Action? ConnectionLost;
    public static event Action? FailedToConnect;
    public static event Action? ConnectionSuccess;

    /// <summary>
    /// Getter for the Players given playerID so that it can be used to identify them later
    /// </summary>
    /// <returns></returns>
    public static int getID()
    {
        return PlayerID;
    }

    /// <summary>
    /// Connects to the server and saves the inputted playerName
    /// </summary>
    /// <param name="serverAddress"></param>
    /// <param name="playerName"></param>
    public static void Connect(string serverAddress, string playerName)
    { 
        Networking.ConnectToServer(OnConnect, serverAddress, 11000);
        PlayerName = playerName;
    }

    /// <summary>
    /// Tells the server a move up command has been sent if there is a connection
    /// </summary>
    public static void MoveUp()
    {
        if(Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"up\"}\n");
    }

    /// <summary>
    /// Tells the server a move down command has been sent if there is a connection
    /// </summary>
    public static void MoveDown()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"down\"}\n");
    }

    /// <summary>
    /// Tells the server a move left command has been sent if there is a connection
    /// </summary>
    public static void MoveLeft()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"left\"}\n");
    }

    /// <summary>
    /// Tells the server a move right command has been sent if there is a connection
    /// </summary>
    public static void MoveRight()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"right\"}\n");
    }

    /// <summary>
    /// Callback for when connection to server is established, starts the loop to listen for data from the server.
    /// Also saves the socket connection for use.
    /// </summary>
    /// <param name="state"></param>
    private static void OnConnect(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            if (FailedToConnect != null)
                FailedToConnect();
            state.TheSocket.Close();
            return;   
        }
        // event to grey out the connect button if a successful connection has been made
        if (ConnectionSuccess != null)
            ConnectionSuccess();

        Server = state;
        state.OnNetworkAction = ReceiveData;
        Networking.GetData(state);
        Networking.Send(Server.TheSocket, PlayerName + "\n");

    }

    /// <summary>
    /// Used for the networking codes OnNetworkAction call, accepts data from the server and uses a private helper method to deserialize it
    /// </summary>
    /// <param name="state"></param>
    private static void ReceiveData(SocketState state)
    {
        // tells the view to display a message if connection to a server is lost while playing, closes the socket, and resets the world
        if(state.ErrorOccurred)
        {
            World.Clear();
            if (ConnectionLost != null)
                ConnectionLost();
            state.TheSocket.Close();
            return;
        }
        // call method for json deserialization
        ProcessData(state);

        //continues loop
        Networking.GetData(state);



        
    }

    /// <summary>
    /// Processes given data in the form of a socket state by deserializing it from a json format
    /// </summary>
    /// <param name="state"></param>
    private static void ProcessData(SocketState state)
    {
        // creates a string of all data provided and a regex to split it into usable lines
        string data = state.GetData();
        string[] parts = Regex.Split(data, @"(?<=[\n])");

        // checks for the first 2 messages from the server, being the PlayerID and WorldSize, saving them and removing them from parts[]
        if (Double.TryParse(parts[0], out double result))
        {
            PlayerID = (int)result;
            World.WorldSize = (float)Double.Parse(parts[1]);
            parts = parts.Skip(2).ToArray<string>();
        }


        // code for deserializing Walls, snakes, and powerups and adding them to the world state
        // locked when the current WorldState is being drawn
        lock (World.WorldState)
        {
            // clears the list of powerUps in the world so that their new positions can be drawn
            World.RemovePowerups();

            // loop to iterate through each line of data to be deserialized in parts[]
            foreach (string p in parts)
            {
                // if statements to clear out empty strings and a final new line that sometimes get made by the regex
                if (p.Length == 0)
                    continue;
                if (p[p.Length - 1] != '\n')
                    break;

                // Deserialize lines of data from the server and add them to the World
                if (p[2] == 'w')
                {
                    World.addWall(JsonConvert.DeserializeObject<Wall>(p)!);
                }
                if (p[2] == 's')
                {
                    World.addSnake(JsonConvert.DeserializeObject<Snake>(p)!);
                }
                if (p[2] == 'p')
                {
                    World.addPowerup(JsonConvert.DeserializeObject<Powerup>(p)!);
                }

                // clear the buffer 
                state.RemoveData(0, p.Length);
            }
        }

        // Event so the View knows to update
        if(UpdateFrame != null)
            UpdateFrame();

    }
}