using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Provides broadcast messaging via UDP.
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