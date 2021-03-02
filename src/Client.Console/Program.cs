using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GroupChat.Client.Console
{
    class Program
    {
        public const int CommonPort = 9000;
        public const int ChatPort = 9001;
        private static readonly IPAddress MulticastIpAddress = IPAddress.Parse("230.0.0.1");
        
        // todo: double ports just for localhost. remove this later
        // public const int CommonPort2 = 9002;
        // public const int ChatPort2 = 9003;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args[0] == "-r")
                {
                    SendRequest("hymeck", ChatId);
                }
                
                else if (args[0] == "-c")
                {
                    CreateChat(ChatId);
                }
                
                else if (args[0] == "host")
                    PrintHostData();
                
                else if (args.Length == 2)
                {
                    StartChatMessaging(args[0], args[1]);
                }
                
                else
                    PrintSettings();
            }
            
            else
                PrintSettings();
        }

        private static void PrintHostData()
        {
            var hostname = Dns.GetHostName();
            System.Console.WriteLine($"Hostname: {hostname}");
            var hostEntry = Dns.GetHostEntry(hostname);
            System.Console.WriteLine("Address list:");
            System.Console.WriteLine(string.Join('\n', hostEntry.AddressList.AsEnumerable()));
        }
        
        private static bool _stop;
        private static void StartChatMessaging(string username, string chatId)
        {
            System.Console.TreatControlCAsInput = false; // otherwise the system will handle CTRL+C for us
            // occurs when user input <ctrl>+<C>
            System.Console.CancelKeyPress += OnCancelKeyPress;
            
            var chat = new ChatMulticastUdpClient(MulticastIpAddress, ChatPort);
            chat.DatagramReceived += OnDatagramReceived;
            chat.BeginReceive();
            
            System.Console.WriteLine($"== Chat '{chatId}' ==");
            while (true)
            {
                var text = System.Console.ReadLine();
                
                if (_stop)
                    break;
                
                var message = new Message(username, chatId, text, DateTime.Now);
                chat.SendMulticast(message.Serialize());
            }
            
            chat.SendMulticast(new Message(username, chatId, "Bye-bye", DateTime.Now).Serialize());
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
            var message = e.Datagram.Deserialize<Message>();
            System.Console.WriteLine(message);
        }

        private static void PrintSettings() => System.Console.WriteLine("-c - create chat\n" +
                                                                        "-r - request to chat\n" +
                                                                        "username chatId");

        private static readonly string ChatId = "hymeck group";

        // todo: remember to set BeginReceive callback
        private static UdpClient GetUdpClient(IPEndPoint localEndpoint)
        {
            var udpClient = new UdpClient {ExclusiveAddressUse = false};

            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;

            udpClient.Client.Bind(localEndpoint);
            
            return udpClient;
        }
        
        private static UdpClient GetUdpClient(IPEndPoint localEndpoint, IPAddress multicastIp)
        {
            var udpClient = GetUdpClient(localEndpoint);
            udpClient.JoinMulticastGroup(multicastIp, localEndpoint.Address);
            return udpClient;
        }
        
        private static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            
            throw 
                new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static readonly IPAddress LocalIpAddress = GetLocalIpAddress();
        
        public static void CreateChat(string chatId)
        {
            // var listener = new UdpClient(CommonPort2, AddressFamily.InterNetwork);
            // var localEndpoint = new IPEndPoint(LocalIpAddress, CommonPort);
            var localEndpoint = new IPEndPoint(IPAddress.Any, CommonPort);
            var listener = GetUdpClient(localEndpoint);
            // var chatListener = GetUdpClient(localEndpoint, MulticastIpAddress);
            IPEndPoint remoteEndpoint = null;
            System.Console.WriteLine($"=== Chat '{chatId}' is created ===");
            try
            {
                while (true)
                {
                    var requestData = listener.Receive(ref remoteEndpoint);
                    var request = requestData.Deserialize<GroupAccessRequest>();

                    System.Console.WriteLine($"{request.Username} requested to access group '{chatId}'");
                    // todo: confirm or deny

                    var response = new GroupAccessResponse { Result = GroupAccessResult.Allow};
                    if (response.IsAllowed())
                    {
                        // todo: if confirm send multicast message to group participants
                    }
                    
                    var serializedResponse = response.Serialize();
                    listener.Send(serializedResponse, serializedResponse.Length, remoteEndpoint);
                }
            }

            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            
            finally
            {
                listener.Close();
            }
        }

        public static void SendRequest(string username, string chatId)
        {
            // var ip = IPAddress.Loopback;
            var localEndpoint = new IPEndPoint(LocalIpAddress, CommonPort);
            // var requestListener = new UdpClient(CommonPort, AddressFamily.InterNetwork);
            var requestListener = GetUdpClient(localEndpoint);
            // var requestListener = new UdpClient();
            // requestListener.Client.Bind(new IPEndPoint(IPAddress.Any, CommonPort));
            try
            {
                var request = new GroupAccessRequest(chatId, username);
                var requestData = request.Serialize();
                // requestListener.Send(requestData, requestData.Length, ip.ToString(), CommonPort2);
                // requestListener.Send(requestData, requestData.Length, localEndpoint);
                requestListener.Send(requestData, requestData.Length);

                var remoteEndpoint = new IPEndPoint(0, 0);
                // var receiveThread = new Thread(_ =>
                // {
                    var response = requestListener.Receive(ref remoteEndpoint);
                    if (response.Length == 0)
                        return;
                    
                    var parsedResponse = response.Deserialize<GroupAccessResponse>();
                    System.Console.WriteLine($"Response: {parsedResponse}");
                // });
                //
                // receiveThread.Start();
            }

            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            
            finally
            {
                requestListener.Close();
            }
        }
    }
}