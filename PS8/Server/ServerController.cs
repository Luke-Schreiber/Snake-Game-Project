using NetworkUtil;
using System.Net.WebSockets;

namespace SnakeGame
{
    class Server
    {
        private Dictionary<long, SocketState> clients;
        private static World serverWorld;

        public static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer();
        }

        public Server()
        {
            clients = new Dictionary<long, SocketState>();
        }

        public void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewPlayerConnected, 11000);

            Console.WriteLine("Server is running");
        }

        private void NewPlayerConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
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
            state.GetData
        }


    }
}