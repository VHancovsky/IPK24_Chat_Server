using System.Net;

namespace ipk24server
{
    internal class Flags
    {
        public IPAddress ServerAddress { get; private set; }
        public ushort ServerPort { get; private set; }
        public ushort? UdpConfTimeout { get; private set; }
        public byte? UdpMaxTrans { get; private set; }

        public Flags(string[] args)
        {
            ParseFlags(args);
        }

        // parses each flag and it's value
        private void ParseFlags(string[] args)
        {
            string argAddress = null;
            string argPort = null;
            string udpConfTimeout = null;
            string udpMaxTrans = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h")
                {
                    Console.WriteLine("Help argument was called");
                    Environment.Exit(0);
                }
                else
                {
                    if (args[i] == "-l" && i + 1 < args.Length)
                    {
                        argAddress = args[i + 1];
                        i++;
                    }
                    if (args[i] == "-p" && i + 1 < args.Length)
                    {
                        argPort = args[i + 1];
                        i++;
                    }
                    if (args[i] == "-d" && i + 1 < args.Length)
                    {
                        udpConfTimeout = args[i + 1];
                        i++;
                    }
                    if (args[i] == "-r" && i + 1 < args.Length)
                    {
                        udpMaxTrans = args[i + 1];
                        i++;
                    }
                }
            }

            if (argAddress == null || argPort == null)
            {
                Console.WriteLine("Mandatory arguments -l and -p are missing.");
                Environment.Exit(1);
            }

            ServerAddress = IPAddress.Parse(argAddress);
            ServerPort = ushort.Parse(argPort);

            if (udpConfTimeout != null)
            {
                UdpConfTimeout = ushort.Parse(udpConfTimeout);
            }
            if (udpMaxTrans != null)
            {
                UdpMaxTrans = byte.Parse(udpMaxTrans);
            }
        }
    }
}
