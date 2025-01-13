# TCP Chat Server

This project implements a TCP-based chat server that supports multiple clients through concurrent connections, providing basic chat server features, built using C# and leverages the Socket class for network communication.

## Commands
|Command|Description|
|:------|:----------|
|`AUTH <username> IS <display_name> WITH <password>`|Authenticates the client with a username, display name, and password.|
|`JOIN <channel_name> <display_name>`|Joins channel (creates and joins channel if it doesn't exists)|
|`MSG <display_name> IS <message>`|Broadcasts the message to all clients in the same channel|
|`BYE`|Disconnects client from the server|

## Example interaction
Client A
```bash
  AUTH userA IS User_A WITH passwordA123
  REPLY OK IS Auth success
  MSG FROM User A IS Hello, everyone!
  REPLY OK IS Message delivered
  BYE
  BYE
```

Client B
```bash
  AUTH userB IS User_B WITH passwordB123
  REPLY OK IS Auth success
  JOIN channel_3 IS User_B
  REPLY OK IS Join success
  MSG FROM User B IS Hi, User One!
  REPLY OK IS Message delivered
  BYE
  BYE
```
