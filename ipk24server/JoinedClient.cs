using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;

namespace ipk24server
{
    internal class JoinedClient
    {
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
