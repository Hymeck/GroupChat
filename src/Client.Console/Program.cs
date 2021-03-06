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

        private static readonly CancellationTokenSource Cts = new();

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            // TreatControlCAsInput = true; // otherwise the system will handle CTRL+C for us
            CancelKeyPress += (_, e) =>
            {
                WriteLine("Cancelling...");
                Cts.Cancel();
                e.Cancel = true;
            };

            var username = args[0];
            _peerceClient = new PeerceClient(username);
            _peerceClient.GroupJoinRequestReceived += OnGroupJoinRequestReceived;
            _peerceClient.GroupMessageReceived += OnGroupMessageReceived;

            // join case
            if (args.Length == 3)
            {
                var groupId = args[2];
                join:
                try
                {
                    WriteLine("Waiting for response.");

                    _peerceClient.JoinGroup(groupId);
                }

                catch (SocketException)
                {
                    WriteLine("No response. Try again? ([Y/n])");
                    var answer = ReadLine()?.ToUpper();

                    if (answer == "Y" || answer == "YES")
                        goto join;
                    
                }

                try
                {
                    Task.Run(MessagingLoop, Cts.Token).Wait(Cts.Token);
                }

                catch (OperationCanceledException)
                {
                    WriteLine("Bye.");
                    _peerceClient.Finish();
                }
            }

            // create case
            else if (args.Length == 4)
            {
                var groupId = args[2];
                var multicastIpAddress = IPAddress.Parse(args[3]);
                _peerceClient.CreateGroup(groupId, multicastIpAddress);

                try
                {
                    Task.Run(MessagingLoop, Cts.Token).Wait(Cts.Token);
                }

                catch (OperationCanceledException)
                {
                    WriteLine("Bye.");
                    _peerceClient.Finish();
                }
            }
        }

        private static async Task MessagingLoop()
        {
            if (!_peerceClient.IsGroupParticipant)
            {
                WriteLine("You are not participant of any group.");
                return;
            }

            while (true)
            {
                // var text = ReadLine();
                var text = await ReadLineAsync();

                if (Cts.Token.IsCancellationRequested)
                {
                    Cts.Token.ThrowIfCancellationRequested();
                    break;
                }

                _peerceClient.SendMessage(text);

                // handle request queue
                _peerceClient.HandleJoinRequestQueue();
            }
        }

        private static async Task<string> ReadLineAsync() => await Task.Run(ReadLine, Cts.Token);

        private static void OnGroupMessageReceived(object sender, GroupMessageEventArgs e)
        {
            WriteLine("===");
            WriteLine($"** {e.GroupId} **");
            WriteLine($" {e.Username}");
            WriteLine($" {e.Text}\n");
            WriteLine($"\t{e.SentAt.ToShortTimeString()}");
            WriteLine("===\n");
        }

        private static void OnGroupJoinRequestReceived(object sender, GroupJoinRequestEventArgs e)
        {
            WriteLine("+++");
            WriteLine($"{e.GroupId}");
            WriteLine($"{e.Username}");
            WriteLine($"{e.From.Address}");
            WriteLine($"\t{e.RequestedAt}");
            WriteLine("+++");
            WriteLine("Accept or deny? ([Y/n])");
            WriteLine(e.ToString());

            var answer = ReadLineAsync().Result.ToUpper();
            // todo: fuck, i don't come to these lines
            if (answer == "Y" || answer == "YES")
                _peerceClient.Accept(e);
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