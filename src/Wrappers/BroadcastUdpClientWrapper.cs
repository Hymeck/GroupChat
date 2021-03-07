using System.Net;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Setups <see cref="BaseUdpClientWrapper"/> with broadcast IP address.
    /// </summary>
    public class BroadcastUdpClientWrapper : BaseUdpClientWrapper
    {
        /// <inheritdoc/>
        public BroadcastUdpClientWrapper(int port, IPAddress localIpAddress = null) : 
            base(IPAddress.Broadcast, port, localIpAddress)
        {
        }
    }
}