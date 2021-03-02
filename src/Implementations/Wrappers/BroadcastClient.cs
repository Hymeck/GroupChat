using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Implementations.Wrappers
{
    public class BroadcastClient
    {
        private readonly IPEndPoint _remoteEndpoint;
        
        private readonly UdpClient _client;

        public BroadcastClient(int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));
            
            var localIp = localIpAddress ?? IPAddress.Any;

            _remoteEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
            
            _client = new UdpClient();
            // allow multiple clients in the same PC
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            var localEndpoint = new IPEndPoint(localIp, port);
            _client.Client.Bind(localEndpoint);
        }
    }
}