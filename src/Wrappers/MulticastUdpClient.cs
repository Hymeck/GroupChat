using System;
using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Provides multicast messaging via UDP.
    /// </summary>
    public class MulticastUdpClient : UdpClientWrapper
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentException"><paramref name="remoteIpAddress"/> is not a multicast IP address.</exception>
        public MulticastUdpClient(IPAddress remoteIpAddress, int port, IPAddress localIpAddress = null) : base(remoteIpAddress,
            port, localIpAddress)
        {
            if (!IsMulticastIpAddress(remoteIpAddress))
                throw new ArgumentException($"{nameof(remoteIpAddress)} is not a multicast IP address.", nameof(remoteIpAddress));
            // join to multicast ip address
            Client.JoinMulticastGroup(_remoteEndpoint.Address);
        }

        /// <summary>
        ///  <inheritdoc/>
        /// Leaves the multicast group. 
        /// </summary>
        public override void Close()
        {
            // todo: send message to group participants?
            Client.DropMulticastGroup(_remoteEndpoint.Address);
            base.Close();
        }

        /// <summary>
        /// Determines whether specified <paramref name="ipAddress"/> belongs to multicast IP address range.
        /// </summary>
        /// <param name="ipAddress">IP address.</param>
        /// <returns>true if <paramref name="ipAddress"/> is multicast IP address; otherwise, false.</returns>
        private static bool IsMulticastIpAddress(IPAddress ipAddress)
        {
            var firstOctet = ipAddress.GetAddressBytes()[0]; // get first octet
            var first4BitsValue = firstOctet >> 4; // get high 4 bits
            return first4BitsValue == 0b1110; // high 4 bits of first octet always equal to 1110
        }
    }
}