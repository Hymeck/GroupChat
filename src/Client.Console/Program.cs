using System;
using System.Net;
using GroupChat.Extensions;
using GroupChat.Implementations;
using GroupChat.Implementations.Dtos;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    class Program
    {
        private static bool _stop;
        
        /// <summary>
        /// Port used for network participants to communicate.
        /// </summary>
        public static readonly int CommonPort = 9000;
        
        /// <summary>
        /// Port used for group participants to communicate.
        /// </summary>
        public static readonly int ChatPort = 9100;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args.Length == 2)
                {
                    // StartChatMessaging(args[0], args[1]);
                    StartChatMessaging2(args[0], args[1]);
                }
                
                else
                    PrintSettings();
            }
            
            else
                PrintSettings();
        }

        private static void StartChatMessaging2(string username, string groupId)
        {
            System.Console.TreatControlCAsInput = false; // otherwise the system will handle CTRL+C for us
            System.Console.CancelKeyPress += OnCancelKeyPress;

            var create = username == "hymeck1" ? true : false;
            var groupParticipant = new GroupParticipant(username, CommonPort, ChatPort);
            
            if (create) 
                groupParticipant.CreateGroup(groupId);

            groupParticipant.MessageReceived += OnGroupMessageReceived;
            
            System.Console.WriteLine($"== Group '{groupId}' ==");
            while (true)
            {
                var text = System.Console.ReadLine();
                
                if (_stop)
                    break;
                
                var message = new Message(username, text, DateTime.Now);
                
                groupParticipant.SendMessage(message);
            }
            
        }

        private static void OnGroupMessageReceived(object sender, MessageEventArgs<Message> e)
        {
            System.Console.WriteLine(e.Message);
        }

        private static void StartChatMessaging(string username, string chatId)
        {
            System.Console.TreatControlCAsInput = false; // otherwise the system will handle CTRL+C for us
            System.Console.CancelKeyPress += OnCancelKeyPress;
            
            var chat = new MulticastUdpClient(IPAddress.Parse("224.0.0.1"), CommonPort);
            chat.DatagramReceived += OnDatagramReceived;
            chat.BeginReceive();
            
            System.Console.WriteLine($"== Chat '{chatId}' ==");
            while (true)
            {
                var text = System.Console.ReadLine();
                
                if (_stop)
                    break;
                
                var message = new Message(username, text, DateTime.Now);
                chat.Send(message.XmlSerialize());
            }
            
            chat.Send(new Message(username, "Bye-bye", DateTime.Now).XmlSerialize());
            chat.Close();
        }
        
        // handle ctrl+c
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // set the Cancel property to true to prevent the process from terminating.
            try
            {
                System.Console.WriteLine("Bye.");
            }
            catch
            {
                // ignored
            }

            _stop = false;
        }

        private static void OnDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var message = e.Datagram.XmlDeserialize<Message>();
            System.Console.WriteLine(message);
        }

        private static void PrintSettings() => System.Console.WriteLine(
                                                                        "username - username to display\n" +
                                                                        "chatId - chat id to join");
        
        private static readonly string ChatId = "hymeck group";
    }
}