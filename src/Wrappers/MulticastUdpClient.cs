using System;
using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Configures <see cref="UdpClientWrapper"/> to multicast messaging.
    /// </summary>
    public class MulticastUdpClient : UdpClientWrapper
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentException"><paramref name="destinationIpAddress"/> is not a multicast IP address.</exception>
        public MulticastUdpClient(IPAddress destinationIpAddress, int port, IPAddress localIpAddress = null) : base(destinationIpAddress,
            port, localIpAddress)
        {
            if (!IsMulticastIpAddress(destinationIpAddress))
                throw new ArgumentException($"{nameof(destinationIpAddress)} is not a multicast IP address.", nameof(destinationIpAddress));
            
            // join to multicast ip address
            UdpClient.JoinMulticastGroup(_destinationEndpoint.Address);
        }

        /// <summary>
        ///  <inheritdoc/>
        /// Leaves the multicast group. 
        /// </summary>
        public override void Dispose()
        {
            // After calling the DropMulticastGroup method,
            // the underlying socket sends an Internet Group Management Protocol (IGMP) packet
            // to the router, removing the router from the multicast group.
            UdpClient.DropMulticastGroup(_destinationEndpoint.Address);
            base.Dispose();
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