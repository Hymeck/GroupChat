using System.Net;
using static System.Console;

namespace GroupChat.Client.Console
{
    class Program
    {
        /// <summary>
        /// Port used for network participants to communicate.
        /// </summary>
        public static readonly int CommonPort = 9000;
        
        /// <summary>
        /// Port used for group participants to communicate.
        /// </summary>
        public static readonly int GroupPort = 9100;

        public static bool _stop;

        private static PeerceClient peerceClient;
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }
            
            var username = args[0];
            peerceClient = new PeerceClient(username);
            peerceClient.GroupJoinRequestReceived += OnGroupJoinRequestReceived;
            peerceClient.GroupMessageReceived += OnGroupMessageReceived;
            
            // join case
            if (args.Length == 3)
            {
                var groupId = args[2];
                peerceClient.JoinGroup(groupId);
            }
            
            // create case
            else if (args.Length == 4)
            {
                var groupId = args[2];
                var multicastIpAddress = IPAddress.Parse(args[3]);
                peerceClient.CreateGroup(groupId, multicastIpAddress);
            }
            
            TreatControlCAsInput = false; // otherwise the system will handle CTRL+C for us
            CancelKeyPress += (_, _) =>
            {
                WriteLine("Bye.");
                _stop = false;
            };
            
            while (true)
            {
                var text = ReadLine();
                
                if (_stop)
                    break;
                
                peerceClient.SendMessage(text);
            }
            
            peerceClient.Finish();
        }

        private static void OnGroupMessageReceived(object sender, GroupMessageEventArgs e)
        {
            WriteLine(e);
        }

        private static void OnGroupJoinRequestReceived(object sender, GroupJoinRequestEventArgs e)
        {
            WriteLine(e.ToString());
            WriteLine("Accept or deny? ([Y/n])");

            var answer = ReadLine()?.ToUpper();

            // todo: fuck, i don't come to these lines
            if (answer == "Y" || answer == "YES") 
                peerceClient.Accept();
            
        }

        private static void PrintHelp()
        {
            WriteLine("name [command] <options>");
            WriteLine("name - username which will displayed for other network participants");
            WriteLine("command - command to use");
            
            WriteLine("\tcreate [id] [addr]");
            WriteLine("\t\tid - unique group ID");
            WriteLine("\t\taddr - multicast IP address associated with group");
            
            WriteLine("\tjoin [id]");
            WriteLine("\t\tid - unique group ID");
        }
    }
}