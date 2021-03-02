using System;
using System.Net;
using System.Net.Sockets;
using GroupChat.Implementations.EventArgs;

namespace GroupChat.Implementations.Wrappers
{
    /// <summary>
    /// Adapts <see cref="UdpClient"/> to support multicast messaging.
    /// </summary>
    public class ChatClient
    {
        /// <summary>
        /// Holds multicast endpoint which contains multicast IP address and port.
        /// </summary>
        private readonly IPEndPoint _remoteEndpoint;

        /// <summary>
        /// Used to send and receive UDP datagrams.
        /// </summary>
        private readonly UdpClient _client;

        /// <summary>
        /// Indicates whether listening of receiving data has begun or not.
        /// </summary>
        private bool _beginReceived;

        /// <summary>
        /// Initializes a new instance of <see cref="ChatClient"/> class with specified <paramref name="multicastIpAddress"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// <remarks>If you don't know your real local IP address pass <paramref name="localIpAddress"/> as null.</remarks> 
        /// </summary>
        /// <param name="multicastIpAddress">The multicast IP address.</param>
        /// <param name="port">The port.</param>
        /// <param name="localIpAddress">The local IP address.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="multicastIpAddress"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="multicastIpAddress"/> is not a multicast IP address.</exception>
        public ChatClient(IPAddress multicastIpAddress, int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (multicastIpAddress == null)
                throw new ArgumentNullException(nameof(multicastIpAddress));

            if (!IsMulticastIpAddress(multicastIpAddress))
                throw new ArgumentException("specified IP address is not a multicast IP address.", nameof(multicastIpAddress));
            
            var localIp = localIpAddress ?? IPAddress.Any;
            _remoteEndpoint = new IPEndPoint(multicastIpAddress, port);

            _client = new UdpClient();
            // allow multiple clients in the same PC
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            var localEndpoint = new IPEndPoint(localIp, port);
            _client.Client.Bind(localEndpoint);
            
            // join to multicast ip address
            _client.JoinMulticastGroup(multicastIpAddress);
        }

        /// <summary>
        /// Setups <see cref="ChatClient"/> instance to receive data.
        /// <remarks>Call it once.</remarks>
        /// </summary>
        public void BeginReceive()
        {
            if (_beginReceived)
                return;

            // begin listen for received data
            _beginReceived = true;
            CallUdpClientBeginReceive();
        }

        /// <summary>
        /// Sends data to multicast address.
        /// </summary>
        /// <param name="data">/the data to be sent.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        public void SendMulticast(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Sent data is null or empty.");

            CallUdpClientBeginSend(data);
        }

        // todo: add message to send?
        /// <summary>
        /// Leaves multicast group, closes udp client.
        /// </summary>
        public void Close()
        {
            _client.DropMulticastGroup(_remoteEndpoint.Address);
            // todo: send message that i've left?
            _client.Close();
        }

        /// <summary>
        /// Invokes when datagram received.
        /// </summary>
        /// <param name="result">An object returned by a call of _client.BeginReceive.</param>
        private void ReceivedCallback(IAsyncResult result)
        {
            var senderEndpoint = new IPEndPoint(0, 0);
            var receivedData = _client.EndReceive(result, ref senderEndpoint);

            // trigger event if subscribed 
            DatagramReceived?.Invoke(this, new DatagramReceivedEventArgs(receivedData));
            // restart listening for UDP incomes
            CallUdpClientBeginReceive();
        }

        // call BeginReceive of udp client
        /// <summary>
        /// Invokes datagram receiving.
        /// </summary>
        private void CallUdpClientBeginReceive() => _client.BeginReceive(ReceivedCallback, state: null);
        
        /// <summary>
        /// Invokes when datagram sent.
        /// </summary>
        /// <param name="result">An object returned by a call of _client.BeginSend.</param>
        private void SentCallback(IAsyncResult result)
        {
            _client.EndSend(result);
        }

        // call BeginSend of udp client
        /// <summary>
        /// Invokes datagram sending.
        /// </summary>
        /// <param name="datagram">datagram to send.</param>
        private void CallUdpClientBeginSend(byte[] datagram) =>
            _client.BeginSend(datagram, datagram.Length, _remoteEndpoint, SentCallback, state: null);

        /// <summary>
        /// Determines whether specified IP address belongs to multicast IP address range.
        /// </summary>
        /// <param name="ipAddress">IP address.</param>
        /// <returns>true if specified IP address belongs to multicast IP address range; otherwise, false.</returns>
        private static bool IsMulticastIpAddress(IPAddress ipAddress)
        {
            // 4 high bits in first octet of multicast IP address always equal to 1110
            
            // get high octet
            var firstOctet = ipAddress.GetAddressBytes()[0];
            
            // get high 4 bits
            var fourHighBitsValue = (byte) (firstOctet >> 4);
            
            return fourHighBitsValue == 0b1110;
        }
        
        /// <summary>
        /// Event occuring when datagram is received.
        /// </summary>
        public event EventHandler<DatagramReceivedEventArgs> DatagramReceived;
    }
}