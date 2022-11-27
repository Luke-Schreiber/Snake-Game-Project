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
    private static double PlayerID;
    public static event Action? UpdateFrame;

    public static void Connect(string serverAddress, string playerName)
    {
        Networking.ConnectToServer(OnConnect, serverAddress, 11000);
        PlayerName = playerName;
    }

    public static void MoveUp()
    {
        if(Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"up\"}\n");
    }

    public static void MoveDown()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"down\"}\n");
    }

    public static void MoveLeft()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"left\"}\n");
    }

    public static void MoveRight()
    {
        if (Server != null)
            Networking.Send(Server.TheSocket, "{\"moving\":\"right\"}\n");
    }

    private static void OnConnect(SocketState state)
    {
        Server = state;
        state.OnNetworkAction = ReceiveData;
        Networking.Send(Server.TheSocket, PlayerName);

    }

    private static void ReceiveData(SocketState state)
    {
        // call method for deserialize json
        ProcessData(state);

        //continues loop
        Networking.GetData(state);
    }

    private static void ProcessData(SocketState state)
    {
        string data = state.GetData();
        string[] parts = Regex.Split(data, @"(?<=[\n])");

        if (Double.TryParse(parts[0], out double result))
        {
            PlayerID = result;
            World.setSize(Double.Parse(parts[1]));
        }

        foreach (string p in parts)
        {
            if (p.Length == 0)
                continue;
            if (p[p.Length - 1] != '\n')
                break;

            // Deserialize
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
            state.RemoveData(0, p.Length);
        }

        if(UpdateFrame != null)
            UpdateFrame();

    }
}