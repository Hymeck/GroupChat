using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Entry
{
    class Program
    {
        static void Main(string[] args)
        {
            // ** user entity **
            // * as creator *
            // create group
            // listen to requests to join the group
            // confirm access request
            // deny access request
            // destroy group
            // notify participants that group is destroyed
            // * as participant *
            // send message to other participants
            // receive message from other participants
            // leave group

            // * utils *
            // choose username
            // finish session (close app)
            // release resources


            // ** additional stones under the water **

            // behaviour of leaving depends on whether participant is creator or not
            // if creator leaves then send notifications to participants and destroy group
            // if participant leaves then notify other participants and disconnect
            if (args.Length != 0)
            {
                if (args[0] == "-r")
                {
                    SendRequest();
                }
                
                else if (args[0] == "-c")
                {
                    CreateChat();
                }
                else
                    Console.WriteLine("-c - create chat\n-r - request to chat");
            }
            else
                Console.WriteLine("-c - create chat\n-r - request to chat");
        }

        public static Guid GenerateChatId() => Guid.NewGuid();

        private static readonly Guid ChatId = GenerateChatId();
        
        public static void CreateChat(int port = 9000)
        {
            var chatListener = new UdpClient(port);
            IPEndPoint requestIp = null;
            try
            {
                while (true)
                {
                    var requestData = chatListener.Receive(ref requestIp);
                    var parsedRequestData = Encoding.UTF8.GetString(requestData);
                    // todo: confirm or deny
                    Console.WriteLine($"Request data: {parsedRequestData}");

                    var response = Encoding.UTF8.GetBytes($"Welcome. Chat id: '{ChatId}'.\n" +
                                                          $"Your port: {requestIp.Port}");

                    chatListener.Send(response, response.Length, requestIp);
                    break;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            finally
            {
                chatListener.Close();
            }
        }

        public static void SendRequest(int remotePort = 9000)
        {
            var localhostIp = IPAddress.Loopback;
            var requestListener = new UdpClient();
            try
            {
                string username = "hymeck";
                var requestData = Encoding.UTF8.GetBytes(username);
                var remoteHost = new IPEndPoint(localhostIp, remotePort);
                requestListener.Send(requestData, requestData.Length, localhostIp.ToString(), remotePort);

                var response = requestListener.Receive(ref remoteHost);
                var parsedResponse = Encoding.UTF8.GetString(response);
                Console.WriteLine($"Response: {parsedResponse}");
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            finally
            {
                requestListener.Close();
            }
        }
    }
}