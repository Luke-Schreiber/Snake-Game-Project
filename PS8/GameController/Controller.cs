using NetworkUtil;
using System.Net.Security;

namespace SnakeGame;
public static class Controller
{
    private static SocketState Server;
    private static string PlayerName;

    /*
    public static void main(String[] args)
    {
        Controller controller = new Controller();
        controller.Connect("localhost");

        string userName = Console.ReadLine();
        Networking.Send(controller.Server.TheSocket, userName);
    }
    */

    public static void Connect(string serverAddress, string playerName)
    {
        Networking.ConnectToServer(OnConnect, serverAddress, 11000);
        PlayerName = playerName;
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
        string test = state.GetData();

        //continues loop
        Networking.GetData(state);
    }
}