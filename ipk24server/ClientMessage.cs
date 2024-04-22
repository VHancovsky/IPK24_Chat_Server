using System;
using System.Text.RegularExpressions;

namespace ipk24server
{
    internal class ClientMessage
    {
        // List to store the names of all channels
        public static List<string> listOfChannels = new List<string>();

        private static string userNameOrChannelRegex = "^[a-zA-Z0-9-]{1,20}$";
        private static string displayNameRegex = "^[\x21-\x7E]{1,20}$";
        private static string secretRegex = "^[a-zA-Z0-9-]{1,128}$";
        private static string messageRegex = "^[\x20-\x7E]{0,1400}$";

        public static bool ValidateAuth(string userName, string displayName, string secret)
        {
            bool userNameValid = Regex.IsMatch(userName, userNameOrChannelRegex);
            bool displayNameValid = Regex.IsMatch(displayName, displayNameRegex);
            bool secretValid = Regex.IsMatch(secret, secretRegex);

            // return true only if all fields are valid
            return userNameValid && displayNameValid && secretValid;
        }

        public static bool ValidateMsg(string displayName, string messageContent, JoinedClient client)
        {
            bool displayNameValid = Regex.IsMatch(displayName, displayNameRegex);
            bool messageValid = Regex.IsMatch(messageContent, messageRegex);

            // return true only if all fields are valid
            return displayNameValid && messageValid && client.DisplayName == displayName;
        }

        public static bool ValidateJoin(string displayName, string channelName, JoinedClient client)
        {
            bool displayNameValid = Regex.IsMatch(displayName, displayNameRegex);
            bool channelValid = Regex.IsMatch(channelName, userNameOrChannelRegex);

            return displayNameValid && channelValid && client.DisplayName == displayName;
        }

        public static bool ChannelInList(string channel, List<string> listOfChannels)
        {
            return listOfChannels.Contains(channel);
        }
    }
}
