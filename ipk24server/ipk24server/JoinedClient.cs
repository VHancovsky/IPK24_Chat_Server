using System.Net.Sockets;

namespace ipk24server
{
    internal class JoinedClient
    {
        // list of all joined clients on server
        public static List<JoinedClient> allJoinedClientsOnServer = new List<JoinedClient>();

        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string JoinedChannelName { get; set; }
        public Socket ClientSocket { get; set; }

        public JoinedClient(string userName, string displayName, string joinedChannelName, Socket clientSocket)
        {
            this.UserName = userName;
            this.DisplayName = displayName;
            this.JoinedChannelName = joinedChannelName;
            this.ClientSocket = clientSocket;
        }

        // method to check if the username is taken or not
        public static bool UserNameInList(string userName)
        {
            foreach (JoinedClient client in allJoinedClientsOnServer)
            {
                if (client.UserName == userName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
