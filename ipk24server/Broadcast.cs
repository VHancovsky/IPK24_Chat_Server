using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ipk24server
{
    internal class Broadcast
    {
        public static void BroadcastToChannel(string channelName, string message, string dontSendToUser = "")
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // convert string message to byte array

            foreach (JoinedClient client in JoinedClient.allJoinedClientsOnServer)
            {
                if (client.JoinedChannelName == channelName && client.DisplayName != dontSendToUser)
                {
                    // send msg to each user in channel in bytes
                    client.ClientSocket.Send(messageBytes);
                }
            }
        }
    }
}
