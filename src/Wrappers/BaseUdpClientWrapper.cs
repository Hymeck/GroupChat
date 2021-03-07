using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Configures <see cref="System.Net.Sockets.UdpClient"/> object to allow multiple clients in the same machine and sets destination and local endpoints.
    /// </summary>
    public class BaseUdpClientWrapper
    {
        #region fields

        /// <summary>
        /// Holds destination endpoint.
        /// </summary>
        protected readonly IPEndPoint _destinationEndpoint;

        protected readonly IPEndPoint _localEndpoint;
        
        /// <summary>
        /// Used to send and receive UDP datagrams.
        /// </summary>
        public readonly UdpClient UdpClient;

        #endregion fields

        #region properties

        public IPEndPoint DestinationEndpoint => _destinationEndpoint;

        public IPEndPoint LocalEndpoint => _localEndpoint;

        #endregion properties

        #region constructors

        /// <summary>
        /// Initializes a new instance with specified <paramref name="destinationIpAddress"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// <remarks>If you don't know your real local IP address pass <paramref name="localIpAddress"/> as null.</remarks> 
        /// </summary>
        /// <param name="destinationIpAddress">A destination IP address.</param>
        /// <param name="port">The port used to send and receive datagrams.</param>
        /// <param name="localIpAddress">A local IP address.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destinationIpAddress"/> is null.</exception>
        public BaseUdpClientWrapper(IPAddress destinationIpAddress, int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (destinationIpAddress == null)
                throw new ArgumentNullException(nameof(destinationIpAddress));

            var localIp = localIpAddress ?? IPAddress.Any;
            _destinationEndpoint = new IPEndPoint(destinationIpAddress, port);
            _localEndpoint = new IPEndPoint(localIp, port);
            
            UdpClient = new UdpClient();
            // allow multiple clients in the same machine
            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            UdpClient.Client.Bind(_localEndpoint);
        }

        #endregion constructors
        
    }
}