using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Entry
{
    class Program
    {
        public const int CommonPort = 9000;
        public const int ChatPort = 9001;
        
        // todo: double ports just for localhost. remove this shit later
        public const int CommonPort2 = 9002;
        public const int ChatPort2 = 9003;
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
                
                else
                    PrintSettings();
            }
            
            else
                PrintSettings();
        }

        private static void PrintSettings() => Console.WriteLine("-c - create chat\n-r - request to chat");

        private static readonly string ChatId = "hymeck group";
        
        public static void CreateChat(string chatId)
        {
            var listener = new UdpClient(CommonPort2);
            IPEndPoint remoteEndpoint = null;
            Console.WriteLine($"=== Chat '{chatId}' is created ===");
            try
            {
                while (true)
                {
                    var requestData = listener.Receive(ref remoteEndpoint);
                    var request = requestData.Deserialize<GroupAccessRequest>();

                    Console.WriteLine($"{request.Username} requested to access group '{chatId}'");
                    // todo: confirm or deny

                    var response = new GroupAccessResponse { Result = GroupAccessResult.Allow};
                    
                    var serializedResponse = response.Serialize();
                    listener.Send(serializedResponse, serializedResponse.Length, remoteEndpoint);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            finally
            {
                listener.Close();
            }
        }

        public static void SendRequest(string username, string chatId)
        {
            var ip = IPAddress.Loopback;
            var requestListener = new UdpClient(CommonPort);
            try
            {
                var requestData = new GroupAccessRequest(chatId, username).Serialize();
                requestListener.Send(requestData, requestData.Length, ip.ToString(), CommonPort2);

                IPEndPoint remoteEndpoint = null;
                // var receiveThread = new Thread(_ =>
                // {
                    var response = requestListener.Receive(ref remoteEndpoint);
                    if (response.Length == 0)
                        return;
                    
                    var parsedResponse = response.Deserialize<GroupAccessResponse>();
                    Console.WriteLine($"Response: {parsedResponse}");
                // });
                //
                // receiveThread.Start();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            finally
            {
                requestListener.Close();
            }
        }
    }
}