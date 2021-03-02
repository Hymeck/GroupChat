#nullable enable
using System;
using System.Net;
using System.Net.Sockets;

namespace GroupChat.Shared.Wrappers
{
    /// <summary>
    /// Wraps <see cref="UdpClient"/> to send and receive datagrams.
    /// </summary>
    public abstract class UdpClientWrapper
    {
        #region fields
        
        /// <summary>
        /// Holds multicast endpoint which contains multicast IP address and port.
        /// </summary>
        protected readonly IPEndPoint _remoteEndpoint;

        /// <summary>
        /// Used to send and receive UDP datagrams.
        /// </summary>
        public readonly UdpClient _client;
        
        /// <summary>
        /// Indicates whether listening of receiving data has begun or not.
        /// </summary>
        protected bool _beginReceived;
        
        #endregion fields

        #region constructor

        /// <summary>
        /// Initializes a new instance of <see cref="UdpClientWrapper"/> class with specified <paramref name="remoteIpAddress"/>, <paramref name="port"/> and <paramref name="localIpAddress"/>.
        /// <remarks>If you don't know your real local IP address pass <paramref name="localIpAddress"/> as null.</remarks> 
        /// </summary>
        /// <param name="remoteIpAddress">The multicast IP address.</param>
        /// <param name="port">The port.</param>
        /// <param name="localIpAddress">The local IP address.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than 0 or greater than 65535.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="remoteIpAddress"/> is null.</exception>
        protected UdpClientWrapper(IPAddress remoteIpAddress, int port, IPAddress localIpAddress = null)
        {
            if (port < ushort.MinValue || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (remoteIpAddress == null)
                throw new ArgumentNullException(nameof(remoteIpAddress));

            var localIp = localIpAddress ?? IPAddress.Any;
            _remoteEndpoint = new IPEndPoint(remoteIpAddress, port);

            _client = new UdpClient();
            // allow multiple clients in the same PC
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            
            // bind socket with local endpoint
            var localEndpoint = new IPEndPoint(localIp, port);
            _client.Client.Bind(localEndpoint);
        }

        #endregion constructor
        
        #region public methods

        /// <summary>
        /// Setups instance to receive data.
        /// <remarks>Call it once.</remarks>
        /// </summary>
        public virtual void BeginReceive()
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
        public virtual void Send(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Sent data is null or empty.");

            CallUdpClientBeginSend(data);
        }

        /// <summary>
        /// Closes udp client.
        /// </summary>
        public virtual void Close()
        {
            _client.Close();
        }
        
        #endregion public methods

        #region private methods

        /// <summary>
        /// Invokes datagram receiving.
        /// </summary>
        private void CallUdpClientBeginReceive() => _client.BeginReceive(ReceivedCallback, state: null);

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
        
        #endregion private methods
        
        /// <summary>
        /// Event occuring when datagram received.
        /// </summary>
        public event EventHandler<DatagramReceivedEventArgs> DatagramReceived;
    }
}