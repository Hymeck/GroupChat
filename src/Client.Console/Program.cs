using System;
using System.Net;
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

                _peerceClient.JoinGroup(groupId, cts.Token).Wait();

                if (_peerceClient.IsGroupParticipant)
                    _peerceClient.GroupMessageReceived += OnGroupMessageReceived;
            }

            // create case
            else if (args.Length == 4)
            {
                var groupId = args[2];
                var multicastIp = IPAddress.Parse(args[3]);

                _peerceClient.GroupMessageReceived += OnGroupMessageReceived;
                _peerceClient.CreateGroup(groupId, multicastIp);
            }

            TreatControlCAsInput = true;
            CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Task.Run(() =>
            {
                if (!_peerceClient.IsGroupParticipant)
                    return;

                while (true)
                {
                    var text = ReadLine();

                    _peerceClient.SendMessage(text);

                    _peerceClient.ProcessGroupJoinRequests(RequestChoice)
                        .Wait();
                }
            }, cts.Token).Wait();

            WriteLine("Bye.");
            _peerceClient.Close();
        }

        private static void PrintJoinRequestData(GroupJoinRequest gjr, IPEndPoint from)
        {
            WriteLine($"+++{Environment.NewLine}" +
                      $"{gjr.GroupId}{Environment.NewLine}" +
                      $"{gjr.Username}{Environment.NewLine}" +
                      $"{from.Address}{Environment.NewLine}" +
                      $"\t{gjr.SentAt}{Environment.NewLine}" +
                      $"+++{Environment.NewLine}" +
                      "Accept or deny? ([Y/n])");
        }

        private static bool RequestChoice(GroupJoinRequest joinRequest, IPEndPoint from)
        {
            PrintJoinRequestData(joinRequest, from);

            var answer = ReadLine()?.ToUpper();
            return answer == "Y" || answer == "YES";
        }

        private static void OnGroupMessageReceived(object sender, GroupMessageEventArgs e)
        {
            WriteLine($"==={Environment.NewLine}" +
                      $"** {e.GroupId} **{Environment.NewLine}" +
                      $"{e.Username}{Environment.NewLine}" +
                      $"{e.Text}{Environment.NewLine}" +
                      $"\t{e.SentAt.ToShortTimeString()}{Environment.NewLine}" +
                      $"==={Environment.NewLine}");
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