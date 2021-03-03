using System;
using System.Net;
using GroupChat.Extensions;
using GroupChat.Shared.Wrappers;

namespace GroupChat.Client.Console
{
    public class PeerceClient
    {
        public readonly string Username;
        private BroadcastUdpClient _broadcastUdpClient;
        private MulticastUdpClient _multicastUdpClient;

        public PeerceClient(string username, int port = 9000, IPAddress localIpAddress = null)
        {
            Username = username;
            
            ConfigureBroadcast(port, localIpAddress);
        }

        private void ConfigureBroadcast(int port, IPAddress localIpAddress = null)
        {
            if (_broadcastUdpClient != null)
            {
                _broadcastUdpClient.Close();
                _broadcastUdpClient = null;
            }

            _broadcastUdpClient = new BroadcastUdpClient(port, localIpAddress);
            _broadcastUdpClient.DatagramReceived += OnBroadcastDatagramReceived;
            _broadcastUdpClient.BeginReceive();
        }

        private void OnBroadcastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var message = e.Datagram.XmlDeserialize<Message>();
            System.Console.WriteLine(message);
        }

        private void ConfigureMulticast(IPAddress multicastIpAddress, int port = 9100, IPAddress localIpAddress = null)
        {
            if (_multicastUdpClient != null)
            {
                _multicastUdpClient.Close();
                _multicastUdpClient = null;
            }

            _multicastUdpClient = new MulticastUdpClient(multicastIpAddress, port, localIpAddress);
            _multicastUdpClient.DatagramReceived += OnMulticastDatagramReceived;
            _multicastUdpClient.BeginReceive();
        }

        private void OnMulticastDatagramReceived(object sender, DatagramReceivedEventArgs e)
        {
            var message = e.Datagram.XmlDeserialize<Message>();
            System.Console.WriteLine(message + " " + e.From);
        }
        
        private string _groupId;
        
        public void CreateGroup(string groupId, IPAddress multicastIpAddress, int port = 9100, IPAddress localIpAddress = null, bool notify = true)
        {
            // todo: checks
            
            _groupId = groupId;
            ConfigureMulticast(multicastIpAddress, port, localIpAddress);

            if (notify)
            {
                var message = new Message(Username, $"Group '{groupId}' is created. Join us!", DateTime.Now);
                _broadcastUdpClient.Send(message.XmlSerialize());
            }
        }

        public void JoinGroup(string groupId, IPAddress multicastIpAddress, int port = 9100, IPAddress localIpAddress = null)
        {
            _groupId = groupId;
            ConfigureMulticast(multicastIpAddress, port, localIpAddress);
        }
        
        public void SendMessage(string text)
        {
            if (_multicastUdpClient == null)
                return;

            var msg = new Message(Username, text, DateTime.Now);
            _multicastUdpClient.Send(msg.XmlSerialize());
        }
        
        public void Finish()
        {
            _broadcastUdpClient?.Close();
            _multicastUdpClient?.Close();
        }
    }
}