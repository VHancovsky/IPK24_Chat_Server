using System.Net.Sockets;
using System.Net;
using System.Text;
using ipk24server;

// parse arguments using the Flags class
Flags flags = new Flags(args);

// access parsed values
IPAddress serverAddress = flags.ServerAddress;
ushort serverPort = flags.ServerPort;
ushort? confTimeout = flags.UdpConfTimeout;
byte? maxTrans = flags.UdpMaxTrans;

// TCP connect
Socket recievingSocket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
IPEndPoint localEndPoint = new IPEndPoint(serverAddress, serverPort);

// socket listening for new clients
recievingSocket.Bind(localEndPoint);
recievingSocket.Listen(10);

// main communication loop
while (true)
{
    Socket clientSocket = recievingSocket.Accept();

    // get the IP address and port of the connected client
    string clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
    int clientPort = ((IPEndPoint)clientSocket.RemoteEndPoint).Port;

    // fork processes for multiple clients
    Task clientProcess = Task.Run(async delegate {
        using (clientSocket)
        {
            // reuses recently freed socket
            clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // set the current channel to 'default' and append it to the list of channels
            string currChannel = "default";
            ClientMessage.listOfChannels.Add(currChannel);

            string[] messageParts;
            string message;

            try
            {
                string userName;
                string displayName;
                string secret;

                // ACCEPT STATE LOOP
                while (true)
                {
                    byte[] receivedBytes = new byte[1500];
                    int readBytes;

                    string incompleteMessage = "";
                    StringBuilder incompleteMessageBuilder = new StringBuilder();

                    // RECIEVE until user prints "\r\n" after AUTH
                    while (true)
                    {
                        readBytes = clientSocket.Receive(receivedBytes);
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, readBytes);

                        incompleteMessageBuilder.Append(receivedMessage);

                        incompleteMessage = incompleteMessageBuilder.ToString();

                        // loops inside this while loop until it gets the whole message from user
                        if (incompleteMessage.EndsWith("\r\n"))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // removes '\r\n' from the message
                    message = incompleteMessage;
                    message = message.Trim();

                    // splits the message to separate parts
                    messageParts = message.Split(' ');

                    userName = messageParts[1];
                    displayName = messageParts[3];
                    secret = messageParts[5];

                    // checks if the first string from user message is one of the valid message types
                    if (messageParts[0].ToUpper() == "AUTH")
                    {
                        // validate AUTH message syntax + list of already authorized usernames
                        if (ClientMessage.ValidateAuth(userName, displayName, secret))
                        {
                            if (!JoinedClient.UserNameInList(userName))
                            {   
                                // username not taken
                                Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());

                                string replyMessage = "REPLY OK IS Auth success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");
                                break;
                            }
                            else
                            {
                                // username taken
                                string replyMessage = "REPLY NOK IS Auth not success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                            }
                        }
                        else
                        {
                            // incorrect AUTH message syntax
                            string replyMessage = "REPLY NOK IS Auth not success\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                            clientSocket.Send(replyData);

                            Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                        }
                    }
                    else
                    {
                        // server recieved anything else than AUTH message
                        string replyMessageERR = "ERR Wrong message type\r\n";
                        byte[] replyDataERR = Encoding.UTF8.GetBytes(replyMessageERR);
                        clientSocket.Send(replyDataERR);

                        string replyMessageBYE = "BYE\r\n";
                        byte[] replyDataBYE = Encoding.UTF8.GetBytes(replyMessageBYE);
                        clientSocket.Send(replyDataBYE);

                        // close the socket
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                    }
                }

                JoinedClient client = new JoinedClient(userName, displayName, currChannel, clientSocket);
                // adding to a static variable
                JoinedClient.allJoinedClientsOnServer.Add(client);

                // broadcast to all users in default channel about new connected user
                Broadcast.BroadcastToChannel(currChannel, "MSG FROM Server IS " + displayName + " has joined " + currChannel + ".\n");

                // OPEN STATE LOOP
                while (true)
                {
                    // msg recieve from client
                    byte[] receivedBytes = new byte[1500];
                    int readBytes;

                    string incompleteMessage = "";
                    StringBuilder incompleteMessageBuilder = new StringBuilder();

                    // RECIEVE until user prints "\r\n" after one of the message types ('JOIN', 'MSG' or 'BYE')
                    while (true)
                    {
                        readBytes = clientSocket.Receive(receivedBytes);
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, readBytes);

                        incompleteMessageBuilder.Append(receivedMessage);

                        incompleteMessage = incompleteMessageBuilder.ToString();

                        // loops inside this while loop until it gets the whole message from user
                        if (incompleteMessage.EndsWith("\r\n"))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    message = incompleteMessage;
                    message = message.Trim();

                    // spliting into parts
                    messageParts = message.Split(' ').Select(p => p.Trim()).ToArray();

                    // check if the user message is valid MSG, JOIN, BYE or ERR command
                    if (messageParts[0].ToUpper() == "MSG" && messageParts.Length >= 4)
                    {
                        displayName = messageParts[2];
                        string msgText = string.Join(" ", messageParts.Skip(4));

                        // check if the MSG message is valid 
                        if (ClientMessage.ValidateMsg(displayName, msgText))
                        {
                            client.DisplayName = displayName;
                            currChannel = client.JoinedChannelName;
                            Broadcast.BroadcastToChannel(currChannel, "MSG FROM " + client.DisplayName + " IS " + msgText + "\n", userName);

                            Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());
                        }
                        else
                        {
                            // if not, server replies not ok
                            string replyMessage = "REPLY NOK IS Msg not success\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                            clientSocket.Send(replyData);

                            Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                        }
                    }
                    else if (messageParts[0].ToUpper() == "JOIN" && messageParts.Length == 4)
                    {
                        string channelID = messageParts[1];
                        displayName = messageParts[3];

                        // store the original channel
                        currChannel = client.JoinedChannelName;

                        // check if the JOIN message is valid
                        if (ClientMessage.ValidateJoin(displayName, channelID, client))
                        {
                            // print to all remaining users in original channel that user has left it
                            Broadcast.BroadcastToChannel(currChannel, "MSG FROM Server IS " + displayName + " has left " + currChannel + ".\n", userName);

                            // if the channelID is not in the list of channels, it means that the channel doesn't exist, server creates it and user joins it.
                            // if it already exists, user joins the channel
                            if (ClientMessage.ChannelInList(channelID, ClientMessage.listOfChannels))
                            {
                                Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());

                                client.JoinedChannelName = channelID;
                                Broadcast.BroadcastToChannel(channelID, "MSG FROM Server IS " + displayName + " has joined " + channelID + ".\n");

                                string replyMessage = "REPLY OK IS Join success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");
                            }
                            else
                            {
                                Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());

                                ClientMessage.listOfChannels.Add(channelID);
                                client.JoinedChannelName = channelID;
                                Broadcast.BroadcastToChannel(channelID, "MSG FROM Server IS " + displayName + " has joined " + channelID + ".\n");

                                string replyMessage = "REPLY OK IS Join success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");
                            }
                        }
                        else
                        {
                            // if not, server replies not ok
                            string replyMessage = "REPLY NOK IS Join not success\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                            clientSocket.Send(replyData);

                            Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                        }
                    }
                    else if (messageParts[0].ToUpper() == "BYE" || messageParts[0].ToUpper() == "ERR")
                    {
                        Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());

                        // Broadcast that the client is leaving the channel
                        Broadcast.BroadcastToChannel(currChannel, "MSG FROM Server IS " + displayName + " has left the channel.\n", userName);

                        // Remove the client from the list of joined clients
                        JoinedClient.allJoinedClientsOnServer.Remove(client);

                        // Close the socket
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        break;
                    }
                    else
                    {
                        // anything else than valid message types is error
                        string replyMessage = "ERR Wrong message type\r\n";
                        byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                        clientSocket.Send(replyData);

                        string replyMessageBYE = "BYE\r\n";
                        byte[] replyDataBYE = Encoding.UTF8.GetBytes(replyMessageBYE);
                        clientSocket.Send(replyDataBYE);

                        // Close the socket
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        break;
                    }
                }
            }
            finally
            {
                // removes user from the list of clients on server
                JoinedClient.allJoinedClientsOnServer.Clear();
                // cleanup client socket
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
        }
    });
}