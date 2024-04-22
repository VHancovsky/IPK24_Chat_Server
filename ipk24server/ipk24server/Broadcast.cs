using System.Text;

namespace ipk24server
{
    internal class Broadcast
    {
        // method for general communication in the channel between users, also notifies 
        public static void BroadcastToChannel(string channelName, string message, string dontSendToUser = "")
        {
            // convert string message to byte array
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            foreach (JoinedClient client in JoinedClient.allJoinedClientsOnServer)
            {
                if (client.JoinedChannelName == channelName && client.UserName != dontSendToUser)
                {
                    // send msg to each user in channel in bytes
                    client.ClientSocket.Send(messageBytes);
                }
            }
        }
    }
}
