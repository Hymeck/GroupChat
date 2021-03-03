using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Configures <see cref="UdpClientWrapper"/> to broadcast messaging via UDP.
    /// </summary>
    public class BroadcastUdpClient : UdpClientWrapper
    {
        /// <inheritdoc/>
        public BroadcastUdpClient(int port, IPAddress localIpAddress = null) : 
            base(IPAddress.Broadcast, port, localIpAddress)
        {
        }
    }
}