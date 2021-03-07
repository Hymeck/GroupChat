using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace GroupChat.Client.Console
{
    class Program
    {
        private static PeerceClient _peerceClient;
        private static readonly CancellationTokenSource cts = new();

#pragma warning disable 1998
        static async Task Main(string[] args)
#pragma warning restore 1998
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            var username = args[0];
            _peerceClient = new PeerceClient(username);
            _peerceClient.StartBroadcastReceiving(cts.Token);

            // join case
            if (args.Length == 3)
            {
                var groupId = args[2];
            }

            // create case
            else if (args.Length == 4)
            {
                var groupId = args[2];
                var multicastIp = IPAddress.Parse(args[3]);
                _peerceClient.CreateGroup(groupId, multicastIp);
            }
        }

        private static void OnGroupMessageReceived(object sender, GroupMessageEventArgs e)
        {
            WriteLine("===");
            WriteLine($"** {e.GroupId} **");
            WriteLine($" {e.Username}");
            WriteLine($" {e.Text}\n");
            WriteLine($"\t{e.SentAt.ToShortTimeString()}");
            WriteLine("===\n");
        }

        private static void PrintHelp()
        {
            WriteLine("name [command] <options>");
            WriteLine("name - username which will be displayed for other network participants");
            WriteLine("command - command to use");

            WriteLine("\tcreate [id] [addr]");
            WriteLine("\t\tid - unique group ID");
            WriteLine("\t\taddr - multicast IP address associated with group");

            WriteLine("\tjoin [id]");
            WriteLine("\t\tid - unique group ID");
        }
    }
}