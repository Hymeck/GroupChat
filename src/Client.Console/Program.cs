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
        
        private static readonly CancellationTokenSource Cts = new();
        
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }
            
            var username = args[0];
            _peerceClient = new PeerceClient(username);
            _peerceClient.GroupJoinRequestReceived += OnGroupJoinRequestReceived;
            _peerceClient.GroupMessageReceived += OnGroupMessageReceived;
            
            // join case
            if (args.Length == 3)
            {
                var groupId = args[2];
                _peerceClient.JoinGroup(groupId);
            }
            
            // create case
            else if (args.Length == 4)
            {
                var groupId = args[2];
                var multicastIpAddress = IPAddress.Parse(args[3]);
                _peerceClient.CreateGroup(groupId, multicastIpAddress);
            }
            
            // TreatControlCAsInput = true; // otherwise the system will handle CTRL+C for us
            CancelKeyPress += (_, e) =>
            {
                WriteLine("Cancelling...");
                Cts.Cancel();
                e.Cancel = true;
            };
            
            var mainTask = Task.Run(MessagingLoop, Cts.Token);

            try
            {
                await mainTask;
            }

            catch (OperationCanceledException)
            {
                WriteLine("Bye.");
                _peerceClient.Finish();
            }
        }

        private static async Task MessagingLoop()
        {
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
            }
        }
        
        private static async Task<string> ReadLineAsync() => await Task.Run(ReadLine, Cts.Token); 

        private static void OnGroupMessageReceived(object sender, GroupMessageEventArgs e)
        {
            WriteLine(e);
        }

        private static void OnGroupJoinRequestReceived(object sender, GroupJoinRequestEventArgs e)
        {
            WriteLine(e.ToString());
            WriteLine("Accept or deny? ([Y/n])");

            var answer = ReadLineAsync().Result;
            // todo: fuck, i don't come to these lines
            if (answer == "Y" || answer == "YES") 
                _peerceClient.Accept();
            
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