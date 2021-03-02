using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Client.Console
{
    /// <summary>
    /// Adapts <see cref="UdpClient"/> to support multicast messaging.
    /// </summary>
    public class ChatMulticastUdpClient
    {
        private readonly int _port;

        private IPEndPoint _remoteEndpoint;
        private IPEndPoint _localEndpoint;

        private IPAddress _multicastIp;
        private IPAddress _localIp;

        private UdpClient _client;

        private bool _beginReceived;

        /// <summary>
        /// Initializes a new instance of <see cref="ChatMulticastUdpClient"/> class with specified <paramref name="multicastIp"/>, <paramref name="port"/> and <paramref name="localIp"/>.
        /// If you don't know your real local IP address pass <paramref name="localIp"/> as null. 
        /// </summary>
        /// <param name="multicastIp">The multicast IP address.</param>
        /// <param name="port">The port.</param>
        /// <param name="localIp">The local IP address.</param>
        /// <exception cref="ArgumentNullException"><paramref name="multicastIp"/> is null.</exception>
        public ChatMulticastUdpClient(IPAddress multicastIp, int port, IPAddress localIp = null)
        {
            // init fields
            _multicastIp = multicastIp ??
                           throw new ArgumentNullException(nameof(multicastIp),
                               $"{nameof(multicastIp)} can't be null.");
            _port = port;
            _localIp = localIp ?? IPAddress.Any;

            // setup remote and local endpoints
            _remoteEndpoint = new IPEndPoint(_multicastIp, _port);
            _localEndpoint = new IPEndPoint(_localIp, _port);

            _client = new UdpClient();
            // allow multiple clients in the same PC
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            // bind socket with local endpoint
            _client.Client.Bind(_localEndpoint);
            // join to multicast ip address
            _client.JoinMulticastGroup(_multicastIp);
        }

        // begin receive udp incomes 
        /// <summary>
        /// Setups <see cref="ChatMulticastUdpClient"/> instance to receive data.
        /// Call it once.
        /// </summary>
        public void BeginReceive()
        {
            if (_beginReceived)
                return;

            // begin listen for received data
            CallUdpClientBeginReceive();
            _beginReceived = true;
        }

        /// <summary>
        /// Sends data to multicast address.
        /// </summary>
        /// <param name="data">/the data to be sent.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        public void SendMulticast(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Sent data is null.");

            CallUdpClientBeginSend(data);
        }

        // leave multicast group, close udp client
        public void Close()
        {
            _client.DropMulticastGroup(_multicastIp);
            // todo: send message that i've left?
            _client.Close();
        }

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
        private void CallUdpClientBeginReceive() => _client.BeginReceive(ReceivedCallback, state: null);

        private void SentCallback(IAsyncResult result)
        {
            _client.EndSend(result);
        }

        // call BeginSend of udp client
        private void CallUdpClientBeginSend(byte[] datagram) =>
            _client.BeginSend(datagram, datagram.Length, _remoteEndpoint, SentCallback, state: null);
        
        /// <summary>
        /// Event occuring when datagram is received.
        /// </summary>
        public event EventHandler<DatagramReceivedEventArgs> DatagramReceived;
    }
}