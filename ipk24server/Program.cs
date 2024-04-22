using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using ipk24server;

// Parse command - line arguments using the Flags class
Flags flags = new Flags(args);

// Access parsed values
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

    // Get the IP address and port of the connected client
    string clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
    int clientPort = ((IPEndPoint)clientSocket.RemoteEndPoint).Port;

    // fork processes for multiple clients
    Task t = Task.Run(async delegate {
        using (clientSocket)
        {
            clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

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

                        receivedMessage = receivedMessage.Substring(0, readBytes - 2);

                        incompleteMessageBuilder.Append(receivedMessage);

                        incompleteMessage = incompleteMessageBuilder.ToString();

                        if (incompleteMessage.EndsWith("\\r\\n"))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    message = incompleteMessage;
                    message = message.Substring(0, incompleteMessage.Length - 4);

                    messageParts = message.Split(' ');

                    userName = messageParts[1];
                    displayName = messageParts[3];
                    secret = messageParts[5];

                    if (messageParts[0].ToUpper() == "AUTH")
                    {
                        // validate AUTH message syntax + list of already authorized usernames
                        if (ClientMessage.ValidateAuth(userName, displayName, secret))
                        {
                            if (!JoinedClient.UserNameInList(userName))
                            {
                                Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());

                                string replyMessage = "REPLY OK IS Auth success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");
                                break;
                            }
                            else
                            {
                                // USERNAME TAKEN
                                string replyMessage = "REPLY NOK IS Auth not success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                                clientSocket.Send(replyData);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                            }
                        }
                        else
                        {
                            // AUTH INCORRECT SYNTAX
                            string replyMessage = "REPLY NOK IS Auth not success\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                            clientSocket.Send(replyData);

                            Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | !REPLY");
                        }
                    }
                    else
                    {
                        string replyMessageERR = "ERR Wrong message type\r\n";
                        byte[] replyDataERR = Encoding.UTF8.GetBytes(replyMessageERR);
                        clientSocket.Send(replyDataERR);

                        string replyMessageBYE = "BYE\r\n";
                        byte[] replyDataBYE = Encoding.UTF8.GetBytes(replyMessageBYE);
                        clientSocket.Send(replyDataBYE);

                        // Close the socket
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

                        receivedMessage = receivedMessage.Substring(0, readBytes - 2);

                        incompleteMessageBuilder.Append(receivedMessage);

                        incompleteMessage = incompleteMessageBuilder.ToString();

                        if (incompleteMessage.EndsWith("\\r\\n"))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    message = incompleteMessage;
                    message = message.Substring(0, incompleteMessage.Length - 4);

                    // spliting into parts
                    messageParts = message.Split(' ').Select(p => p.Trim()).ToArray();

                    // check is message is valid MSG command
                    if (messageParts[0].ToUpper() == "MSG" && messageParts.Length >= 4)
                    {
                        displayName = messageParts[2];
                        string msgText = string.Join(" ", messageParts.Skip(4));

                        if (ClientMessage.ValidateMsg(displayName, msgText, client))
                        {
                            currChannel = client.JoinedChannelName;
                            Broadcast.BroadcastToChannel(currChannel, "MSG FROM " + displayName + " IS " + msgText + "\n", displayName);

                            Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0].ToUpper());
                        }
                        else
                        {
                            string replyMessage = "ERR Invalid display name or message format\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);
                            clientSocket.Send(replyData);
                        }
                    }
                    else if (messageParts[0].ToUpper() == "JOIN" && messageParts.Length == 4)
                    {
                        string channelID = messageParts[1];
                        displayName = messageParts[3];

                        currChannel = client.JoinedChannelName;

                        if (ClientMessage.ValidateJoin(displayName, channelID, client))
                        {
                            Broadcast.BroadcastToChannel(currChannel, "MSG FROM Server IS " + displayName + " has left " + currChannel + ".\n", displayName);
                            if (ClientMessage.ChannelInList(channelID, ClientMessage.listOfChannels))
                            {
                                Console.WriteLine("RECV " + serverAddress + ":" + serverPort + " | " + messageParts[0]);

                                client.JoinedChannelName = channelID;
                                Broadcast.BroadcastToChannel(channelID, "MSG FROM Server IS " + displayName + " has joined " + channelID + ".");

                                string replyMessage = "REPLY OK IS Join success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");

                                clientSocket.Send(replyData);
                            }
                            else
                            {
                                Console.WriteLine("RECV " + clientIP + ":" + clientPort + " | " + messageParts[0]);

                                ClientMessage.listOfChannels.Add(channelID);
                                client.JoinedChannelName = channelID;
                                Broadcast.BroadcastToChannel(channelID, "MSG FROM Server IS " + displayName + " has joined " + channelID + ".");

                                string replyMessage = "REPLY OK IS Join success\r\n";
                                byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);

                                Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");

                                clientSocket.Send(replyData);
                            }
                        }
                        else
                        {
                            string replyMessage = "REPLY NOK IS Join not success\r\n";
                            byte[] replyData = Encoding.UTF8.GetBytes(replyMessage);

                            Console.WriteLine("SENT " + clientIP + ":" + clientPort + " | REPLY");

                            clientSocket.Send(replyData);
                        }
                    }
                    else if (messageParts[0].ToUpper() == "BYE")
                    {
                        Console.WriteLine("RECV " + serverAddress + ":" + serverPort + " | " + messageParts[0]);

                        // Broadcast that the client is leaving the channel
                        Broadcast.BroadcastToChannel(currChannel, "MSG FROM Server IS " + displayName + " has left the channel.\n", displayName);

                        // Remove the client from the list of joined clients
                        JoinedClient.allJoinedClientsOnServer.Remove(client);

                        // Close the socket
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        break;
                    }
                    else
                    {
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
                JoinedClient.allJoinedClientsOnServer.Clear();
                // cleanup client socket
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
        }
    });
}